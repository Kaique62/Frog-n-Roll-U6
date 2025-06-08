using UnityEngine;

public class UIAudioManager : MonoBehaviour
{
    // O "static instance" continua igual, é ótimo!
    public static UIAudioManager instance;

    // --- MUDANÇA 1: Referência aos ARQUIVOS de áudio, não aos componentes ---
    [Header("Clipes de Áudio (SFX)")]
    public AudioClip hoverSoundClip; // Armazena o som de hover
    public AudioClip clickSoundClip; // Armazena o som de clique
    // Se quiser mais sons, é só adicionar mais AudioClips aqui!
    // public AudioClip saveSoundClip;

    // --- MUDANÇA 2: Um único "alto-falante" para todos os efeitos sonoros ---
    private AudioSource sfxSource; // Será nosso player de efeitos sonoros (SFX)

    void Awake()
    {
        // A lógica do Singleton continua a mesma
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // --- MUDANÇA 3: Pegamos o componente AudioSource neste objeto ---
        // Garante que temos nosso "alto-falante" pronto para usar
        sfxSource = GetComponent<AudioSource>();
        if (sfxSource == null)
        {
            // Se esquecermos de adicionar o componente, ele cria um automaticamente
            Debug.LogWarning("Nenhum AudioSource encontrado no AudioManager. Adicionando um automaticamente.");
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }
    }

    // A função de hover agora usa o AudioClip
    public void PlayHoverSound()
    {
        // Verifica se o ARQUIVO de som foi definido
        if (hoverSoundClip != null)
        {
            // Usa o "alto-falante" único para tocar o clipe de hover uma vez
            sfxSource.PlayOneShot(hoverSoundClip);
        }
    }

    // A função de clique agora usa o AudioClip
    public void PlayClickSound()
    {
        // Verifica se o ARQUIVO de som foi definido
        if (clickSoundClip != null)
        {
            // Usa o "alto-falante" único para tocar o clipe de clique uma vez
            sfxSource.PlayOneShot(clickSoundClip);
        }
    }
}