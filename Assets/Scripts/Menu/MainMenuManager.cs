using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPrincipalManager : MonoBehaviour
{
    [SerializeField] private string nomeDoLevelDeJogo;
    [SerializeField] private GameObject painelMenuInicial;
    [SerializeField] private GameObject painelOpcoes;

    public void Jogar()
    {
        SceneManager.LoadScene(nomeDoLevelDeJogo);
    }

    public void AbrirOpcoes()
    {
        painelMenuInicial.SetActive(false);
        painelOpcoes.SetActive(true);
    }

    public void AbrirSubState(string state){
        SceneManager.LoadScene("KeyBindMenu", LoadSceneMode.Additive);
        //Time.timeScale = 0f;
    }

    public void FecharOpcoes()
    {
        // Salvar configura��es
        FpsLimiter fpsLimiter = FindObjectOfType<FpsLimiter>();
        if (fpsLimiter != null)
        {
            fpsLimiter.SalvarTodasAsConfigs(); // chama sua fun��o p�blica de salvar
        }

        painelOpcoes.SetActive(false);
        painelMenuInicial.SetActive(true);
    }

    public void SairJogo()
    {
        Debug.Log("Sair do jogo");
        Application.Quit();
    }
}
