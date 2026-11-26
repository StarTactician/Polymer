using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

// Serializes one building claim across the two existing save files on the game thread.
internal static class BuildingClaimJournal
{
    private const string PendingName = "BuildingClaim.pending.json";
    private const string PlayerName = "PlayerData.json";
    private const string WarehouseName = "BuildingData.json";
    private static readonly Encoding Utf8 = new UTF8Encoding(false, true);

    [Serializable]
    public sealed class FileChange
    {
        public string beforeHash;
        public string afterHash;
        public string afterBase64;
    }

    [Serializable]
    public sealed class PendingClaim
    {
        public int version;
        public string id;
        public FileChange player;
        public FileChange warehouse;
    }

    internal static void RequireNoPendingClaim(string directory)
    {
        if (HasPendingClaim(directory))
            throw new InvalidDataException("A building claim must be recovered before saving. Pending journal was retained.");
    }

    private static bool HasPendingClaim(string directory)
    {
        try
        {
            // File.Exists can also return false on an access error. Only absence is safe here.
            File.GetAttributes(Path.Combine(directory, PendingName));
            return true;
        }
        catch (FileNotFoundException) { return false; }
        catch (DirectoryNotFoundException) { return false; }
    }

    internal static void Commit(string directory, string playerJson, string warehouseJson)
    {
        RequireNoPendingClaim(directory);
        var record = new PendingClaim
        {
            version = 1,
            id = Guid.NewGuid().ToString("N"),
            player = CreateChange(Path.Combine(directory, PlayerName), playerJson),
            warehouse = CreateChange(Path.Combine(directory, WarehouseName), warehouseJson)
        };
        string journalPath = Path.Combine(directory, PendingName);
        WriteAtomic(journalPath, Utf8.GetBytes(JsonUtility.ToJson(record, false)), false);
        Apply(directory, journalPath, record);
    }

    internal static bool Recover(string directory)
    {
        if (!HasPendingClaim(directory)) return false;
        string journalPath = Path.Combine(directory, PendingName);
        PendingClaim record;
        try
        {
            record = JsonUtility.FromJson<PendingClaim>(File.ReadAllText(journalPath, Utf8));
        }
        catch (Exception error) when (error is ArgumentException || error is InvalidOperationException)
        {
            throw new InvalidDataException("Invalid building claim journal. Original files were retained.", error);
        }
        Apply(directory, journalPath, record);
        return true;
    }

    private static void Apply(string directory, string journalPath, PendingClaim record)
    {
        if (record == null || record.version != 1 || !Guid.TryParseExact(record.id, "N", out _))
            throw new InvalidDataException("Invalid building claim journal. Original files were retained.");

        byte[] playerAfter = ReadAfter(record.player);
        byte[] warehouseAfter = ReadAfter(record.warehouse);
        string playerPath = Path.Combine(directory, PlayerName);
        string warehousePath = Path.Combine(directory, WarehouseName);

        // Validate both originals before changing either one.
        bool replacePlayer = NeedsReplacement(playerPath, record.player);
        bool replaceWarehouse = NeedsReplacement(warehousePath, record.warehouse);
        if (replacePlayer) WriteAtomic(playerPath, playerAfter, true);
        if (replaceWarehouse) WriteAtomic(warehousePath, warehouseAfter, true);
        if (Hash(File.ReadAllBytes(playerPath)) != record.player.afterHash ||
            Hash(File.ReadAllBytes(warehousePath)) != record.warehouse.afterHash)
            throw new InvalidDataException("Building claim did not verify. Pending journal was retained.");
        File.Delete(journalPath);
    }

    private static FileChange CreateChange(string path, string afterJson)
    {
        byte[] after = Utf8.GetBytes(afterJson);
        return new FileChange
        {
            beforeHash = Hash(File.ReadAllBytes(path)),
            afterHash = Hash(after),
            afterBase64 = Convert.ToBase64String(after)
        };
    }

    private static byte[] ReadAfter(FileChange change)
    {
        if (change == null || !IsHash(change.beforeHash) || !IsHash(change.afterHash) ||
            string.IsNullOrEmpty(change.afterBase64))
            throw new InvalidDataException("Incomplete building claim journal. Original files were retained.");
        byte[] data;
        try { data = Convert.FromBase64String(change.afterBase64); }
        catch (FormatException error) { throw new InvalidDataException("Invalid building claim payload.", error); }
        if (Hash(data) != change.afterHash)
            throw new InvalidDataException("Building claim payload checksum failed. Original files were retained.");
        return data;
    }

    private static bool NeedsReplacement(string path, FileChange change)
    {
        string current = Hash(File.ReadAllBytes(path));
        if (current == change.afterHash) return false;
        if (current == change.beforeHash) return true;
        throw new InvalidDataException($"Building claim conflicts with {Path.GetFileName(path)}. Pending journal was retained.");
    }

    private static bool IsHash(string value)
    {
        if (value == null || value.Length != 44) return false;
        try { return Convert.FromBase64String(value).Length == 32; }
        catch (FormatException) { return false; }
    }

    private static string Hash(byte[] bytes)
    {
        using (SHA256 sha = SHA256.Create()) return Convert.ToBase64String(sha.ComputeHash(bytes));
    }

    private static void WriteAtomic(string path, byte[] data, bool replace)
    {
        string temp = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            using (var file = new FileStream(temp, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                file.Write(data, 0, data.Length);
                file.Flush(true);
            }
            if (replace) File.Replace(temp, path, null);
            else File.Move(temp, path);
        }
        finally
        {
            // The pending journal, not the per-write scratch file, is the recovery evidence.
            try { File.Delete(temp); }
            catch (IOException error) { Debug.LogException(error); }
            catch (UnauthorizedAccessException error) { Debug.LogException(error); }
        }
    }
}
