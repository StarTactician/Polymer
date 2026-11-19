using UnityEngine;
using UnityEditor;

public class RemoveColliders : EditorWindow
{
    [MenuItem("Tools/批量工具/移除 Collider 组件")]
    static void Init()
    {
        RemoveColliders window = (RemoveColliders)GetWindow(typeof(RemoveColliders));
        window.titleContent = new GUIContent("移除 Collider");
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("移除 Collider 工具", EditorStyles.boldLabel);
        GUILayout.Label("此操作将从选中物体及其所有子物体中移除所有 Collider 类型组件。", EditorStyles.wordWrappedLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("移除选中物体上的 Collider"))
        {
            RemoveAllCollidersFromSelection();
        }
    }

    private void RemoveAllCollidersFromSelection()
    {
        int totalCount = 0;

        foreach (GameObject go in Selection.gameObjects)
        {
            Collider[] colliders = go.GetComponentsInChildren<Collider>(true);
            foreach (Collider col in colliders)
            {
                Undo.DestroyObjectImmediate(col); // 支持撤销
                totalCount++;
            }
        }

        Debug.Log($"已移除 {totalCount} 个 Collider 组件。");
    }
}
