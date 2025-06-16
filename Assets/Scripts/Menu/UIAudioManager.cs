using UnityEngine;
using UnityEngine.SceneManagement;

public class UIAudioManager : MonoBehaviour
{
    public static UIAudioManager instance;

    [Header("Audio Clips (SFX)")]
    public AudioClip hoverSoundClip;
    public AudioClip clickSoundClip;

    private AudioSource sfxSource;

    public float CurrentVolume => sfxSource.volume;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        sfxSource = GetComponent<AudioSource>();
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }
        sfxSource.ignoreListenerPause = true;
    }

    void Start()
    {
        UpdateSfxVolume();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateSfxVolume();
    }

    public void UpdateSfxVolume()
    {
        string savedVolume = ConfigManager.Read("volume_SfxVolume");
        if (float.TryParse(savedVolume, out float volume))
        {
            sfxSource.volume = Mathf.Clamp01(volume);
        }
        else
        {
            sfxSource.volume = 1f;
        }
    }

    public void SetSfxVolume(float volume)
    {
        float clampedVolume = Mathf.Clamp01(volume);
        sfxSource.volume = clampedVolume;

        // ===== CORREÇÃO APLICADA AQUI =====
        // Chamando o método 'Save' do seu ConfigManager, em vez de 'Write'.
        ConfigManager.Save("volume_SfxVolume", clampedVolume.ToString());
    }

    public void PlayHoverSound()
    {
        if (hoverSoundClip != null) sfxSource.PlayOneShot(hoverSoundClip);
    }

    public void PlayClickSound()
    {
        if (clickSoundClip != null) sfxSource.PlayOneShot(clickSoundClip);
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}