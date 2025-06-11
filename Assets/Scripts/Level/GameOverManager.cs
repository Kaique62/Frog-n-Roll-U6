using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [Header("Game Over Menu")]
    public RectTransform menuContainer;
    public float animDuration = 0.5f;
    public Vector2 hiddenPos = new Vector2(0, -635);
    public Vector2 shownPos = new Vector2(0, 135);

    [Header("Background Fade")]
    public Image backgroundImage;
    public float fadeDuration = 0.5f;
    private Color baseBackgroundColor;

    [Header("Audio Slowdown")]
    public float audioSlowdownDuration = 2f;

    private Coroutine animCoroutine;

    /// <summary>
    /// Initializes UI to hidden state and sets background transparency to 0.
    /// </summary>
    void Start()
    {
        if (menuContainer != null)
            menuContainer.anchoredPosition = hiddenPos;

        baseBackgroundColor = backgroundImage.color;
        backgroundImage.color = new Color(baseBackgroundColor.r, baseBackgroundColor.g, baseBackgroundColor.b, 0f);
    }

    /// <summary>
    /// Displays the Game Over menu:
    /// - Pauses game time.
    /// - Gradually slows down and fades out audio.
    /// - Animates UI menu and background fade.
    /// </summary>
    public void ShowGameOver()
    {
        Time.timeScale = 0f;

        var levelController = FindObjectOfType<LevelController>();
        if (levelController != null && levelController.musicAudioSource != null)
            StartCoroutine(SlowDownAudio(levelController.musicAudioSource, audioSlowdownDuration));
        else
            AudioListener.pause = true;

        if (animCoroutine != null)
            StopCoroutine(animCoroutine);

        animCoroutine = StartCoroutine(AnimateMenu(true));
        StartCoroutine(FadeBackground(0f, 190f / 255f));
    }

    /// <summary>
    /// Gradually reduces audio pitch and volume to zero over <paramref name="duration"/> seconds, then stops the audio.
    /// Uses unscaled time to function while the game is paused.
    /// </summary>
    private IEnumerator SlowDownAudio(AudioSource audioSource, float duration)
    {
        float startPitch = audioSource.pitch;
        float startVolume = audioSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            audioSource.pitch = Mathf.Lerp(startPitch, 0f, t);
            audioSource.volume = Mathf.Lerp(startVolume, 0f, t);
            yield return null;
        }

        audioSource.Stop();
        audioSource.pitch = 0f;
        audioSource.volume = 0f;
    }

    /// <summary>
    /// Animates the Game Over menu sliding in or out.
    /// </summary>
    /// <param name="show">True to show menu, false to hide.</param>
    private IEnumerator AnimateMenu(bool show)
    {
        float elapsed = 0f;
        Vector2 start = show ? hiddenPos : shownPos;
        Vector2 end = show ? shownPos : hiddenPos;

        while (elapsed < animDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = 1f - Mathf.Pow(1f - elapsed / animDuration, 2f); // ease-out
            menuContainer.anchoredPosition = Vector2.Lerp(start, end, t);
            yield return null;
        }

        menuContainer.anchoredPosition = end;
    }

    /// <summary>
    /// Fades the background image alpha from <paramref name="fromAlpha"/> to <paramref name="toAlpha"/> over time.
    /// </summary>
    private IEnumerator FadeBackground(float fromAlpha, float toAlpha)
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = 1f - Mathf.Pow(1f - elapsed / fadeDuration, 2f); // ease-out
            float a = Mathf.Lerp(fromAlpha, toAlpha, t);
            backgroundImage.color = new Color(baseBackgroundColor.r, baseBackgroundColor.g, baseBackgroundColor.b, a);
            yield return null;
        }

        backgroundImage.color = new Color(baseBackgroundColor.r, baseBackgroundColor.g, baseBackgroundColor.b, toAlpha);
    }

    /// <summary>
    /// Resets the current scene, resumes time and audio.
    /// </summary>
    public void ResetScene()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Loads the Main Menu scene, resumes time and audio.
    /// </summary>
    public void GoToHome()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene("MainMenu");
    }
}
