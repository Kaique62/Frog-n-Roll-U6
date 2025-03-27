using System.Collections;
using UnityEngine;

public class LevelController : MonoBehaviour
{
    public AudioSource musicAudioSource;  // Referência para o AudioSource (música)
    public float countdownTime = 3f;      // Tempo da contagem regressiva (em segundos)
    public GameObject countdownUI;       // Referência para a UI da contagem regressiva (se desejar exibir isso)
    
    void Start()
    {
        StartCoroutine(StartLevel());
    }

    IEnumerator StartLevel()
    {
        // Ativa a UI de contagem regressiva (se necessário)
        if (countdownUI != null)
        {
            countdownUI.SetActive(true);
        }

        // Inicia a contagem regressiva
        yield return StartCoroutine(Countdown());

        // Após a contagem regressiva, toca a música
        if (musicAudioSource != null && !musicAudioSource.isPlaying)
        {
            Debug.Log("audioStart");
            musicAudioSource.Play();
        }

        // Aqui você pode chamar funções para iniciar outras ações, como movimentar a câmera
        // Por exemplo, iniciar a movimentação da câmera ou qualquer outra ação que aconteça após a música começar
        // StartCoroutine(YourNextActionHere());
    }

    IEnumerator Countdown()
    {
        float remainingTime = countdownTime;

        // Exibe a contagem regressiva (opcional)
        while (remainingTime > 0)
        {
            remainingTime -= Time.deltaTime;

            // Atualiza a UI da contagem regressiva (se tiver UI associada)
            if (countdownUI != null)
            {
                // Aqui você pode atualizar a UI de contagem regressiva, por exemplo, usando texto ou barras de progresso
                // countdownUI.GetComponent<Text>().text = Mathf.Ceil(remainingTime).ToString();
            }

            yield return null;
        }

        // Desativa a UI de contagem regressiva após o tempo passar (se necessário)
        if (countdownUI != null)
        {
            countdownUI.SetActive(false);
        }
    }
}
