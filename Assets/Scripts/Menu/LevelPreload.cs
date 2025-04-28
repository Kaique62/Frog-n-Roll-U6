using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelPreload : MonoBehaviour
{
    [Header("Preload Settings")]
    public string Song = "";

    // Static cache accessible from anywhere
    public static Dictionary<string, Sprite> SpriteCache { get; private set; }
    public static AudioClip MusicClip { get; private set; }

    private void Start()
    {
        SpriteCache = new Dictionary<string, Sprite>();
        StartCoroutine(PreloadFiles());
    }

    IEnumerator PreloadFiles()
    {
        string spritesFolderPath = "Preload/" + Song;
        string musicFilePath = "Musics/" + Song;

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

        // Load Music
        MusicClip = Resources.Load<AudioClip>(musicFilePath);
        if (MusicClip != null)
        {
            Debug.Log($"[Preloader] Music loaded: {MusicClip.name}");
        }
        else
        {
            Debug.LogWarning($"[Preloader] Failed to load music at: {musicFilePath}");
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

    // Helper methods for convenience
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
}