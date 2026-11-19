using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

public class PerformanceOverlay : MonoBehaviour
{
    float deltaTime = 0.0f;

    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    void OnGUI()
    {
        int w = Screen.width, h = Screen.height;

        GUIStyle style = new GUIStyle();
        Rect rect = new Rect(10, 10, w, h * 2 / 100);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = h * 2 / 50;
        style.normal.textColor = Color.white;

        float msec = deltaTime * 1000.0f;
        float fps = 1.0f / deltaTime;

        int drawCalls = UnityStats.drawCalls;
        int batches = UnityStats.batches;
        //int particles = UnityStats.particleCount;
        int verts = UnityStats.vertices;

        string text = string.Format(
            $"FPS: {fps:F1} ({msec:F1}ms)\n" +
            $"DrawCalls: {drawCalls}, Batches: {batches}\n" +
            //$"Verts: {verts}, Particles: {particles}\n" +
            $"RAM: {Profiler.GetTotalReservedMemoryLong() / 1024 / 1024} MB");

        GUI.Label(rect, text, style);
    }
}
