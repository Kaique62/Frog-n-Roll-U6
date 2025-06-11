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

    [Header("Visual Elements")]

    /// <summary>
    /// RectTransform of the main menu UI.
    /// </summary>
    [SerializeField] private RectTransform mainMenuRect;

    /// <summary>
    /// RectTransform of the logo UI element.
    /// </summary>
    [SerializeField] private RectTransform fnrLogoRect;

    /// <summary>
    /// RectTransform of the options panel.
    /// </summary>
    [SerializeField] private RectTransform optionsPanelRect;

    /// <summary>
    /// Transform of a large UI element that will be scaled during animation.
    /// </summary>
    [SerializeField] private Transform bigElement;

    [Header("Animation")]

    /// <summary>
    /// Duration of animations in seconds.
    /// </summary>
    [SerializeField] private float animationDuration = 0.3f;

    /// <summary>
    /// Initial scale of the big UI element.
    /// </summary>
    [SerializeField] private Vector3 initialScale = new Vector3(0.45f, 0.45f, 0.45f);

    /// <summary>
    /// Final scale of the big UI element.
    /// </summary>
    [SerializeField] private Vector3 finalScale = Vector3.one;

    /// <summary>
    /// Horizontal offset used for sliding animations.
    /// </summary>
    [SerializeField] private float horizontalOffset = 1000f;

    [Header("Transition Prefab")]

    /// <summary>
    /// Transition settings used when switching scenes.
    /// </summary>
    public TransitionSettings transition;

    /// <summary>
    /// Delay before loading the next scene.
    /// </summary>
    public float loadDelay;

    // Original anchored positions to restore after animations
    private Vector3 mainMenuOriginalPos;
    private Vector3 fnrLogoOriginalPos;
    private Vector3 optionsPanelOriginalPos;

    /// <summary>
    /// Initializes UI elements and positions.
    /// </summary>
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
            // Initially position options panel outside the screen to the right
            optionsPanelRect.anchoredPosition = optionsPanelOriginalPos + Vector3.right * horizontalOffset;
        }
    }

    /// <summary>
    /// Starts the game by transitioning to the preload state.
    /// </summary>
    public void Play()
    {
        TransitionManager.Instance().Transition("PreloadState", transition, loadDelay);
    }

    /// <summary>
    /// Opens the options panel with animation, sliding the main menu and logo out.
    /// </summary>
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

    /// <summary>
    /// Closes the options panel with animation, sliding main menu and logo back in.
    /// Saves any settings before closing.
    /// </summary>
    public void CloseOptions()
    {
        // Save settings before closing options
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

    /// <summary>
    /// Loads a sub-state scene additively, e.g., for key binding menu.
    /// </summary>
    /// <param name="state">Name of the sub-state to open.</param>
    public void OpenSubState(string state)
    {
        SceneManager.LoadScene("KeyBindMenu", LoadSceneMode.Additive);
    }

    /// <summary>
    /// Exits the game application.
    /// </summary>
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

    /// <summary>
    /// Coroutine to animate the scale of a Transform from a start scale to an end scale.
    /// </summary>
    /// <param name="target">Transform to animate.</param>
    /// <param name="from">Starting scale.</param>
    /// <param name="to">Ending scale.</param>
    /// <returns>IEnumerator for coroutine.</returns>
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

    /// <summary>
    /// Coroutine to animate the anchored position of a RectTransform from a start to an end position.
    /// </summary>
    /// <param name="target">RectTransform to animate.</param>
    /// <param name="from">Starting anchored position.</param>
    /// <param name="to">Ending anchored position.</param>
    /// <returns>IEnumerator for coroutine.</returns>
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
}
