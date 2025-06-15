using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Displays and updates the current time and progress of background music,
/// and optionally pulses the time text in sync with the song's BPM.
/// </summary>
public class MusicTimer : MonoBehaviour
{
    public enum PulseType { None, Fade, Scale }

    [Header("References")]
    public AudioSource musicAudioSource;
    public TextMeshProUGUI timeText;
    public Slider progressSlider;

    [Header("Pulse Settings")]
    public PulseType pulseType = PulseType.None;
    public float songBPM = 120f;            // Beats per minute
    public float pulseStrength = 1.2f;      // Scale/alpha multiplier
    public float pulseDuration = 0.3f;      // Total time of the pulse (in and out)

    private static MusicTimer instance;

    private float musicLength;
    private float currentTime;
    private float beatInterval;
    private float nextBeatTime;

    private Coroutine pulseCoroutine;

    public static MusicTimer Instance => instance;
    public float CurrentTime => currentTime;
    public float TotalTime => musicLength;

    private void Awake()
    {
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
            beatInterval = 60f / songBPM;
            nextBeatTime = beatInterval;

            UpdateUI();
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

            if (pulseType != PulseType.None && currentTime >= nextBeatTime)
            {
                nextBeatTime += beatInterval;

                if (pulseCoroutine != null)
                    StopCoroutine(pulseCoroutine);

                pulseCoroutine = StartCoroutine(PulseText());
            }
        }
    }

    public void DebugTime()
    {
        int min = Mathf.FloorToInt(currentTime / 60f);
        int sec = Mathf.FloorToInt(currentTime % 60f);
        int ms = Mathf.FloorToInt((currentTime * 1000f) % 1000f);

        Debug.Log("Ação Realizada em: " + $"{min}:{sec}:{ms}");
    }

    private void UpdateUI()
    {
        int min = Mathf.FloorToInt(currentTime / 60f);
        int sec = Mathf.FloorToInt(currentTime % 60f);
        int ms = Mathf.FloorToInt((currentTime * 1000f) % 1000f);
        string currentFormatted = $"{min:00}:{sec:00}.{ms:000}";

        int totalMin = Mathf.FloorToInt(musicLength / 60f);
        int totalSec = Mathf.FloorToInt(musicLength % 60f);
        string totalFormatted = $"{totalMin:00}:{totalSec:00}";

        if (timeText != null)
            timeText.text = $"{currentFormatted} / {totalFormatted}";

        if (progressSlider != null)
            progressSlider.value = currentTime;
    }

    private IEnumerator PulseText()
    {
        if (timeText == null) yield break;

        float elapsed = 0f;
        float halfDuration = pulseDuration / 2f;

        switch (pulseType)
        {
            case PulseType.Fade:
                Color originalColor = timeText.color;
                float targetAlpha = originalColor.a * pulseStrength;

                // Fade in
                while (elapsed < halfDuration)
                {
                    float t = elapsed / halfDuration;
                    float newAlpha = Mathf.Lerp(originalColor.a, targetAlpha, t);
                    timeText.color = new Color(originalColor.r, originalColor.g, originalColor.b, newAlpha);
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                // Fade out
                elapsed = 0f;
                while (elapsed < halfDuration)
                {
                    float t = elapsed / halfDuration;
                    float newAlpha = Mathf.Lerp(targetAlpha, originalColor.a, t);
                    timeText.color = new Color(originalColor.r, originalColor.g, originalColor.b, newAlpha);
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                timeText.color = originalColor;
                break;

            case PulseType.Scale:
                Vector3 originalScale = timeText.transform.localScale;
                Vector3 targetScale = originalScale * pulseStrength;

                // Scale up
                while (elapsed < halfDuration)
                {
                    float t = elapsed / halfDuration;
                    timeText.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                // Scale down
                elapsed = 0f;
                while (elapsed < halfDuration)
                {
                    float t = elapsed / halfDuration;
                    timeText.transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                timeText.transform.localScale = originalScale;
                break;
        }
    }
}
