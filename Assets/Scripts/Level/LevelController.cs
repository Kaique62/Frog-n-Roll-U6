using TMPro;
using UnityEngine;
using System.Collections;

public class LevelController : MonoBehaviour
{
    [Header("Score System")]
    public float score = 0;
    public TMP_Text scoreText;
    public TMP_Text scoreTextMultiplier;
    public float pointMultiplier;

    [Header("Popup Text")]
    public TMP_Text popupText;                 // World-space TextMeshPro object (assign in inspector)
    public float popupDuration = 1f;           // How long the popup stays visible
    private Coroutine popupCoroutine;

    [Header("Music")]
    public AudioSource musicAudioSource;

    public static bool gameStarted = false;

    void Start()
    {
        UpdateScoreUI();

        popupText.gameObject.SetActive(false);
        popupText.color = new Color(popupText.color.r, popupText.color.g, popupText.color.b, 0f);

        if (popupText != null)
        {
            popupText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (!gameStarted && Input.anyKey)
        {
            StartLevel();
        }
    }

    private void StartLevel()
    {
        gameStarted = true;

        if (musicAudioSource != null && !musicAudioSource.isPlaying)
        {
            musicAudioSource.volume = LoadSavedVolume();
            musicAudioSource.Play();
        }
    }

    public void AddScore(float delay)
    {
        float points = 0;
        string popupStr = "";
        Color popupColor = Color.white;

        if (delay >= 170)
        {
            points = 1000;
            popupStr = "Perfect";
            popupColor = Color.yellow;
            pointMultiplier += 0.3f;
        }
        else if (delay >= 150)
        {
            points = 700;
            popupStr = "Good";
            popupColor = Color.green;
            pointMultiplier += 0.1f;
        }
        else if (delay >= 120)
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
            scoreText.text = score.ToString();

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
}
