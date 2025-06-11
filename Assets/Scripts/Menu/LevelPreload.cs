using System.Collections;
using System.Collections.Generic;
using EasyTransition;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelPreload : MonoBehaviour
{
    /// <summary>
    /// The name of the song/level to preload.
    /// </summary>
    [Header("Preload Settings")]
    public string Song = "";

    /// <summary>
    /// Maximum RAM usage percentage allowed (0.1 - 0.9).
    /// </summary>
    [Header("Memory Management")]
    [Tooltip("Maximum RAM usage percentage (0.1-0.9)")]
    [Range(0.1f, 0.9f)] public float maxRamUsagePercent = 0.7f;

    /// <summary>
    /// Minimum required RAM in megabytes.
    /// </summary>
    [Tooltip("Minimum required RAM (MB)")]
    public int minRequiredRamMB = 500;

    /// <summary>
    /// Maximum loading time allowed per frame in milliseconds.
    /// </summary>
    [Header("Performance")]
    [Tooltip("Max load time per frame (ms)")]
    [Range(1f, 33f)] public float maxLoadTimePerFrame = 10f;

    /// <summary>
    /// Delay between loading each resource in seconds.
    /// </summary>
    [Tooltip("Delay between resource loads")]
    [Range(0f, 0.1f)] public float interLoadDelay = 0.01f;

    /// <summary>
    /// Transition settings used when switching scenes.
    /// </summary>
    [Header("Transition")]
    public TransitionSettings transition;

    /// <summary>
    /// Delay before starting the scene load transition.
    /// </summary>
    public float loadDelay;

    /// <summary>
    /// Cached sprites loaded for the current song/level.
    /// </summary>
    public static Dictionary<string, Sprite> SpriteCache { get; private set; }

    /// <summary>
    /// Cached audio clips loaded for the current song/level.
    /// </summary>
    public static Dictionary<string, AudioClip> AudioCache { get; private set; }

    private bool spritesLoaded = false;
    private bool audioLoaded = false;
    private bool resourcesAvailable = true;

    /// <summary>
    /// Initializes caches, checks system resources and starts preload coroutine.
    /// Falls back to main menu if resources are insufficient.
    /// </summary>
    private void Start()
    {
        SpriteCache = new Dictionary<string, Sprite>();
        AudioCache = new Dictionary<string, AudioClip>();

        resourcesAvailable = CheckSystemResources();

        if (resourcesAvailable)
        {
            StartCoroutine(PreloadFiles());
        }
        else
        {
            FallbackToMainMenu();
        }
    }

    /// <summary>
    /// Checks if the system has enough available memory to safely load resources.
    /// </summary>
    /// <returns>True if resources are sufficient; otherwise, false.</returns>
    private bool CheckSystemResources()
    {
        long systemRamMB = SystemInfo.systemMemorySize;
        long usedRamMB = (int)(System.GC.GetTotalMemory(false) / 1048576);
        long availableRamMB = systemRamMB - usedRamMB;

        if (availableRamMB < minRequiredRamMB)
        {
            Debug.LogError($"Insufficient RAM: {availableRamMB}MB available < {minRequiredRamMB}MB required");
            return false;
        }

        long safeLimitMB = (long)(systemRamMB * maxRamUsagePercent);

        if (usedRamMB > safeLimitMB)
        {
            Debug.LogError($"Memory limit exceeded: {usedRamMB}MB > {safeLimitMB}MB");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Coroutine that starts loading sprites and audio clips, then loads the next scene when done.
    /// </summary>
    /// <returns>IEnumerator for coroutine.</returns>
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
    /// Coroutine to load all sprites in the specified folder and cache them.
    /// </summary>
    /// <param name="path">Resource path to load sprites from.</param>
    /// <returns>IEnumerator for coroutine.</returns>
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
    /// Coroutine to load all audio clips in the specified folder and cache them.
    /// </summary>
    /// <param name="path">Resource path to load audio clips from.</param>
    /// <returns>IEnumerator for coroutine.</returns>
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
    /// Loads the next scene corresponding to the song, or falls back to the main menu if unavailable.
    /// </summary>
    private void LoadNextScene()
    {
        string sceneName = Song;

        if (resourcesAvailable && Application.CanStreamedLevelBeLoaded(sceneName))
        {
            SafeTransition(sceneName);
        }
        else
        {
            FallbackToMainMenu();
        }
    }

    /// <summary>
    /// Performs a safe scene transition using the transition manager if available.
    /// </summary>
    /// <param name="sceneName">Name of the scene to load.</param>
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
    /// Fallback to load the main menu scene with transition.
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
    /// Returns the cached sprite by key, or null if not found.
    /// </summary>
    /// <param name="key">Sprite key/name.</param>
    /// <returns>The cached sprite or null.</returns>
    public static Sprite GetSprite(string key)
    {
        if (SpriteCache != null && SpriteCache.TryGetValue(key, out var sprite))
        {
            return sprite;
        }
        return null;
    }

    /// <summary>
    /// Attempts to get a cached sprite by key.
    /// </summary>
    /// <param name="key">Sprite key/name.</param>
    /// <param name="sprite">Output sprite if found.</param>
    /// <returns>True if sprite is found; otherwise, false.</returns>
    public static bool TryGetSprite(string key, out Sprite sprite)
    {
        sprite = null;
        if (SpriteCache == null) return false;
        return SpriteCache.TryGetValue(key, out sprite);
    }

    /// <summary>
    /// Returns the cached audio clip by key, or null if not found.
    /// </summary>
    /// <param name="key">Audio clip key/name.</param>
    /// <returns>The cached audio clip or null.</returns>
    public static AudioClip GetAudio(string key)
    {
        if (AudioCache != null && AudioCache.TryGetValue(key, out var clip))
        {
            return clip;
        }
        return null;
    }

    /// <summary>
    /// Attempts to get a cached audio clip by key.
    /// </summary>
    /// <param name="key">Audio clip key/name.</param>
    /// <param name="clip">Output audio clip if found.</param>
    /// <returns>True if audio clip is found; otherwise, false.</returns>
    public static bool TryGetAudio(string key, out AudioClip clip)
    {
        clip = null;
        if (AudioCache == null) return false;
        return AudioCache.TryGetValue(key, out clip);
    }

    /// <summary>
    /// Cleans up caches and unloads unused assets when this object is destroyed.
    /// </summary>
    private void OnDestroy()
    {
        if (SpriteCache != null) SpriteCache.Clear();
        if (AudioCache != null) AudioCache.Clear();
        Resources.UnloadUnusedAssets();
    }
}
