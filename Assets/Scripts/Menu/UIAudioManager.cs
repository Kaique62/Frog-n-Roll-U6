using UnityEngine;

/// <summary>
/// Manages UI audio effects such as hover and click sounds using a singleton pattern.
/// </summary>
public class UIAudioManager : MonoBehaviour
{
    /// <summary>
    /// Singleton instance of the UIAudioManager.
    /// </summary>
    public static UIAudioManager instance;

    [Header("Audio Clips (SFX)")]

    /// <summary>
    /// Audio clip to play when UI elements are hovered over.
    /// </summary>
    public AudioClip hoverSoundClip;

    /// <summary>
    /// Audio clip to play when UI elements are clicked.
    /// </summary>
    public AudioClip clickSoundClip;

    // You can add more audio clips here, e.g., saveSoundClip

    /// <summary>
    /// The single audio source used to play all UI sound effects.
    /// </summary>
    private AudioSource sfxSource;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// Sets up the singleton and ensures the AudioSource component exists.
    /// </summary>
    void Awake()
    {
        // Singleton logic: if no instance exists, assign this and persist across scenes
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Destroy duplicate instances
            Destroy(gameObject);
        }

        // Get or add AudioSource component to play sound effects
        sfxSource = GetComponent<AudioSource>();
        if (sfxSource == null)
        {
            Debug.LogWarning("No AudioSource found on UIAudioManager. Adding one automatically.");
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }
    }

    /// <summary>
    /// Plays the hover sound effect once using the audio source.
    /// </summary>
    public void PlayHoverSound()
    {
        if (hoverSoundClip != null)
        {
            sfxSource.PlayOneShot(hoverSoundClip);
        }
    }

    /// <summary>
    /// Plays the click sound effect once using the audio source.
    /// </summary>
    public void PlayClickSound()
    {
        if (clickSoundClip != null)
        {
            sfxSource.PlayOneShot(clickSoundClip);
        }
    }
}
