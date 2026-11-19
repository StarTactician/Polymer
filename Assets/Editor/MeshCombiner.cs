using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class MeshCombiner
{
    [MenuItem("Tools/合并选中物体的 Mesh（支持大模型）")]
    static void CombineMeshes()
    {
        if (Selection.activeGameObject == null)
        {
            Debug.LogWarning("请先选择一个包含多个 Mesh 的父物体！");
            return;
        }

        GameObject root = Selection.activeGameObject;
        MeshFilter[] meshFilters = root.GetComponentsInChildren<MeshFilter>();

        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        for (int i = 0; i < meshFilters.Length; i++)
        {
            if (meshFilters[i].sharedMesh == null) continue;
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
        }

        GameObject combinedGO = new GameObject(root.name + "_Combined");
        MeshFilter mf = combinedGO.AddComponent<MeshFilter>();
        MeshRenderer mr = combinedGO.AddComponent<MeshRenderer>();

        Mesh newMesh = new Mesh();
        newMesh.indexFormat = IndexFormat.UInt32; // ✅ 允许超过65535个顶点
        newMesh.CombineMeshes(combine);

        mf.sharedMesh = newMesh;

        // 尝试用第一块 MeshRenderer 的材质（统一材质的前提下）
        MeshRenderer firstMR = meshFilters[0].GetComponent<MeshRenderer>();
        if (firstMR != null)
            mr.sharedMaterial = firstMR.sharedMaterial;

        Debug.Log($"✅ 合并完成，共合并 {combine.Length} 个子物体，总顶点数 {newMesh.vertexCount}");
    }
}

