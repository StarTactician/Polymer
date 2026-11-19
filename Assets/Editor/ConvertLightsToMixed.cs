using UnityEditor;
using UnityEngine;

public class ConvertLightsToMixed : EditorWindow
{
    [MenuItem("Tools/光照/将选中物体中的 Realtime 光源改为 Mixed")]
    static void ConvertSelectedLights()
    {
        int modifiedCount = 0;

        foreach (GameObject go in Selection.gameObjects)
        {
            Light[] lights = go.GetComponentsInChildren<Light>(true);

            foreach (Light light in lights)
            {
                if (light.lightmapBakeType == LightmapBakeType.Realtime)
                {
                    Undo.RecordObject(light, "Convert Realtime Light to Mixed");
                    light.lightmapBakeType = LightmapBakeType.Mixed;
                    modifiedCount++;
                }
            }
        }

        Debug.Log($"✅ 已将 {modifiedCount} 个 Realtime 光源改为 Mixed。");
    }
}
