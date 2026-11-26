using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Linq;

public class BuildingManager
{
    private static BuildingManager Inst;
    private string buildingDataFilePath = Application.persistentDataPath + "/BuildingData.json";
    private BuildingList buildingList;
    public List<BuildingData> unlockBuilding=new List<BuildingData>();
    private Dictionary<int, Transform> cameraPosDict = new Dictionary<int, Transform>();
    private readonly Dictionary<int, GameObject> buildingModels = new Dictionary<int, GameObject>();

    private static string BuildingKey(string name)
    {
        if (name == "LandingPadLevel1") return "LandingPad_Level1";
        if (name == "LandingPadLevel2") return "LandingPad_Level2";
        return name;
    }

    public static BuildingManager GetInst()
    {
        if (Inst == null)
        {
            Inst = new BuildingManager();
        }
        return Inst;
    }
    public BuildingManager()
    {
        Inst = this;
    }
    public void OnStart()
    {
        LoadBuildingData();
        GenerateBuildingsOnLoad();
    }
    public int AmountUmlockCount()
    {
        if (buildingList == null || buildingList.buildings == null)
        {
            OnStart();
            Debug.Log("场景已切换，重新初始化数据");
        }
        return buildingList.buildings.Count(b => b.isUnlocked);
    }
    public int TotalBuildingProgress()
    {
        if (buildingList == null || buildingList.buildings == null)
        {
            OnStart();
            Debug.Log("场景已切换，重新初始化数据");
        }
        // Legacy data stores building progress under unlockReward.
        return buildingList.buildings.Where(b => b.isUnlocked)
            .Sum(b => b.progressValue != 0 ? b.progressValue : b.unlockReward);
    }

    private const int LunarBiosphereId = 13;
    private const string LunarBiosphereName = "LunarBiosphere";

    private void RestoreMissingLunarBiosphere()
    {
        if (buildingList.buildings.Any(b => b != null && (b.id == LunarBiosphereId || b.name == LunarBiosphereName)))
            return;

        TextAsset defaults = Resources.Load<TextAsset>("DefaultBuildingData");
        if (defaults == null)
            return;

        BuildingList defaultsList = JsonUtility.FromJson<BuildingList>(defaults.text);
        BuildingData biosphere = defaultsList?.buildings?.Find(b =>
            b != null && b.id == LunarBiosphereId && b.name == LunarBiosphereName);
        if (biosphere != null)
            buildingList.buildings.Add(biosphere);
    }

    public void LoadBuildingData()
    {
        BuildingClaimJournal.Recover(Application.persistentDataPath);
        unlockBuilding.Clear();
        if (File.Exists(buildingDataFilePath))
        {
            string json = File.ReadAllText(buildingDataFilePath);
            buildingList = JsonUtility.FromJson<BuildingList>(json);
            if (buildingList == null || buildingList.buildings == null)
            {
                Debug.LogError("建筑数据为空，请检查 JSON 文件。");
                return;
            }
            RestoreMissingLunarBiosphere();
            Debug.Log("建筑数据加载成功！");
        }
        else
        {
            Debug.LogWarning("建筑数据文件不存在，加载默认建筑数据...");
            TextAsset resourcesBuildingData = Resources.Load<TextAsset>("DefaultBuildingData");
            if (resourcesBuildingData != null)
            {
                buildingList = JsonUtility.FromJson<BuildingList>(resourcesBuildingData.text);
                SaveBuildingData();
                Debug.Log("默认建筑数据加载成功！");
            }
            else
            {
                Debug.LogError("无法加载默认建筑数据！");
                buildingList = new BuildingList();
            }
        }
        RebuildUnlockedBuildings();
    }

    public void SaveBuildingData()
    {
        BuildingClaimJournal.RequireNoPendingClaim(Application.persistentDataPath);
        string json = JsonUtility.ToJson(buildingList, true);
        File.WriteAllText(buildingDataFilePath, json);
        Debug.Log("建筑数据已保存！");
    }
    public BuildingData GetBuildingByName(string factoryName)
    {
        foreach (BuildingData factory in unlockBuilding)
        {
            if (BuildingKey(factory.name) == BuildingKey(factoryName))
            {
                return factory;
            }
        }
        return null;
    }    
    public BuildingData GetBuildingByID(int factoryID)
    {
        foreach (BuildingData factory in unlockBuilding)
        {
            if (factory.id == factoryID)
            {
                return factory;
            }
        }
        return null;
    }

    /// <summary>
    /// 在初始化时，根据建筑的已解锁情况，在主界面的月球背景安放对应的建筑
    /// </summary>
    public void GenerateBuildingsOnLoad()
    {
        foreach (var pair in buildingModels)
        {
            if (pair.Value != null && !IsActiveBuilding(GetBuildingByID(pair.Key)))
            {
                pair.Value.SetActive(false);
                cameraPosDict.Remove(pair.Key);
            }
        }

        foreach (BuildingData building in unlockBuilding)
        {
            if (!IsActiveBuilding(building)) continue;
            if (buildingModels.TryGetValue(building.id, out GameObject existing) && existing != null)
            {
                existing.SetActive(true);
                Transform existingCamera = existing.transform.Find("CameraPos");
                if (existingCamera != null) cameraPosDict[building.id] = existingCamera;
                continue;
            }

            GameObject buildingPrefab = Resources.Load<GameObject>($"Building/{BuildingKey(building.name)}");
            if (buildingPrefab != null)
            {
                GameObject buildingObj = GameObject.Instantiate(buildingPrefab);
                buildingModels[building.id] = buildingObj;
                buildingObj.transform.position = new Vector3(building.position.x, building.position.y, building.position.z);
                buildingObj.transform.localScale = new Vector3(building.scale.x, building.scale.y, building.scale.z);
                buildingObj.transform.rotation = Quaternion.Euler(building.rotation.x, building.rotation.y, building.rotation.z);
                Debug.Log($"生成建筑: {building.name} at {building.position}");

                Transform camPos = buildingObj.transform.Find("CameraPos");//获取CameraPos位置
                if(building.type==0)
                {
                    Transform camPos2 = buildingObj.transform.Find("Process1Pos");//
                    if(!GameRoot.GetInst().MainCamera)
                    {
                        
                        Debug.Log("主摄像机没找到");
                        GameRoot.GetInst().FindCamera();
                    }
                    if (camPos2)
                        GameRoot.GetInst().MainCamera.transform.position = camPos2.transform.position;
                    else Debug.Log(building.name+ "还没有设置Process1Pos");
                }
                
                if (camPos != null) cameraPosDict[building.id] = camPos;
            }
            else
            {
                Debug.LogWarning($"未找到建筑预制体: {building.name}");
            }
        }
    }

    public Transform GetCameraPos(int id)
    {
        return cameraPosDict.TryGetValue(id, out Transform pos) ? pos : null;
    }

    /// <summary>
    /// 资源领取方法
    /// </summary>
    /// <param name="factoryName"></param>
    public bool CollectBuildingResources(string factoryName) => CollectBuildingResources(factoryName, DateTime.Now);

    private void RecoverBeforeBuildingUse()
    {
        if (!BuildingClaimJournal.Recover(Application.persistentDataPath)) return;
        // A failed claim can be retried without restarting the scene. Refresh both in-memory views.
        LoadBuildingData();
        SaveManager.GetInst().LoadPlayerDate();
    }

    private bool CollectBuildingResources(string factoryName, DateTime now)
    {
        try { RecoverBeforeBuildingUse(); }
        catch (Exception error) when (error is IOException || error is UnauthorizedAccessException || error is InvalidDataException)
        {
            Debug.LogException(error);
            return false;
        }
        BuildingData factory = GetBuildingByName(factoryName);
        if (!IsActiveBuilding(factory) || factory.type != 1) return false;

        if (factory.incomeType != "EU" && factory.incomeType != "coin" && factory.incomeType != "PU")
            return false;

        bool productionChanged = ApplyBuildingProduction(now);
        int amount = factory.currentReserves;
        if (amount <= 0)
        {
            if (productionChanged) SaveBuildingData();
            return false;
        }

        bool isPU = factory.incomeType == "PU";
        PlayerData player = SaveManager.GetInst().CreatePlayerSaveData();
        if (isPU) player.PU += amount;
        else player.coin += amount;

        string warehouseAfter;
        try
        {
            factory.currentReserves = 0;
            warehouseAfter = JsonUtility.ToJson(buildingList, true);
        }
        finally { factory.currentReserves = amount; }

        try
        {
            BuildingClaimJournal.Commit(Application.persistentDataPath, JsonUtility.ToJson(player, true), warehouseAfter);
        }
        catch (Exception error) when (error is IOException || error is UnauthorizedAccessException || error is InvalidDataException)
        {
            Debug.LogException(error);
            return false;
        }

        SaveManager.GetInst().ApplyCommittedBuildingClaim(player);
        factory.currentReserves = 0;

        var reward = new GetCoinPanel
        {
            rewardPuShown = factory.incomeType == "PU",
            PUNumber = factory.incomeType == "PU" ? amount : 0,
            rewardCoinShown = factory.incomeType == "EU",
            CoinNumber = factory.incomeType == "EU" ? amount : 0
        };
        try
        {
            AchievementManager.GetInst().CheckAchievProgress(isPU ? "PU" : "coin");
            UIManager.GetInst().Push(reward);
        }
        catch (Exception error)
        {
            Debug.LogException(error);
        }
        return true;
    }

    public void UpdateBuildingProduction() => UpdateBuildingProduction(DateTime.Now);

    private void UpdateBuildingProduction(DateTime now)
    {
        RecoverBeforeBuildingUse();
        if (ApplyBuildingProduction(now)) SaveBuildingData();
    }

    private bool ApplyBuildingProduction(DateTime now)
    {
        bool changed = false;
        foreach (BuildingData building in unlockBuilding)
        {
            if (!IsActiveBuilding(building) || building.type != 1 || building.producInterval <= 0 || building.income <= 0) continue;
            if (string.IsNullOrEmpty(building.nextGenerateTime))
            {
                building.nextGenerateTime = now.AddSeconds(building.producInterval).ToString("yyyy-MM-dd HH:mm:ss");
                changed = true;
                continue;
            }

            DateTime next = DateTime.Parse(building.nextGenerateTime);
            if (now < next) continue;

            long intervalTicks = TimeSpan.TicksPerSecond * building.producInterval;
            long batches = 1 + (now.Ticks - next.Ticks) / intervalTicks;
            long availableSpace = Math.Max(0L, (long)building.maxReserves - building.currentReserves);
            long added = (long)Math.Min(availableSpace, batches * (decimal)building.income);
            building.currentReserves = (int)(building.currentReserves + added);
            // Expired batches advance the clock even when the warehouse cannot store them.
            building.nextGenerateTime = next.AddTicks(batches * intervalTicks).ToString("yyyy-MM-dd HH:mm:ss");
            changed = true;
        }
        return changed;
    }

    /// <summary>
    /// 解锁指定建筑时调用
    /// </summary>
    /// <param name="buildingName"></param>
    public void UnlockBuilding(string buildingName)
    {
        string key = BuildingKey(buildingName);
        if (string.IsNullOrEmpty(key)) return;
        UnlockBuilding(buildingList.buildings.Find(b => BuildingKey(b.name) == key));
    }

    public void UnlockBuilding(int id, string legacyName)
    {
        BuildingData building = buildingList.buildings.Find(b => b.id == id);
        UnlockBuilding(building ?? buildingList.buildings.Find(b => BuildingKey(b.name) == BuildingKey(legacyName)));
    }

    private void UnlockBuilding(BuildingData building)
    {
        if (building == null || building.isUnlocked) return;
        building.isUnlocked = true;
        RebuildUnlockedBuildings();
        SaveBuildingData();
        AchievementManager.GetInst().CheckAchievProgress("building");
        if (SceneManager.GetActiveScene().name == "StartScene") GenerateBuildingsOnLoad();
    }

    public bool IsActiveBuilding(BuildingData building)
    {
        return building != null && building.isUnlocked && !string.IsNullOrEmpty(building.name) &&
            !IsReplacedByUpgrade(building, unlockBuilding);
    }

    public bool IsReplacedByUpgrade(BuildingData building, List<BuildingData> unlockedList)
    {
        foreach (var b in unlockedList)
        {
            if (b.isUnlocked && !string.IsNullOrEmpty(b.upgradeBy) && BuildingKey(b.upgradeBy) == BuildingKey(building.name))
                return true;
        }
        return false;
    }
    public bool IsUnlock(string buildName)
    {
        BuildingData building;
        building=GetBuildingByName(buildName);
        if(building!=null)
        {
            return building.isUnlocked;
        }
        else
        {
            Debug.Log(buildName+"建筑未解锁");
            return false;
        }
    }    
    public bool IsUnlock(int ID)
    {
        BuildingData building;
        building=GetBuildingByID(ID);
        if(building!=null)
        {
            return building.isUnlocked;
        }
        else
        {
            Debug.Log(ID+"建筑未解锁");
            return false;
        }
    }
    /// <summary>
    /// 获取已经解锁的工厂类建筑
    /// </summary>
    private void RebuildUnlockedBuildings()
    {
        unlockBuilding.Clear();
        if (buildingList?.buildings != null)
            unlockBuilding.AddRange(buildingList.buildings.Where(b => b != null && b.isUnlocked));
    }

}


