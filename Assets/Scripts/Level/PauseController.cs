using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public static bool isPaused = false;

    [Header("UI References")]
    public RectTransform menuContainer;
    public Image backgroundImage;
    public TextMeshProUGUI countdownText;

    [Header("Animation Settings")]
    public float animDuration = 0.5f;
    public float fadeDuration = 0.5f;
    public Vector2 hiddenPos = new Vector2(0, -635);
    public Vector2 shownPos = new Vector2(0, 135);
    
    [Header("Countdown Settings")]
    public Vector2 countdownStartPos = new Vector2(0, -200);
    public Vector2 countdownEndPos = new Vector2(0, 100);
    public float countdownAnimDuration = 0.4f;

    private Color baseBackgroundColor;

    void Start()
    {
        isPaused = false;
        Time.timeScale = 1f;

        // LINHA REMOVIDA: Esta linha movia o menu para uma posição escondida.
        if (menuContainer != null) menuContainer.anchoredPosition = hiddenPos;
        if (countdownText != null) countdownText.text = "";
        
        if(backgroundImage != null)
        {
            baseBackgroundColor = backgroundImage.color;
            // LINHA REMOVIDA: Esta linha tornava o fundo transparente ao iniciar.
            backgroundImage.color = new Color(baseBackgroundColor.r, baseBackgroundColor.g, baseBackgroundColor.b, 0f);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        isPaused = true;
        AudioListener.pause = true;

        StartCoroutine(AnimateMenu(true));
        StartCoroutine(FadeBackground(0f, 190f / 255f));
    }

    public void ResumeGame()
    {
        isPaused = false;
        StartCoroutine(AnimateMenu(false));
        StartCoroutine(CountdownToResume());
    }

    private IEnumerator CountdownToResume()
    {
        string[] steps = { "3", "2", "1", "GO!!" };
        if (countdownText != null) countdownText.text = "";
        foreach (string step in steps)
        {
            if (countdownText != null)
            {
                countdownText.text = step;
                yield return new WaitForSecondsRealtime(0.7f);
            }
        }
        if (countdownText != null) countdownText.text = "";
        StartCoroutine(FadeBackground(backgroundImage.color.a, 0f));
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    public void ResetScene()
    {
        isPaused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToHome()
    {
        isPaused = false;
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
        Color startColor = new Color(baseBackgroundColor.r, baseBackgroundColor.g, baseBackgroundColor.b, fromAlpha);
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