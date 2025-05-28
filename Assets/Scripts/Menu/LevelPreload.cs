using System.Collections;
using System.Collections.Generic;
using EasyTransition;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelPreload : MonoBehaviour
{
    [Header("Preload Settings")]
    public string Song = "";

    [Header("Memory Management")]
    [Tooltip("Maximum RAM usage percentage (0.1-0.9)")]
    [Range(0.1f, 0.9f)] public float maxRamUsagePercent = 0.7f;
    [Tooltip("Minimum required RAM (MB)")]
    public int minRequiredRamMB = 500;

    [Header("Performance")]
    [Tooltip("Max load time per frame (ms)")]
    [Range(1f, 33f)] public float maxLoadTimePerFrame = 10f;
    [Tooltip("Delay between resource loads")]
    [Range(0f, 0.1f)] public float interLoadDelay = 0.01f;

    [Header("Transition")]
    public TransitionSettings transition;
    public float loadDelay;

    public static Dictionary<string, Sprite> SpriteCache { get; private set; }
    public static Dictionary<string, AudioClip> AudioCache { get; private set; }

    private bool spritesLoaded = false;
    private bool audioLoaded = false;
    private bool resourcesAvailable = true;

    private void Start()
    {
        SpriteCache = new Dictionary<string, Sprite>();
        AudioCache = new Dictionary<string, AudioClip>();
        
        // Check system resources
        resourcesAvailable = CheckSystemResources();
        
        if (resourcesAvailable)
        {
            StartCoroutine(PreloadFiles());
        }
        else
        {
            // Fallback immediately if insufficient resources
            FallbackToMainMenu();
        }
    }

    private bool CheckSystemResources()
    {
        // Get available system memory
        long systemRamMB = SystemInfo.systemMemorySize;
        long usedRamMB = (int)(System.GC.GetTotalMemory(false) / 1048576);
        long availableRamMB = systemRamMB - usedRamMB;

        if (availableRamMB < minRequiredRamMB)
        {
            Debug.LogError($"Insufficient RAM: {availableRamMB}MB available < {minRequiredRamMB}MB required");
            return false;
        }
        
        // Calculate safe usage limit
        long safeLimitMB = (long)(systemRamMB * maxRamUsagePercent);
        
        if (usedRamMB > safeLimitMB)
        {
            Debug.LogError($"Memory limit exceeded: {usedRamMB}MB > {safeLimitMB}MB");
            return false;
        }
        
        return true;
    }

    IEnumerator PreloadFiles()
    {
        string spritesFolderPath = "Preload/" + Song;
        string musicFolderPath = "Musics/" + Song;

        // Start parallel loading
        StartCoroutine(LoadSprites(spritesFolderPath));
        StartCoroutine(LoadAudio(musicFolderPath));

        // Wait for completion
        yield return new WaitUntil(() => spritesLoaded && audioLoaded);
        LoadNextScene();
    }

    IEnumerator LoadSprites(string path)
    {
        // Load all sprites at once (more efficient than async)
        Sprite[] sprites = Resources.LoadAll<Sprite>(path);
        
        if (sprites == null || sprites.Length == 0)
        {
            spritesLoaded = true;
            yield break;
        }

        // Spread loading across frames
        foreach (Sprite sprite in sprites)
        {
            if (sprite == null) continue;
            
            if (!SpriteCache.ContainsKey(sprite.name))
            {
                SpriteCache.Add(sprite.name, sprite);
            }
            
            // Apply performance limits
            if (interLoadDelay > 0) yield return new WaitForSeconds(interLoadDelay);
        }
        
        spritesLoaded = true;
    }

    IEnumerator LoadAudio(string path)
    {
        // Load all audio at once
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
            
            // Apply performance limits
            if (interLoadDelay > 0) yield return new WaitForSeconds(interLoadDelay);
        }
        
        audioLoaded = true;
    }

    private void LoadNextScene()
    {
        string sceneName = "Scenes/Levels/" + Song;
        
        if (resourcesAvailable && Application.CanStreamedLevelBeLoaded(sceneName))
        {
            SafeTransition(sceneName);
        }
        else
        {
            FallbackToMainMenu();
        }
    }

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

    // Sprite helpers
    public static Sprite GetSprite(string key)
    {
        if (SpriteCache != null && SpriteCache.TryGetValue(key, out var sprite))
        {
            return sprite;
        }
        return null;
    }

    public static bool TryGetSprite(string key, out Sprite sprite)
    {
        sprite = null;
        if (SpriteCache == null) return false;
        return SpriteCache.TryGetValue(key, out sprite);
    }

    // Audio helpers
    public static AudioClip GetAudio(string key)
    {
        if (AudioCache != null && AudioCache.TryGetValue(key, out var clip))
        {
            return clip;
        }
        return null;
    }

    public static bool TryGetAudio(string key, out AudioClip clip)
    {
        clip = null;
        if (AudioCache == null) return false;
        return AudioCache.TryGetValue(key, out clip);
    }

    private void OnDestroy()
    {
        // Clean up resources
        if (SpriteCache != null) SpriteCache.Clear();
        if (AudioCache != null) AudioCache.Clear();
        Resources.UnloadUnusedAssets();
    }
}