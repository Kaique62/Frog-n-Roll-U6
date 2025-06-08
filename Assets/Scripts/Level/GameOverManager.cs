using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [Header("Menu de Game Over")]
    public RectTransform menuContainer;
    public float animDuration = 0.5f;
    public Vector2 hiddenPos = new Vector2(0, -635);
    public Vector2 shownPos = new Vector2(0, 135);

    [Header("Background Escurecedor")]
    public Image backgroundImage;
    public float fadeDuration = 0.5f;
    private Color backgroundTargetColor;

    // --- MUDANÇA 1: Nova variável para controlar a duração do efeito de áudio ---
    [Header("Efeito de Áudio")]
    public float audioSlowdownDuration = 2f; // Duração em segundos para o áudio desacelerar

    private Coroutine animCoroutine;

    void Start()
    {
        if (menuContainer != null)
            menuContainer.anchoredPosition = hiddenPos;

        backgroundTargetColor = backgroundImage.color;
        Color startColor = backgroundTargetColor;
        startColor.a = 0f;
        backgroundImage.color = startColor;
    }

    public void ShowGameOver()
    {
        // Pausa o tempo do jogo, mas não o áudio ainda
        Time.timeScale = 0f;
        
        // --- MUDANÇA 2: A linha AudioListener.pause = true; foi REMOVIDA daqui ---
        // Se pausássemos o áudio agora, o efeito de slowdown não seria ouvido.

        // --- MUDANÇA 3: Lógica para encontrar e desacelerar o áudio do LevelController ---
        LevelController levelController = FindObjectOfType<LevelController>();
        if (levelController != null && levelController.musicAudioSource != null)
        {
            StartCoroutine(SlowDownAndFadeOutAudio(levelController.musicAudioSource, audioSlowdownDuration));
        }
        else
        {
            // Se não encontrar a música, apenas pausa o áudio global como antes
            AudioListener.pause = true;
        }

        // As animações da UI continuam normalmente
        if (animCoroutine != null) StopCoroutine(animCoroutine);
        animCoroutine = StartCoroutine(AnimateMenu(true));
        StartCoroutine(FadeBackground(0f, 190f / 255f));
    }

    // --- MUDANÇA 4: Nova Coroutine para o efeito de áudio ---
    private IEnumerator SlowDownAndFadeOutAudio(AudioSource audioSource, float duration)
    {
        float startPitch = audioSource.pitch;
        float startVolume = audioSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; // Essencial usar unscaledDeltaTime pois o tempo está parado
            float t = elapsed / duration;

            // Interpola o pitch e o volume de seus valores iniciais até zero
            audioSource.pitch = Mathf.Lerp(startPitch, 0f, t);
            audioSource.volume = Mathf.Lerp(startVolume, 0f, t);

            yield return null;
        }

        // Garante que tudo termine em zero e para a música de vez
        audioSource.volume = 0f;
        audioSource.pitch = 0f;
        audioSource.Stop();
        
        // Opcional: redefinir o volume global do listener se necessário, mas parar a fonte é mais limpo.
        // AudioListener.pause = true;
    }


    private IEnumerator AnimateMenu(bool show)
    {
        float elapsed = 0f;
        Vector2 start = show ? hiddenPos : shownPos;
        Vector2 end = show ? shownPos : hiddenPos;

        while (elapsed < animDuration)
        {
            float t = elapsed / animDuration;
            t = 1f - Mathf.Pow(1f - t, 2f); // ease-out
            menuContainer.anchoredPosition = Vector2.Lerp(start, end, t);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        menuContainer.anchoredPosition = end;
    }

    private IEnumerator FadeBackground(float fromAlpha, float toAlpha)
    {
        float elapsed = 0f;
        Color baseColor = backgroundTargetColor;

        while (elapsed < fadeDuration)
        {
            float t = elapsed / fadeDuration;
            t = 1f - Mathf.Pow(1f - t, 2f); // ease-out
            float a = Mathf.Lerp(fromAlpha, toAlpha, t);
            backgroundImage.color = new Color(baseColor.r, baseColor.g, baseColor.b, a);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        backgroundImage.color = new Color(baseColor.r, baseColor.g, baseColor.b, toAlpha);
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
}