using UnityEditor;
using UnityEngine;
using TMPro;

public class FontReplacer : EditorWindow
{
    public TMP_FontAsset newFontAsset; // 要替换的字体
    public TMP_FontAsset oldFontAsset; // 要查找的旧字体（可选）

    [MenuItem("Tools/Replace Fonts")]
    public static void ShowWindow()
    {
        GetWindow<FontReplacer>("Font Replacer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Replace Fonts in Scene", EditorStyles.boldLabel);

        newFontAsset = (TMP_FontAsset)EditorGUILayout.ObjectField("New Font Asset", newFontAsset, typeof(TMP_FontAsset), false);
        oldFontAsset = (TMP_FontAsset)EditorGUILayout.ObjectField("Old Font Asset (Optional)", oldFontAsset, typeof(TMP_FontAsset), false);

        if (GUILayout.Button("Replace Fonts"))
        {
            ReplaceFontsInScene();
        }
    }

    private void ReplaceFontsInScene()
    {
        if (newFontAsset == null)
        {
            Debug.LogError("New Font Asset cannot be null.");
            return;
        }

        TextMeshProUGUI[] textComponents = FindObjectsOfType<TextMeshProUGUI>(true); // 查找所有 TextMeshProUGUI
        int count = 0;

        foreach (var text in textComponents)
        {
            if (oldFontAsset == null || text.font == oldFontAsset)
            {
                text.font = newFontAsset; // 替换字体
                count++;
            }
        }

        Debug.Log($"Replaced fonts on {count} TextMeshProUGUI components.");
    }
}
