using UnityEngine;
using UnityEditor;

public class ShowDrawCallInfo
{
    [MenuItem("分析/显示选中物体的 Draw Call")]
    public static void ShowDrawCallEstimate()
    {
        if (Selection.activeGameObject == null)
        {
            Debug.LogWarning("请先选中一个物体！");
            return;
        }

        GameObject go = Selection.activeGameObject;
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>(true);

        int drawCallCount = 0;
        foreach (Renderer renderer in renderers)
        {
            if (renderer.sharedMaterials != null)
                drawCallCount += renderer.sharedMaterials.Length;
        }

        Debug.Log($"【{go.name}】估算 Draw Call 数量：{drawCallCount}（共找到 {renderers.Length} 个 Renderer）");
    }
}
