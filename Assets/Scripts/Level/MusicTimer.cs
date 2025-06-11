using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays and updates the current time and progress of background music.
/// </summary>
public class MusicTimer : MonoBehaviour
{
    [Header("References")]
    public AudioSource musicAudioSource;
    public TextMeshProUGUI timeText;
    public Slider progressSlider;

    private static MusicTimer instance;

    private float musicLength;
    private float currentTime;

    /// <summary>
    /// Singleton instance access.
    /// </summary>
    public static MusicTimer Instance => instance;

    /// <summary>
    /// Current playback time of the music.
    /// This is important for the action timer system, so it knows the hit delay
    /// It's also important for the progress bar, and time labels and stuff
    /// </summary>
    public float CurrentTime => currentTime;

    /// <summary>
    /// Total duration of the music.
    /// </summary>
    public float TotalTime => musicLength;

    private void Awake()
    {
        // Ensure singleton pattern
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void Start()
    {
        if (musicAudioSource != null && musicAudioSource.clip != null)
        {
            musicLength = musicAudioSource.clip.length;

            if (progressSlider != null)
            {
                progressSlider.minValue = 0f;
                progressSlider.maxValue = musicLength;
            }
        }
    }

    private void Update()
    {
        if (musicAudioSource != null && musicAudioSource.isPlaying)
        {
            currentTime = musicAudioSource.time;
            UpdateUI();
        }
    }

    /// <summary>
    /// Updates the timer text and progress bar.
    /// </summary>
    private void UpdateUI()
    {
        // Current time formatting
        int min = Mathf.FloorToInt(currentTime / 60f);
        int sec = Mathf.FloorToInt(currentTime % 60f);
        int ms = Mathf.FloorToInt((currentTime * 1000f) % 1000f);
        string currentFormatted = $"{min:00}:{sec:00}.{ms:000}";

        // Total time formatting
        int totalMin = Mathf.FloorToInt(musicLength / 60f);
        int totalSec = Mathf.FloorToInt(musicLength % 60f);
        string totalFormatted = $"{totalMin:00}:{totalSec:00}";

        // Update UI
        if (timeText != null)
            timeText.text = $"{currentFormatted} / {totalFormatted}";

        if (progressSlider != null)
            progressSlider.value = currentTime;
    }
}
