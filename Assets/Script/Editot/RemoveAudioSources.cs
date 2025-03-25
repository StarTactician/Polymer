using UnityEngine;
using UnityEditor;

public class RemoveAudioSources : EditorWindow
{
    [MenuItem("Tools/批量移除 AudioSource")]
    public static void RemoveAudioSourcesFromSelected()
    {
        if (Selection.gameObjects.Length == 0)
        {
            Debug.LogWarning("⚠️ 请至少选择一个 GameObject");
            return;
        }

        int totalRemoved = 0;

        foreach (GameObject obj in Selection.gameObjects)
        {
            totalRemoved += RemoveFromHierarchy(obj);
        }

        Debug.Log($"✅ 已移除 {totalRemoved} 个 AudioSource 组件！");
    }

    private static int RemoveFromHierarchy(GameObject root)
    {
        int removedCount = 0;
        AudioSource[] audioSources = root.GetComponentsInChildren<AudioSource>(true);
        foreach (AudioSource audio in audioSources)
        {
            GameObject.DestroyImmediate(audio);
            removedCount++;
        }
        return removedCount;
    }
}
