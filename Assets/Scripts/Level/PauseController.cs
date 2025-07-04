using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public static bool isPaused = false;
    // MUDANÇA: "Trava" global para permitir ou bloquear o pause.
    public static bool IsPauseAllowed = true;

    [Header("UI References")]
    public RectTransform menuContainer;
    public Image backgroundImage;
    public TextMeshProUGUI countdownText;
    public GameObject mobileControls;

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
    private bool isCountingDown = false;

    void Start()
    {
        // MUDANÇA: Garante que o pause seja permitido sempre que a cena iniciar.
        IsPauseAllowed = true;
        isPaused = false;
        isCountingDown = false;
        Time.timeScale = 1f;

        if (menuContainer != null) menuContainer.anchoredPosition = hiddenPos;
        if (countdownText != null) countdownText.text = "";
        
        if(backgroundImage != null)
        {
            baseBackgroundColor = backgroundImage.color;
            backgroundImage.color = new Color(baseBackgroundColor.r, baseBackgroundColor.g, baseBackgroundColor.b, 0f);
        }
    }

    void Update()
    {
        // MUDANÇA: Adicionamos 'IsPauseAllowed' na condição.
        if (IsPauseAllowed && Input.GetKeyDown(KeyCode.Escape) && !isCountingDown)
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
        // MUDANÇA: Adicionamos 'IsPauseAllowed' na condição.
        if (!IsPauseAllowed || isPaused || isCountingDown) return;

        Debug.Log("PauseGame() foi chamado. Pausando o jogo...");

        Time.timeScale = 0f;
        isPaused = true;
        AudioListener.pause = true;

        #if UNITY_ANDROID || UNITY_IOS
        if (mobileControls != null)
        {
            mobileControls.SetActive(false);
        }
        #endif

        StartCoroutine(AnimateMenu(true));
        StartCoroutine(FadeBackground(0f, 190f / 255f));
    }

    public void ResumeGame()
    {
        Debug.Log("Botão/Tecla 'Resume Game' foi acionado!");

        isPaused = false;
        StartCoroutine(AnimateMenu(false));
        StartCoroutine(CountdownToResume());
    }

    private IEnumerator CountdownToResume()
    {
        isCountingDown = true;

        #if UNITY_ANDROID || UNITY_IOS
        if (mobileControls != null)
        {
            mobileControls.SetActive(true);
        }
        #endif

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

        isCountingDown = false;
    }

    public void ResetScene()
    {
        Debug.Log("Botão 'Reset Scene' foi clicado!");

        isPaused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToHome()
    {
        Debug.Log("Botão 'Go To Home' foi clicado!");

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
        if (backgroundImage == null) yield break;

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