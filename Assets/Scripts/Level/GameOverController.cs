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
        Time.timeScale = 0f;
        AudioListener.pause = true;

        if (animCoroutine != null) StopCoroutine(animCoroutine);
        animCoroutine = StartCoroutine(AnimateMenu(true));
        StartCoroutine(FadeBackground(0f, 190f / 255f));
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
