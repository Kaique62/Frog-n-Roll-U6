using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelPreload : MonoBehaviour
{
    [Header("Preload Settings")]
    public string Song = "";

    public static Dictionary<string, Sprite> SpriteCache { get; private set; }
    public static Dictionary<string, AudioClip> AudioCache { get; private set; }

    private void Start()
    {
        SpriteCache = new Dictionary<string, Sprite>();
        AudioCache = new Dictionary<string, AudioClip>();
        StartCoroutine(PreloadFiles());
    }

    IEnumerator PreloadFiles()
    {
        string spritesFolderPath = "Preload/" + Song;
        string musicFolderPath = "Musics/" + Song;

        Debug.Log($"[Preloader] Starting preload from: {spritesFolderPath}");

        // Load Sprites
        var sprites = Resources.LoadAll<Sprite>(spritesFolderPath);
        if (sprites.Length > 0)
        {
            foreach (var sprite in sprites)
            {
                SpriteCache[sprite.name] = sprite;
                Debug.Log($"[Preloader] Cached sprite: {sprite.name}");
                yield return null;
            }
        }
        else
        {
            Debug.LogWarning($"[Preloader] No sprites found at: {spritesFolderPath}");
        }

        // Load Audio
        var audioClips = Resources.LoadAll<AudioClip>(musicFolderPath);
        if (audioClips.Length > 0)
        {
            foreach (var clip in audioClips)
            {
                AudioCache[clip.name] = clip;
                Debug.Log($"[Preloader] Cached audio: {clip.name}");
                yield return null;
            }
        }
        else
        {
            Debug.LogWarning($"[Preloader] No audio clips found at: {musicFolderPath}");
        }

        LoadNextScene();
    }

    private void LoadNextScene()
    {
        string sceneName = "Scenes/Levels/" + Song;
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError($"[Preloader] Scene not found: {sceneName}");
        }
    }

    // Sprite helpers
    public static Sprite GetSprite(string key)
    {
        if (SpriteCache != null && SpriteCache.TryGetValue(key, out var sprite))
        {
            return sprite;
        }
        Debug.LogWarning($"[Preloader] Sprite '{key}' not found in cache");
        return null;
    }

    public static bool TryGetSprite(string key, out Sprite sprite)
    {
        sprite = null;
        return SpriteCache != null && SpriteCache.TryGetValue(key, out sprite);
    }

    // Audio helpers
    public static bool TryGetAudio(string key, out AudioClip clip)
    {
        clip = null;
        return AudioCache != null && AudioCache.TryGetValue(key, out clip);
    }
}
