using System.Collections;
using EasyTransition;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPrincipalManager : MonoBehaviour
{
    [SerializeField] private string nomeDoLevelDeJogo;
    [SerializeField] private GameObject painelMenuInicial;
    [SerializeField] private GameObject painelOpcoes;

    [Header("Elementos Visuais")]
    [SerializeField] private RectTransform menuPrincipal;
    [SerializeField] private RectTransform fnrLogoType;
    [SerializeField] private RectTransform painelOpcoesRect;
    [SerializeField] private Transform bigElement;

    [Header("Animação")]
    [SerializeField] private float tempoAnimacao = 0.3f;
    [SerializeField] private Vector3 escalaInicial = new Vector3(0.45f, 0.45f, 0.45f);
    [SerializeField] private Vector3 escalaFinal = Vector3.one;
    [SerializeField] private float deslocamentoX = 1000f;

    [Header("Transition Prefab")]
    public TransitionSettings transition;
    public float loadDelay;

    private Vector3 menuPrincipalPosOriginal;
    private Vector3 fnrLogoTypePosOriginal;
    private Vector3 painelOpcoesPosOriginal;

    private void Start()
    {
        Controls.LoadKeyBinds();
        if (bigElement != null)
            bigElement.localScale = escalaInicial;

        if (menuPrincipal != null)
            menuPrincipalPosOriginal = menuPrincipal.anchoredPosition;

        if (fnrLogoType != null)
            fnrLogoTypePosOriginal = fnrLogoType.anchoredPosition;

        if (painelOpcoesRect != null)
        {
            painelOpcoesPosOriginal = painelOpcoesRect.anchoredPosition;
            painelOpcoesRect.anchoredPosition = painelOpcoesPosOriginal + Vector3.right * deslocamentoX;
        }

        if (painelOpcoes != null)
            painelOpcoes.SetActive(true); // mantém sempre ativo
    }

    public void Jogar()
    {
        TransitionManager.Instance().Transition("PreloadState", transition, loadDelay);
    }

    public void AbrirOpcoes()
    {
        StopAllCoroutines();

        if (menuPrincipal != null)
            StartCoroutine(AnimarMovimento(menuPrincipal, menuPrincipal.anchoredPosition, menuPrincipalPosOriginal + Vector3.right * deslocamentoX));

        if (fnrLogoType != null)
            StartCoroutine(AnimarMovimento(fnrLogoType, fnrLogoType.anchoredPosition, fnrLogoTypePosOriginal + Vector3.right * deslocamentoX));

        if (painelOpcoesRect != null)
            StartCoroutine(AnimarMovimento(painelOpcoesRect, painelOpcoesRect.anchoredPosition, painelOpcoesPosOriginal));

        if (bigElement != null)
            StartCoroutine(AnimarEscala(bigElement, escalaInicial, escalaFinal));
    }
    public void FecharOpcoes()
    {
        // Salvar configurações
        FpsLimiter fpsLimiter = FindObjectOfType<FpsLimiter>();
        if (fpsLimiter != null)
            fpsLimiter.SalvarTodasConfiguracoes();

        StopAllCoroutines();

        if (bigElement != null)
            StartCoroutine(AnimarEscala(bigElement, bigElement.localScale, escalaInicial));

        if (menuPrincipal != null)
            StartCoroutine(AnimarMovimento(menuPrincipal, menuPrincipal.anchoredPosition, menuPrincipalPosOriginal));

        if (fnrLogoType != null)
            StartCoroutine(AnimarMovimento(fnrLogoType, fnrLogoType.anchoredPosition, fnrLogoTypePosOriginal));

        if (painelOpcoesRect != null)
            StartCoroutine(AnimarMovimento(painelOpcoesRect, painelOpcoesRect.anchoredPosition, painelOpcoesPosOriginal + Vector3.right * deslocamentoX));
    }

    public void AbrirSubState(string state)
    {
        SceneManager.LoadScene("KeyBindMenu", LoadSceneMode.Additive);
    }

    public void SairJogo()
    {
        Debug.Log("Sair do jogo");
        Application.Quit();
    }

    private IEnumerator AnimarEscala(Transform alvo, Vector3 de, Vector3 para)
    {
        float tempo = 0f;
        while (tempo < tempoAnimacao)
        {
            alvo.localScale = Vector3.Lerp(de, para, tempo / tempoAnimacao);
            tempo += Time.deltaTime;
            yield return null;
        }
        alvo.localScale = para;
    }

    private IEnumerator AnimarMovimento(RectTransform alvo, Vector3 de, Vector3 para)
    {
        float tempo = 0f;
        while (tempo < tempoAnimacao)
        {
            alvo.anchoredPosition = Vector3.Lerp(de, para, tempo / tempoAnimacao);
            tempo += Time.deltaTime;
            yield return null;
        }
        alvo.anchoredPosition = para;
    }
}
