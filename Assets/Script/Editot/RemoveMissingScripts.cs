using UnityEngine;
using UnityEditor;

public class RemoveMissingScripts : EditorWindow
{
    [MenuItem("Tools/Remove Missing Scripts")]
    private static void RemoveScriptsFromSelected()
    {
        if (Selection.gameObjects.Length == 0)
        {
            Debug.LogWarning("⚠ 请选择至少一个 GameObject 进行清理！");
            return;
        }

        int totalRemoved = 0;

        foreach (GameObject obj in Selection.gameObjects)
        {
            totalRemoved += RemoveMissingScriptsRecursively(obj);
        }

        Debug.Log($"✅ 完成清理，共移除 {totalRemoved} 个失效脚本！");
    }

    private static int RemoveMissingScriptsRecursively(GameObject obj)
    {
        int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(obj);

        foreach (Transform child in obj.transform)
        {
            removed += RemoveMissingScriptsRecursively(child.gameObject);
        }

        return removed;
    }
}
