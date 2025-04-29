using UnityEngine;
using TMPro;  // Add this namespace for TextMeshPro

public class PerformanceCounter : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text fpsText; // TextMeshPro Text to display FPS
    public TMP_Text ramText; // TextMeshPro Text to display RAM usage

    [Header("Settings")]
    public float updateInterval = 0.5f; // Update frequency for FPS and RAM data

    private float timeSinceLastUpdate = 0f;
    private int frameCount = 0;
    private float fps = 0f;

    void Update()
    {
        // Update frame count and time
        timeSinceLastUpdate += Time.deltaTime;
        frameCount++;

        if (timeSinceLastUpdate >= updateInterval)
        {
            // Calculate FPS
            fps = frameCount / timeSinceLastUpdate;

            // Reset counters for next interval
            timeSinceLastUpdate = 0f;
            frameCount = 0;

            // Update FPS text
            if (fpsText != null)
                fpsText.text = "FPS: " + Mathf.Floor(fps).ToString();
        }

        // Update RAM usage text
        if (ramText != null)
        {
            float totalMemory = System.GC.GetTotalMemory(false) / (1024f * 1024f); // in MB
            ramText.text = "RAM: " + totalMemory.ToString("F2") + " MB";
        }
    }
}
