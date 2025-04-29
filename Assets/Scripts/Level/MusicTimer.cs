using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MusicTimer : MonoBehaviour
{
    public AudioSource musicAudioSource;
    public TextMeshProUGUI timeText;
    public Slider progressSlider; // <-- Use Slider instead of Image

    private static MusicTimer instance;

    private float musicLength;
    private float currentTime;

    public static MusicTimer Instance => instance;

    public float CurrentTime => currentTime;
    public float TotalTime => musicLength;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        instance = this;
    }

    private void Start()
    {
        if (musicAudioSource != null)
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

    private void UpdateUI()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);
        int milliseconds = Mathf.FloorToInt((currentTime * 1000f) % 1000f);

        int totalMinutes = Mathf.FloorToInt(musicLength / 60f);
        int totalSeconds = Mathf.FloorToInt(musicLength % 60f);

        string currentFormatted = string.Format("{0:00}:{1:00}.{2:000}", minutes, seconds, milliseconds);
        string totalFormatted = string.Format("{0:00}:{1:00}", totalMinutes, totalSeconds);

        if (timeText != null)
            timeText.text = $"{currentFormatted} / {totalFormatted}";

        if (progressSlider != null)
            progressSlider.value = currentTime;
    }
}
