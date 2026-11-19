using UnityEngine;
using UnityEditor;


public class RemoveParticleSystems : EditorWindow
{
    [MenuItem("Tools/批量移除 ParticleSystem")]
    public static void RemoveParticleSystemsFromSelected()
    {
        if (Selection.gameObjects.Length == 0)
        {
            Debug.LogWarning("⚠️ 请至少选择一个 GameObject！");
            return;
        }

        int totalRemoved = 0;

        foreach (GameObject obj in Selection.gameObjects)
        {
            totalRemoved += RemoveFromHierarchy(obj);
        }

        Debug.Log($"✅ 已移除 {totalRemoved} 个 ParticleSystem 组件所在的 GameObject！");
    }

    private static int RemoveFromHierarchy(GameObject root)
    {
        int removedCount = 0;
        ParticleSystem[] particleSystems = root.GetComponentsInChildren<ParticleSystem>(true);
        foreach (ParticleSystem ps in particleSystems)
        {
            if (ps != null)
            {
                //GameObject.DestroyImmediate(ps.gameObject);
                ps.gameObject.SetActive(false);
                removedCount++;
            }
        }
        return removedCount;
    }
}
