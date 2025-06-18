using UnityEngine;
using UnityEngine.UI; // Essencial para controlar componentes de UI como a Imagem
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    [Header("Scene Settings")]
    // O nome da sua cena de menu principal
    public string sceneToLoad = "MainMenu";

    [Header("Logo Animation Settings")]
    // Arraste o objeto da sua logo (que tem o componente Image) aqui
    public Image logoImage;

    // Tempo total da animação de abertura
    public float totalDuration = 3f;

    // Porcentagem do tempo total para cada fase da animação
    [Range(0, 1)] public float fadeInPercent = 0.4f; // 40% do tempo para fade-in
    [Range(0, 1)] public float fadeOutPercent = 0.4f; // 40% do tempo para fade-out
    
    // O quão maior a logo ficará no final do zoom
    public float finalZoomScale = 1.1f; // Aumenta o tamanho em 10%

    void Start()
    {
        // Garante que a logo não seja nula para evitar erros
        if (logoImage == null)
        {
            Debug.LogError("A imagem da logo não foi definida no Inspector!");
            return;
        }

        // Inicia a rotina de animação
        StartCoroutine(AnimateLogoAndLoadScene());
    }

    IEnumerator AnimateLogoAndLoadScene()
    {
        // --- PREPARAÇÃO INICIAL ---
        // Garante que a logo comece totalmente invisível e no tamanho original
        logoImage.color = new Color(logoImage.color.r, logoImage.color.g, logoImage.color.b, 0);
        logoImage.transform.localScale = Vector3.one;

        float fadeInTime = totalDuration * fadeInPercent;
        float fadeOutTime = totalDuration * fadeOutPercent;
        float holdTime = totalDuration - fadeInTime - fadeOutTime;

        // --- FASE 1: FADE-IN E ZOOM-IN ---
        float timer = 0f;
        while (timer < fadeInTime)
        {
            // Calcula o progresso (de 0.0 a 1.0)
            float progress = timer / fadeInTime;

            // Interpola a cor (alpha/transparência) de 0 para 1
            logoImage.color = new Color(logoImage.color.r, logoImage.color.g, logoImage.color.b, progress);
            
            // Interpola a escala (tamanho) de 1 para o valor final do zoom
            logoImage.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * finalZoomScale, progress);

            timer += Time.deltaTime;
            yield return null; // Espera o próximo quadro
        }

        // Garante que os valores finais sejam exatos
        logoImage.color = new Color(logoImage.color.r, logoImage.color.g, logoImage.color.b, 1);
        logoImage.transform.localScale = Vector3.one * finalZoomScale;

        // --- FASE 2: PAUSA (Logo visível) ---
        yield return new WaitForSeconds(holdTime);

        // --- FASE 3: FADE-OUT ---
        timer = 0f;
        while (timer < fadeOutTime)
        {
            float progress = timer / fadeOutTime;

            // Interpola a cor (alpha/transparência) de 1 para 0
            logoImage.color = new Color(logoImage.color.r, logoImage.color.g, logoImage.color.b, 1 - progress);

            timer += Time.deltaTime;
            yield return null; // Espera o próximo quadro
        }

        // Garante que a logo termine totalmente invisível
        logoImage.color = new Color(logoImage.color.r, logoImage.color.g, logoImage.color.b, 0);

        // --- FASE 4: CARREGAR A PRÓXIMA CENA ---
        SceneManager.LoadScene(sceneToLoad);
    }
}