using UnityEngine;
using TMPro;
using SQLite; // Import sqlite-net

public class FPSCounter : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text fpsText; // TextMeshPro Text to display FPS

    [Header("Settings")]
    public float updateInterval = 0.5f; // Update frequency for FPS

    private float timeSinceLastUpdate = 0f;
    private int frameCount = 0;
    private float fps = 0f;

    void Start()
    {
        Debug.Log("Caminho do banco: " + Application.persistentDataPath);
        // Check saved setting to determine if FPS counter should be visible
        string path = System.IO.Path.Combine(Application.persistentDataPath, "config.db");
        var db = new SQLiteConnection(path); // Use sqlite-net connection

        var config = db.Table<Configuracao>()
                       .FirstOrDefault(c => c.Chave == "mostrarFps");

        if (config != null && config.Valor == "True")
        {
            fpsText.gameObject.SetActive(true);
        }
        else
        {
            fpsText.gameObject.SetActive(false);
        }
    }

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
    }
}
