using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages a UI menu that can be toggled.
/// The menu starts inactive and is activated before fading in.
/// </summary>
public class ContinueManager : MonoBehaviour
{
    // --- Public Fields ---
    #region Public Fields

    [Header("UI References")]

    [Tooltip("The main panel that contains all menu buttons.")]
    public RectTransform menuContainer;

    [Tooltip("The CanvasGroup component on the menu container for fading.")]
    public CanvasGroup menuCanvasGroup;

    [Tooltip("The semi-transparent background image.")]
    public Image backgroundImage;


    [Header("Animation Settings")]
    [Tooltip("How long the fade animation takes in seconds.")]
    public float fadeDuration = 0.5f;

    #endregion

    // --- Private State ---
    #region Private State

    // Tracks if the menu is currently visible or not.
    private bool isMenuShown = false;

    // Stores the original color of the background image to correctly calculate fades.
    private Color baseBackgroundColor;

    #endregion

    // --- Unity Lifecycle Methods ---
    #region Unity Lifecycle Methods

    void Start()
    {
        // Ensure the menu is considered hidden at the start.
        isMenuShown = false;

        // Prepare the background image for fading.
        if (backgroundImage != null)
        {
            baseBackgroundColor = backgroundImage.color;
            backgroundImage.color = new Color(baseBackgroundColor.r, baseBackgroundColor.g, baseBackgroundColor.b, 0f);
        }

        // Prepare the menu panel to be hidden.
        if (menuCanvasGroup != null)
        {
            menuCanvasGroup.alpha = 0f;
            menuCanvasGroup.interactable = false;
            menuCanvasGroup.blocksRaycasts = false;
        }

        // IMPORTANT: Start with the entire menu GameObject deactivated.
        if (menuContainer != null)
        {
            menuContainer.gameObject.SetActive(false);
        }
    }
    
    #endregion

    // --- Public Methods (for UI Buttons) ---
    #region Public Methods

    /// <summary>
    /// Activates and fades in the menu.
    /// </summary>
    public void ShowMenu()
    {
        // --- INÍCIO DA MODIFICAÇÃO ---
        Debug.Log("ContinueManager: A função ShowMenu() foi chamada!");
        // --- FIM DA MODIFICAÇÃO ---

        if (isMenuShown)
        {
            Debug.Log("ContinueManager: O menu já estava sendo exibido, a animação não será executada novamente.");
            return; // Prevent running the animation again if already shown.
        }

        isMenuShown = true;

        // IMPORTANT: First, activate the menu GameObject so it can be animated.
        if (menuContainer != null)
        {
            Debug.Log("ContinueManager: Ativando o menuContainer.");
            menuContainer.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("ContinueManager: A referência para 'menuContainer' está nula!");
        }

        // Start the fade-in animations for the menu and the background.
        StartCoroutine(FadeMenu(true));
        StartCoroutine(FadeBackground(0f, 1f)); // Ajustado para 1f (preto sólido) como discutimos.
    }

    /// <summary>
    /// Fades out and deactivates the menu.
    /// </summary>
    public void HideMenu()
    {
        if (!isMenuShown) return; // Prevent running the animation again if already hidden.

        isMenuShown = false;

        // Start the fade-out animations.
        StartCoroutine(FadeMenu(false));
        StartCoroutine(FadeBackground(backgroundImage.color.a, 0f));
    }

    /// <summary>
    /// Resets the current level.
    /// </summary>
    public void ResetScene()
    {
        isMenuShown = false; // Reset state before changing scene.
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Loads the Main Menu scene.
    /// </summary>
    public void GoToHome()
    {
        isMenuShown = false; // Reset state before changing scene.
        SceneManager.LoadScene("MainMenu");
    }

    #endregion

    // --- Coroutines for Animation ---
    #region Coroutines

    /// <summary>
    /// Coroutine to fade the menu panel in or out using its CanvasGroup.
    /// </summary>
    /// <param name="show">True to fade in, false to fade out.</param>
    private IEnumerator FadeMenu(bool show)
    {
        if (menuCanvasGroup == null)
        {
            Debug.LogWarning("ContinueManager: A referência para 'menuCanvasGroup' está nula! A animação do menu não vai funcionar.");
            yield break;
        }

        if (show)
        {
            menuCanvasGroup.interactable = true;
            menuCanvasGroup.blocksRaycasts = true;
        }

        float elapsed = 0f;
        float fromAlpha = show ? 0f : 1f;
        float toAlpha = show ? 1f : 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            menuCanvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, elapsed / fadeDuration);
            yield return null;
        }
        
        menuCanvasGroup.alpha = toAlpha;
        
        if (!show)
        {
            menuCanvasGroup.interactable = false;
            menuCanvasGroup.blocksRaycasts = false;
            if (menuContainer != null)
            {
                menuContainer.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Coroutine to fade the dark background image in or out.
    /// </summary>
    private IEnumerator FadeBackground(float fromAlpha, float toAlpha)
    {
        if (backgroundImage == null)
        {
            Debug.LogWarning("ContinueManager: A referência para 'backgroundImage' está nula! A animação de fundo não vai funcionar.");
            yield break;
        }

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

    #endregion
}