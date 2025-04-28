using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelPreload : MonoBehaviour
{
    [Header("Preload Settings")]
    public string Song = "";  // The name of the song, used to determine the folder and the scene to load.
    
    private AudioSource audioSource;  // Reference to the AudioSource for playing music
    private Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();  // Cache for sprites

    private void Awake()
    {
        // Ensure we have an AudioSource component attached to the same GameObject
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("[Preloader] No AudioSource component found on this GameObject.");
        }
    }

    private void Start()
    {
        StartCoroutine(PreloadFiles());
    }

    IEnumerator PreloadFiles()
    {
        // Define the folder path inside Resources, corresponding to the song
        string spritesFolderPath = "Preload/" + Song;
        string musicFilePath = "Musics/" + Song;

        Debug.Log($"[Preloader] Starting preload from Resources folder: {spritesFolderPath} for sprites and {musicFilePath} for music.");

        // Try to load all sprites from Resources/Preload/Song folder
        var sprites = Resources.LoadAll<Sprite>(spritesFolderPath);

        if (sprites.Length == 0)
        {
            // If no sprites were found in the folder, log a warning and skip to the next scene
            Debug.LogWarning($"[Preloader] No assets found in Resources/{spritesFolderPath}. Skipping preload.");
        }
        else
        {
            // Cache the loaded sprites
            foreach (var sprite in sprites)
            {
                spriteCache[sprite.name] = sprite;
                Debug.Log($"[Preloader] Cached sprite: {sprite.name}");
                yield return null;  // Optional delay, good for visual loading screens
            }
        }

        // Try to load the music from Resources/Musics/Song.mp3
        AudioClip musicClip = Resources.Load<AudioClip>(musicFilePath);

        if (musicClip != null)
        {
            // If the music file is found, load and play it
            audioSource.clip = musicClip;
            audioSource.Play();
            Debug.Log($"[Preloader] Music loaded and playing: {Song}");
        }
        else
        {
            Debug.LogWarning($"[Preloader] Failed to load music: {musicFilePath}");
        }

        Debug.Log("[Preloader] Finished preloading all files!");

        // After preloading finishes, load the next scene
        LoadNextScene();
    }

    // Function to get cached sprite by key
    public Sprite GetCachedSprite(string key)
    {
        if (spriteCache.ContainsKey(key))
        {
            return spriteCache[key];
        }
        else
        {
            Debug.LogWarning($"[Preloader] Sprite with key '{key}' not found in cache.");
            return null;
        }
    }

    // Function to load the next scene based on the Song name
    private void LoadNextScene()
    {
        string sceneName = "Scenes/Levels/" + Song;  // Assuming the scene name is "Levels/Song"
        Debug.Log($"[Preloader] Loading next scene: {sceneName}");

        // Check if the scene exists in the build settings
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            SceneManager.LoadScene(sceneName);
            Debug.Log($"[Preloader] Scene loaded successfully: {sceneName}");
        }
        else
        {
            Debug.LogError($"[Preloader] Scene not found: {sceneName}");
        }
    }
}
