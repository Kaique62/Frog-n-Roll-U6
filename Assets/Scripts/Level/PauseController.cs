using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public static bool isPaused = false;

    [Header("Menu de Pausa")]
    public RectTransform pauseMenu;
    public float animDuration = 0.5f;
    public Vector2 hiddenPos = new Vector2(0, -635);
    public Vector2 shownPos = new Vector2(0, 135);

    [Header("Contagem de Retorno")]
    public TextMeshProUGUI countdownText;
    public Vector2 countdownStartPos = new Vector2(0, -200);
    public Vector2 countdownEndPos = new Vector2(0, 100);
    public float countdownAnimDuration = 0.4f;

    [Header("Background Escurecedor")]
    public Image backgroundImage;
    public float fadeDuration = 0.5f;
    private Color backgroundTargetColor;

    private Coroutine animCoroutine;

    void Start()
    {
        pauseMenu.anchoredPosition = hiddenPos;
        countdownText.text = "";
        backgroundTargetColor = backgroundImage.color;
        Color startColor = backgroundTargetColor;
        startColor.a = 0f;
        backgroundImage.color = startColor;
    }
    
    bool gameStarted = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isPaused)
                PauseGame();
            else
                ResumeGame();
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        isPaused = true;
        AudioListener.pause = true;

        if (animCoroutine != null) StopCoroutine(animCoroutine);
        animCoroutine = StartCoroutine(AnimatePanel(pauseMenu, pauseMenu.anchoredPosition, shownPos));
        StartCoroutine(FadeBackground(0f, 190f / 255f));
    }

    public void ResumeGame()
    {
        isPaused = false;

        if (animCoroutine != null) StopCoroutine(animCoroutine);
        animCoroutine = StartCoroutine(AnimatePanel(pauseMenu, pauseMenu.anchoredPosition, hiddenPos));
        StartCoroutine(CountdownResume());
    }

    private IEnumerator AnimatePanel(RectTransform panel, Vector2 from, Vector2 to)
    {
        float elapsed = 0f;

        while (elapsed < animDuration)
        {
            float t = elapsed / animDuration;
            t = 1f - Mathf.Pow(1f - t, 2f); // ease-out
            panel.anchoredPosition = Vector2.Lerp(from, to, t);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        panel.anchoredPosition = to;
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

    private IEnumerator CountdownResume()
    {
        string[] countdownSteps = { "3", "2", "1", "GO!!" };
        countdownText.text = "";

        foreach (string step in countdownSteps)
        {
            countdownText.text = step;
            countdownText.rectTransform.anchoredPosition = countdownStartPos;

            float elapsed = 0f;
            while (elapsed < countdownAnimDuration)
            {
                float t = elapsed / countdownAnimDuration;
                t = 1f - Mathf.Pow(1f - t, 2f); // ease-out
                countdownText.rectTransform.anchoredPosition = Vector2.Lerp(countdownStartPos, countdownEndPos, t);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            countdownText.rectTransform.anchoredPosition = countdownEndPos;
            yield return new WaitForSecondsRealtime(0.5f);
        }

        countdownText.text = "";
        StartCoroutine(FadeBackground(190f / 255f, 0f));
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    public void ResetScene()
    {
        isPaused = false; // <- Garante que sempre comece "despausado"
        pauseMenu.anchoredPosition = hiddenPos;
        countdownText.text = "";
        backgroundTargetColor = backgroundImage.color;
        Color startColor = backgroundTargetColor;
        startColor.a = 0f;
        backgroundImage.color = startColor;
        Time.timeScale = 1f; // Garante que o tempo volte ao normal
        AudioListener.pause = false; // Garante que o som volte
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // Recarrega a cena atual
    }

    public void GoToHome()
    {
        isPaused = false; // Garantir que o jogo não fique pausado
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene("MainMenu");
    }
}
