using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Comprehensive music timer with millisecond precision, UI display, and beat pulse effects
/// </summary>
public class MusicTimer : MonoBehaviour
{
    public enum PulseType { None, Fade, Scale }

    [Header("Audio Reference")]
    public AudioSource musicAudioSource;

    [Header("UI References")]
    public TextMeshProUGUI timeText;
    public Slider progressSlider;

    [Header("Timing Settings")]
    public float songBPM = 120f;
    public float startOffset = 0f; // Seconds to offset timing

    [Header("Pulse Settings")]
    public PulseType pulseType = PulseType.None;
    public float pulseStrength = 1.2f;
    public float pulseDuration = 0.1f;

    private static MusicTimer instance;
    private float musicLength;
    private float currentTime;
    private float beatInterval;
    private float nextBeatTime;
    private Coroutine pulseCoroutine;

    // Public accessors
    public static MusicTimer Instance => instance;
    public float CurrentTime => currentTime;
    public float TotalTime => musicLength;
    public bool IsPlaying => musicAudioSource.isPlaying;

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
        InitializeTimer();
    }

    private void Update()
    {
        if (musicAudioSource != null && musicAudioSource.isPlaying)
        {
            UpdateTimer();
            HandleBeatPulse();
        }
    }

    private void InitializeTimer()
    {
        if (musicAudioSource != null && musicAudioSource.clip != null)
        {
            musicLength = musicAudioSource.clip.length;
            beatInterval = 60f / songBPM;
            nextBeatTime = startOffset + beatInterval;

            if (progressSlider != null)
            {
                progressSlider.minValue = 0f;
                progressSlider.maxValue = musicLength;
            }
        }
    }

    private void UpdateTimer()
    {
        currentTime = musicAudioSource.time;
        UpdateUI();
    }

    private void HandleBeatPulse()
    {
        if (pulseType != PulseType.None && currentTime >= nextBeatTime)
        {
            nextBeatTime += beatInterval;
            TriggerPulse();
        }
    }

    private void UpdateUI()
    {
        if (timeText != null)
        {
            timeText.text = FormatTime(currentTime) + " / " + FormatTime(musicLength, false);
        }

        if (progressSlider != null)
        {
            progressSlider.value = currentTime;
        }
    }

    public string FormatTime(float time, bool includeMilliseconds = true)
    {
        int min = Mathf.FloorToInt(time / 60f);
        int sec = Mathf.FloorToInt(time % 60f);
        
        if (includeMilliseconds)
        {
            int ms = Mathf.FloorToInt((time * 1000f) % 1000f);
            return $"{min:00}:{sec:00}.{ms:000}";
        }
        return $"{min:00}:{sec:00}";
    }

    public float GetCurrentTime()
    {
        return currentTime;
    }

    public int DebugTime()
    {
        Debug.Log("Current Time: " + FormatTime(currentTime));
        return Mathf.FloorToInt(currentTime);
    }

    private void TriggerPulse()
    {
        if (pulseCoroutine != null)
            StopCoroutine(pulseCoroutine);

        pulseCoroutine = StartCoroutine(PulseEffect());
    }

    private IEnumerator PulseEffect()
    {
        if (timeText == null || pulseType == PulseType.None) 
            yield break;

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
                    elapsed += Time.deltaTime;
                    float t = elapsed / halfDuration;
                    float newAlpha = Mathf.Lerp(originalColor.a, targetAlpha, t);
                    timeText.color = new Color(originalColor.r, originalColor.g, originalColor.b, newAlpha);
                    yield return null;
                }

                // Fade out
                elapsed = 0f;
                while (elapsed < halfDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / halfDuration;
                    float newAlpha = Mathf.Lerp(targetAlpha, originalColor.a, t);
                    timeText.color = new Color(originalColor.r, originalColor.g, originalColor.b, newAlpha);
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
                    elapsed += Time.deltaTime;
                    float t = elapsed / halfDuration;
                    timeText.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
                    yield return null;
                }

                // Scale down
                elapsed = 0f;
                while (elapsed < halfDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / halfDuration;
                    timeText.transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
                    yield return null;
                }

                timeText.transform.localScale = originalScale;
                break;
        }
    }

    public void Play()
    {
        if (musicAudioSource != null && !musicAudioSource.isPlaying)
        {
            musicAudioSource.Play();
        }
    }

    public void Pause()
    {
        if (musicAudioSource != null && musicAudioSource.isPlaying)
        {
            musicAudioSource.Pause();
        }
    }

    public void Stop()
    {
        if (musicAudioSource != null)
        {
            musicAudioSource.Stop();
            currentTime = 0f;
            UpdateUI();
        }
    }

    public void SetTime(float time)
    {
        currentTime = Mathf.Clamp(time, 0f, musicLength);
        if (musicAudioSource != null)
        {
            musicAudioSource.time = currentTime;
        }
        UpdateUI();
    }
}