using UnityEngine;
using TMPro;  // Add this namespace for TextMeshPro

/// <summary>
/// Displays FPS and RAM usage in UI using TextMeshPro components.
/// </summary>
public class PerformanceCounter : MonoBehaviour
{
    /// <summary>
    /// Text component to display FPS.
    /// </summary>
    [Header("UI References")]
    public TMP_Text fpsText;

    /// <summary>
    /// Text component to display RAM usage.
    /// </summary>
    public TMP_Text ramText;

    /// <summary>
    /// Update frequency in seconds for FPS and RAM display.
    /// </summary>
    [Header("Settings")]
    public float updateInterval = 0.5f;

    private float timeSinceLastUpdate = 0f;
    private int frameCount = 0;
    private float fps = 0f;

    /// <summary>
    /// Updates FPS and RAM usage displays at set intervals.
    /// </summary>
    void Update()
    {
        timeSinceLastUpdate += Time.deltaTime;
        frameCount++;

        if (timeSinceLastUpdate >= updateInterval)
        {
            fps = frameCount / timeSinceLastUpdate;
            timeSinceLastUpdate = 0f;
            frameCount = 0;

            if (fpsText != null)
                fpsText.text = "FPS: " + Mathf.Floor(fps).ToString();
        }

        if (ramText != null)
        {
            float totalMemory = System.GC.GetTotalMemory(false) / (1024f * 1024f);
            ramText.text = "RAM: " + totalMemory.ToString("F2") + " MB";
        }
    }
}
