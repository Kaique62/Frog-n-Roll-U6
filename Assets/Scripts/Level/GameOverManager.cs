using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform menuContainer;
    public Image backgroundImage;

    [Header("Animation Settings")]
    public float animDuration = 0.5f;
    public float fadeDuration = 0.5f;
    public Vector2 hiddenPos = new Vector2(0, -635);
    public Vector2 shownPos = new Vector2(0, 135);
    
    [Header("Audio Effect")]
    public float audioSlowdownDuration = 2f;

    private Color baseBackgroundColor;

    void Awake()
    {
        // LINHA REMOVIDA: Esta linha movia o menu para uma posição escondida.
        if (menuContainer != null)
             menuContainer.anchoredPosition = hiddenPos;

        if (backgroundImage != null)
        {
            baseBackgroundColor = backgroundImage.color;
            // LINHA REMOVIDA: Esta linha tornava o fundo transparente ao iniciar.
            backgroundImage.color = new Color(baseBackgroundColor.r, baseBackgroundColor.g, baseBackgroundColor.b, 0f);
        }
    }

    public void ShowGameOver()
    {
        Time.timeScale = 0f;

        LevelController levelController = FindObjectOfType<LevelController>();
        if (levelController != null && levelController.musicAudioSource != null)
        {
            StartCoroutine(SlowDownAudio(levelController.musicAudioSource));
        }
        else
        {
            AudioListener.pause = true;
        }

        StartCoroutine(AnimateMenu(true));
        StartCoroutine(FadeBackground(0f, 190f / 255f));
    }

    private IEnumerator SlowDownAudio(AudioSource audioSource)
    {
        float startPitch = audioSource.pitch;
        float startVolume = audioSource.volume;
        float elapsed = 0f;

        while (elapsed < audioSlowdownDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / audioSlowdownDuration;
            audioSource.pitch = Mathf.Lerp(startPitch, 0f, t);
            audioSource.volume = Mathf.Lerp(startVolume, 0f, t);
            yield return null;
        }

        audioSource.Stop();
    }

    public void ResetScene()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToHome()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene("MainMenu");
    }

    private IEnumerator AnimateMenu(bool show)
    {
        float elapsed = 0f;
        Vector2 start = show ? hiddenPos : shownPos;
        Vector2 end = show ? shownPos : hiddenPos;
        while (elapsed < animDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = 1f - Mathf.Pow(1f - elapsed / animDuration, 2f);
            menuContainer.anchoredPosition = Vector2.Lerp(start, end, t);
            yield return null;
        }
        menuContainer.anchoredPosition = end;
    }

    private IEnumerator FadeBackground(float fromAlpha, float toAlpha)
    {
        float elapsed = 0f;
        Color startColor = backgroundImage.color;
        Color endColor = new Color(baseBackgroundColor.r, baseBackgroundColor.g, baseBackgroundColor.b, toAlpha);
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            backgroundImage.color = Color.Lerp(startColor, endColor, elapsed / fadeDuration);
            yield return null;
        }
        backgroundImage.color = endColor;
    }
}