using UnityEngine;

public class ActionTimer : MonoBehaviour
{
    public float targetTimeInSeconds = 30f; // Set this in the Inspector

    private MusicTimer musicTimer;
    private bool hasWarned = false;

    void Start()
    {
        musicTimer = FindObjectOfType<MusicTimer>();
        if (musicTimer == null)
        {
            Debug.LogWarning("[ActionTimer] No MusicTimer found in the scene.");
        }
    }

    void Update()
    {
        if (musicTimer == null || hasWarned)
            return;

        float currentTime = musicTimer.CurrentTime;

        if (currentTime >= targetTimeInSeconds - 2f)
        {
            hasWarned = true;

            int minutes = Mathf.FloorToInt(currentTime / 60);
            int seconds = Mathf.FloorToInt(currentTime % 60);
            int milliseconds = Mathf.FloorToInt((currentTime * 1000) % 1000);

            Debug.Log($"[ActionTimer] 2 seconds before target time ({targetTimeInSeconds}s): {minutes:00}:{seconds:00}.{milliseconds:000}");
        }
    }

    public void LogCurrentMusicTime()
    {
        if (musicTimer == null)
        {
            Debug.LogWarning("[ActionTimer] Cannot log time - MusicTimer is missing.");
            return;
        }

        float time = musicTimer.CurrentTime;
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        int milliseconds = Mathf.FloorToInt((time * 1000) % 1000);

        Debug.Log($"[ActionTimer] Music Time: {minutes:00}:{seconds:00}.{milliseconds:000}");
    }
}
