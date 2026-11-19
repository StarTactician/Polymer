using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class UnusedTexturesFinder
{
    [MenuItem("Tools/查找未使用的图片资源")]
    public static void FindUnusedTextures()
    {
        string[] allTextures = AssetDatabase.FindAssets("t:Texture");
        List<string> unused = new List<string>();

        foreach (string guid in allTextures)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string[] dependencies = AssetDatabase.GetDependencies(path);

            if (dependencies.Length == 1) // 只依赖自身，说明没人引用它
            {
                unused.Add(path);
            }
        }

        Debug.Log($"🔍 找到 {unused.Count} 张未被引用的贴图：");
        foreach (string path in unused)
        {
            Debug.Log($"🗑️ 未使用：{path}");
        }
    }
}
