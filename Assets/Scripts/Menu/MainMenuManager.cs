using System.Collections;
using EasyTransition;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the main menu interactions, animations, and transitions.
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    /// <summary>
    /// The name of the game level to load.
    /// </summary>
    [SerializeField] private string gameLevelName;

    /// <summary>
    /// The initial main menu panel GameObject.
    /// </summary>
    [SerializeField] private GameObject initialMenuPanel;

    /// <summary>
    /// The options panel GameObject.
    /// </summary>
    [SerializeField] private GameObject optionsPanel;

    // ===== NOVO =====
    /// <summary>
    /// The credits panel GameObject.
    /// </summary>
    [SerializeField] private GameObject creditsPanel;

    [Header("Visual Elements")]
    [SerializeField] private RectTransform mainMenuRect;
    [SerializeField] private RectTransform fnrLogoRect;
    [SerializeField] private RectTransform optionsPanelRect;
    [SerializeField] private Transform bigElement;

    [Header("Animation")]
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private Vector3 initialScale = new Vector3(0.45f, 0.45f, 0.45f);
    [SerializeField] private Vector3 finalScale = Vector3.one;
    [SerializeField] private float horizontalOffset = 1000f;

    [Header("Transition Prefab")]
    public TransitionSettings transition;
    public float loadDelay;

    private Vector3 mainMenuOriginalPos;
    private Vector3 fnrLogoOriginalPos;
    private Vector3 optionsPanelOriginalPos;

    private void Start()
    {
        Controls.LoadKeyBinds();

        if (bigElement != null)
            bigElement.localScale = initialScale;

        if (mainMenuRect != null)
            mainMenuOriginalPos = mainMenuRect.anchoredPosition;

        if (fnrLogoRect != null)
            fnrLogoOriginalPos = fnrLogoRect.anchoredPosition;

        if (optionsPanelRect != null)
        {
            optionsPanelOriginalPos = optionsPanelRect.anchoredPosition;
            optionsPanelRect.anchoredPosition = optionsPanelOriginalPos + Vector3.right * horizontalOffset;
        }
    }

    public void Play()
    {
        TransitionManager.Instance().Transition("Preload", transition, loadDelay);
    }

    public void OpenOptions()
    {
        StopAllCoroutines();

        if (optionsPanel != null)
            optionsPanel.SetActive(true);

        if (mainMenuRect != null)
            StartCoroutine(AnimateMovement(mainMenuRect, mainMenuRect.anchoredPosition, mainMenuOriginalPos + Vector3.right * horizontalOffset));

        if (fnrLogoRect != null)
            StartCoroutine(AnimateMovement(fnrLogoRect, fnrLogoRect.anchoredPosition, fnrLogoOriginalPos + Vector3.right * horizontalOffset));

        if (optionsPanelRect != null)
            StartCoroutine(AnimateMovement(optionsPanelRect, optionsPanelRect.anchoredPosition, optionsPanelOriginalPos));

        if (bigElement != null)
            StartCoroutine(AnimateScale(bigElement, initialScale, finalScale));
    }

    public void CloseOptions()
    {
        FpsLimiter fpsLimiter = FindObjectOfType<FpsLimiter>();
        if (fpsLimiter != null)
            fpsLimiter.SaveAllSettings();

        StopAllCoroutines();

        if (bigElement != null)
            StartCoroutine(AnimateScale(bigElement, bigElement.localScale, initialScale));

        if (mainMenuRect != null)
            StartCoroutine(AnimateMovement(mainMenuRect, mainMenuRect.anchoredPosition, mainMenuOriginalPos));

        if (fnrLogoRect != null)
            StartCoroutine(AnimateMovement(fnrLogoRect, fnrLogoRect.anchoredPosition, fnrLogoOriginalPos));

        if (optionsPanelRect != null)
            StartCoroutine(AnimateMovement(optionsPanelRect, optionsPanelRect.anchoredPosition, optionsPanelOriginalPos + Vector3.right * horizontalOffset));

        StartCoroutine(DeactivatePanelAfterDelay(animationDuration));
    }

    private IEnumerator DeactivatePanelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (optionsPanel != null)
            optionsPanel.SetActive(false);
    }

    public void OpenSubState(string state)
    {
        SceneManager.LoadScene("KeyBindMenu", LoadSceneMode.Additive);
    }

    public void QuitGame()
    {
        Debug.Log("Exit button pressed!");
        #if !UNITY_EDITOR
            Application.Quit();
        #endif
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    private IEnumerator AnimateScale(Transform target, Vector3 from, Vector3 to)
    {
        float time = 0f;
        while (time < animationDuration)
        {
            target.localScale = Vector3.Lerp(from, to, time / animationDuration);
            time += Time.deltaTime;
            yield return null;
        }
        target.localScale = to;
    }

    private IEnumerator AnimateMovement(RectTransform target, Vector3 from, Vector3 to)
    {
        float time = 0f;
        while (time < animationDuration)
        {
            target.anchoredPosition = Vector3.Lerp(from, to, time / animationDuration);
            time += Time.deltaTime;
            yield return null;
        }
        target.anchoredPosition = to;
    }
    
    // ===== NOVAS FUNÇÕES =====
    /// <summary>
    /// Activates the credits panel, making it visible.
    /// </summary>
    public void OpenCredits()
    {
        if (creditsPanel != null)
        {
            creditsPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Deactivates the credits panel, hiding it.
    /// </summary>
    public void CloseCredits()
    {
        if (creditsPanel != null)
        {
            creditsPanel.SetActive(false);
        }
    }
}