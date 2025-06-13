using System.Collections;
using System.Collections.Generic;
using EasyTransition;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles preloading of sprites and audio for a level based on a song name.
/// Works with WebGL by avoiding platform-specific memory checks.
/// </summary>
public class LevelPreload : MonoBehaviour
{
    /// <summary>
    /// The name of the song/level to load.
    /// The scene and resource folders must match this name.
    /// </summary>
    [Header("Preload Settings")]
    public string Song = "";

    /// <summary>
    /// Maximum allowed load time per frame in milliseconds (to avoid frame drops).
    /// </summary>
    [Header("Performance")]
    [Tooltip("Max load time per frame (ms)")]
    [Range(1f, 33f)] public float maxLoadTimePerFrame = 10f;

    /// <summary>
    /// Delay between resource loading operations to avoid locking the main thread.
    /// </summary>
    [Tooltip("Delay between resource loads")]
    [Range(0f, 0.1f)] public float interLoadDelay = 0.01f;

    /// <summary>
    /// Settings used for the scene transition effect.
    /// </summary>
    [Header("Transition")]
    public TransitionSettings transition;

    /// <summary>
    /// Delay before executing the scene load transition (in seconds).
    /// </summary>
    public float loadDelay;

    /// <summary>
    /// Cached dictionary of preloaded sprites.
    /// </summary>
    public static Dictionary<string, Sprite> SpriteCache { get; private set; }

    /// <summary>
    /// Cached dictionary of preloaded audio clips.
    /// </summary>
    public static Dictionary<string, AudioClip> AudioCache { get; private set; }

    private bool spritesLoaded = false;
    private bool audioLoaded = false;

    /// <summary>
    /// Initializes the cache and starts the preload coroutine.
    /// </summary>
    private void Start()
    {
        SpriteCache = new Dictionary<string, Sprite>();
        AudioCache = new Dictionary<string, AudioClip>();

        StartCoroutine(PreloadFiles());
    }

    /// <summary>
    /// Preloads sprites and audio, then transitions to the next scene.
    /// </summary>
    /// <returns>Coroutine enumerator.</returns>
    IEnumerator PreloadFiles()
    {
        string spritesFolderPath = "Preload/" + Song;
        string musicFolderPath = "Musics/" + Song;

        StartCoroutine(LoadSprites(spritesFolderPath));
        StartCoroutine(LoadAudio(musicFolderPath));

        yield return new WaitUntil(() => spritesLoaded && audioLoaded);
        LoadNextScene();
    }

    /// <summary>
    /// Loads all sprites in the specified path and caches them.
    /// </summary>
    /// <param name="path">Resources path where the sprites are located.</param>
    /// <returns>Coroutine enumerator.</returns>
    IEnumerator LoadSprites(string path)
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>(path);

        if (sprites == null || sprites.Length == 0)
        {
            spritesLoaded = true;
            yield break;
        }

        foreach (Sprite sprite in sprites)
        {
            if (sprite == null) continue;

            if (!SpriteCache.ContainsKey(sprite.name))
            {
                SpriteCache.Add(sprite.name, sprite);
            }

            if (interLoadDelay > 0)
                yield return new WaitForSeconds(interLoadDelay);
        }

        spritesLoaded = true;
    }

    /// <summary>
    /// Loads all audio clips in the specified path and caches them.
    /// </summary>
    /// <param name="path">Resources path where the audio clips are located.</param>
    /// <returns>Coroutine enumerator.</returns>
    IEnumerator LoadAudio(string path)
    {
        AudioClip[] audioClips = Resources.LoadAll<AudioClip>(path);

        if (audioClips == null || audioClips.Length == 0)
        {
            audioLoaded = true;
            yield break;
        }

        foreach (AudioClip clip in audioClips)
        {
            if (clip == null) continue;

            if (!AudioCache.ContainsKey(clip.name))
            {
                AudioCache.Add(clip.name, clip);
            }

            if (interLoadDelay > 0)
                yield return new WaitForSeconds(interLoadDelay);
        }

        audioLoaded = true;
    }

    /// <summary>
    /// Loads the scene with the same name as the song if available.
    /// </summary>
    private void LoadNextScene()
    {
        string sceneName = Song;

        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            SafeTransition(sceneName);
        }
        else
        {
            FallbackToMainMenu();
        }
    }

    /// <summary>
    /// Performs a safe scene transition using TransitionManager, if available.
    /// </summary>
    /// <param name="sceneName">The scene to load.</param>
    private void SafeTransition(string sceneName)
    {
        if (TransitionManager.Instance() != null)
        {
            TransitionManager.Instance().Transition(sceneName, transition, loadDelay);
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    /// <summary>
    /// Fallback method to load the main menu if the target scene isn't found.
    /// </summary>
    private void FallbackToMainMenu()
    {
        const string fallbackScene = "MainMenu";

        if (TransitionManager.Instance() != null)
        {
            TransitionManager.Instance().Transition(fallbackScene, transition, loadDelay);
        }
        else
        {
            SceneManager.LoadScene(fallbackScene);
        }
    }

    /// <summary>
    /// Retrieves a sprite from the cache using its name.
    /// </summary>
    /// <param name="key">The name of the sprite.</param>
    /// <returns>The sprite if found, otherwise null.</returns>
    public static Sprite GetSprite(string key)
    {
        if (SpriteCache != null && SpriteCache.TryGetValue(key, out var sprite))
        {
            return sprite;
        }
        return null;
    }

    /// <summary>
    /// Tries to get a sprite from the cache.
    /// </summary>
    /// <param name="key">The sprite name.</param>
    /// <param name="sprite">Output parameter for the sprite if found.</param>
    /// <returns>True if found, otherwise false.</returns>
    public static bool TryGetSprite(string key, out Sprite sprite)
    {
        sprite = null;
        if (SpriteCache == null) return false;
        return SpriteCache.TryGetValue(key, out sprite);
    }

    /// <summary>
    /// Retrieves an audio clip from the cache using its name.
    /// </summary>
    /// <param name="key">The name of the audio clip.</param>
    /// <returns>The audio clip if found, otherwise null.</returns>
    public static AudioClip GetAudio(string key)
    {
        if (AudioCache != null && AudioCache.TryGetValue(key, out var clip))
        {
            return clip;
        }
        return null;
    }

    /// <summary>
    /// Tries to get an audio clip from the cache.
    /// </summary>
    /// <param name="key">The audio clip name.</param>
    /// <param name="clip">Output parameter for the audio clip if found.</param>
    /// <returns>True if found, otherwise false.</returns>
    public static bool TryGetAudio(string key, out AudioClip clip)
    {
        clip = null;
        if (AudioCache == null) return false;
        return AudioCache.TryGetValue(key, out clip);
    }

    /// <summary>
    /// Clears caches and unloads unused assets when this component is destroyed.
    /// </summary>
    private void OnDestroy()
    {
        SpriteCache?.Clear();
        AudioCache?.Clear();
        Resources.UnloadUnusedAssets();
    }
}
