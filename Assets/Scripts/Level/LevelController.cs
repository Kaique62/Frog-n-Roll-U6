using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class LevelController : MonoBehaviour
{
    [Header("Score System")]
    public float score = 0;
    public TMP_Text scoreText;
    public TMP_Text scoreTextMultiplier;
    public float pointMultiplier = 1f;
    public float maxMultiplier = 5f;

    [Header("Popup Text")]
    public TMP_Text popupText;
    public float popupDuration = 1f;
    private Coroutine popupCoroutine;

    [Header("Music")]
    public AudioSource musicAudioSource;

    [Header("Blink Effect")]
    public List<float> blinkTimes = new List<float>();
    public Image blackPanelImage;
    public float blinkThreshold = 0.05f;
    public float blinkDuration = 0.1f;
    public float fadeInDuration = 0.05f;
    public float fadeOutDuration = 0.1f;
    public bool keepLastBlink = true;

    private List<float> remainingBlinkTimes;
    private int activeBlinks = 0;
    private Coroutine currentFadeCoroutine;
    private bool isLastBlink = false;
    private MusicTimer musicTimer;

    public static bool gameStarted = false;

    void Start()
    {
        musicTimer = FindObjectOfType<MusicTimer>();

        gameStarted = false;
        ResetScoreSystem();
        UpdateScoreUI();

        if (popupText != null)
            popupText.gameObject.SetActive(false);

        remainingBlinkTimes = new List<float>(blinkTimes);

        if (blackPanelImage != null)
        {
            blackPanelImage.color = new Color(0, 0, 0, 0);
            blackPanelImage.gameObject.SetActive(true);
        }
    }

    void Update()
    {
        if (!gameStarted && Input.anyKey)
        {
            StartLevel();
        }

        if (gameStarted)
        {
            CheckBlinkTimes();
        }
    }

    private void StartLevel()
    {
        gameStarted = true;

        if (musicAudioSource != null && !musicAudioSource.isPlaying)
        {
            musicAudioSource.volume = LoadSavedVolume();
            musicAudioSource.pitch = 1f;
            musicAudioSource.Play();
        }
    }

    private void CheckBlinkTimes()
    {
        if (musicTimer == null) return;

        float currentTime = musicTimer.GetCurrentTime();

        for (int i = remainingBlinkTimes.Count - 1; i >= 0; i--)
        {
            float blinkTime = remainingBlinkTimes[i];
            float timeDiff = currentTime - blinkTime;

            if (Mathf.Abs(timeDiff) <= blinkThreshold)
            {
                isLastBlink = (remainingBlinkTimes.Count == 1 && keepLastBlink);
                StartCoroutine(BlinkCoroutine());
                remainingBlinkTimes.RemoveAt(i);
            }
            else if (timeDiff > blinkThreshold)
            {
                remainingBlinkTimes.RemoveAt(i);
            }
        }
    }

    private IEnumerator BlinkCoroutine()
    {
        activeBlinks++;

        if (activeBlinks == 1 && blackPanelImage != null)
        {
            if (currentFadeCoroutine != null)
                StopCoroutine(currentFadeCoroutine);

            currentFadeCoroutine = StartCoroutine(FadeImage(0f, 1f, fadeInDuration));
        }

        yield return new WaitForSeconds(blinkDuration);

        activeBlinks--;

        if (activeBlinks == 0 && blackPanelImage != null && !isLastBlink)
        {
            if (currentFadeCoroutine != null)
                StopCoroutine(currentFadeCoroutine);

            currentFadeCoroutine = StartCoroutine(FadeImage(1f, 0f, fadeOutDuration));
        }
    }

    private IEnumerator FadeImage(float startAlpha, float targetAlpha, float duration)
    {
        float elapsed = 0f;
        Color currentColor = blackPanelImage.color;
        currentColor.a = startAlpha;
        blackPanelImage.color = currentColor;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            currentColor.a = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            blackPanelImage.color = currentColor;
            yield return null;
        }

        currentColor.a = targetAlpha;
        blackPanelImage.color = currentColor;
    }

    public void AddScore(float currentTime, ActionTimer timer)
    {
        float transitionDuration = timer.colorChangeDuration;
        float delay = currentTime - timer.elapsedTime; // raw delay
        float error = Mathf.Abs(delay);

        Debug.Log($"{transitionDuration} - {timer.elapsedTime} = {error}");

        float points = 0;
        string popupStr = "";
        Color popupColor = Color.white;

        // If delay is negative, automatically miss
        if (delay < -10)
        {
            points = 300;
            popupStr = "Too Late!";
            popupColor = Color.gray;
            pointMultiplier = 1f;
        }
        else if (error <= 30)
        {
            points = 1000; 
            popupStr = "Perfect"; 
            popupColor = Color.yellow; 
            pointMultiplier += 0.3f;
        }
        else if (error <= 70)
        {
            points = 700; 
            popupStr = "Good"; 
            popupColor = Color.green; 
            pointMultiplier += 0.1f;
        }
        else if (error <= 120)
        {
            points = 500; 
            popupStr = "Bad"; 
            popupColor = Color.red; 
            pointMultiplier = 1f;
        }
        else
        {
            points = 300; 
            popupStr = "Miss"; 
            popupColor = Color.gray; 
            pointMultiplier = 1f;
        }

        points *= pointMultiplier;
        score += points;
        UpdateScoreUI();
        ShowPopup(popupStr, popupColor);
    }



    private void ShowPopup(string text, Color color)
    {
        if (popupText == null) return;

        popupText.text = text;
        popupText.color = new Color(color.r, color.g, color.b, 1f);
        popupText.gameObject.SetActive(true);

        if (popupCoroutine != null)
            StopCoroutine(popupCoroutine);

        popupCoroutine = StartCoroutine(FadeOutPopup());
    }

    private IEnumerator FadeOutPopup()
    {
        float elapsed = 0f;
        Color startColor = popupText.color;

        while (elapsed < popupDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / popupDuration);
            popupText.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        popupText.gameObject.SetActive(false);
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = score.ToString("F0");

        if (scoreTextMultiplier != null)
            scoreTextMultiplier.text = $"X{pointMultiplier:F1}";
    }

    private float LoadSavedVolume()
    {
        string savedVolume = ConfigManager.Read("volume_MusicVolume");
        if (float.TryParse(savedVolume, out float volume))
            return Mathf.Clamp01(volume);

        return 1f;
    }

    public void ResetScoreSystem()
    {
        score = 0;
        pointMultiplier = 1f;
        UpdateScoreUI();
    }

    public void ResetBlinkSystem()
    {
        remainingBlinkTimes = new List<float>(blinkTimes);
        if (blackPanelImage != null)
        {
            blackPanelImage.color = new Color(0, 0, 0, 0);
        }
        isLastBlink = false;
    }
}
