using UnityEngine;
using System.Collections;

public class PauseManager : MonoBehaviour
{
    public static bool isPaused = false;
    public RectTransform pauseMenu; // arraste o painel do menu aqui no inspetor
    public float animDuration = 0.5f; // tempo da animação em segundos
    public Vector2 hiddenPos = new Vector2(0, -635);
    public Vector2 shownPos = new Vector2(0, 135);

    private Coroutine animCoroutine;

    void Start()
    {
        // Garante que o menu começa escondido
        pauseMenu.anchoredPosition = hiddenPos;
    }

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

        // Iniciar animação para mostrar o menu
        if (animCoroutine != null) StopCoroutine(animCoroutine);
        animCoroutine = StartCoroutine(AnimatePanel(pauseMenu, pauseMenu.anchoredPosition, shownPos));
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        isPaused = false;
        AudioListener.pause = false;

        // Iniciar animação para esconder o menu
        if (animCoroutine != null) StopCoroutine(animCoroutine);
        animCoroutine = StartCoroutine(AnimatePanel(pauseMenu, pauseMenu.anchoredPosition, hiddenPos));
    }

    private IEnumerator AnimatePanel(RectTransform panel, Vector2 from, Vector2 to)
    {
        float elapsed = 0f;

        while (elapsed < animDuration)
        {
            float t = elapsed / animDuration;

            // Ease out: começa rápido e termina devagar
            t = 1f - Mathf.Pow(1f - t, 2f); // quadratic ease-out

            panel.anchoredPosition = Vector2.Lerp(from, to, t);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        panel.anchoredPosition = to;
    }
}
