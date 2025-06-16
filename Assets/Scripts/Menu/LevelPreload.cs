using System.Collections;
using System.Collections.Generic;
using EasyTransition;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles preloading of sprites and audio for a level based on a song name.
/// Works with WebGL by avoiding platform-specific memory checks.
/// Supports persistence between scenes for cross-scene cache usage.
/// </summary>
public class LevelPreload : MonoBehaviour
{
    [Header("Preload Settings")]
    public string Song = "";

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

    // Singleton instance para persistir entre cenas
    private static LevelPreload _instance;

    // Indica se o preload terminou (publico para outros scripts esperarem)
    public static bool IsReady { get; private set; } = false;

    private void Awake()
    {
        // Singleton para garantir uma instância única e persistente
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Inicializa caches somente se ainda não inicializados
        if (SpriteCache == null)
            SpriteCache = new Dictionary<string, Sprite>();

        if (AudioCache == null)
            AudioCache = new Dictionary<string, AudioClip>();
    }

    private void Start()
    {
        IsReady = false;
        StartCoroutine(PreloadFiles());
    }

    IEnumerator PreloadFiles()
    {
        string spritesFolderPath = "Preload/" + Song;
        string musicFolderPath = "Musics/" + Song;

        // Start both coroutines, espera os dois terminarem
        var spritesCoroutine = StartCoroutine(LoadSprites(spritesFolderPath));
        var audioCoroutine = StartCoroutine(LoadAudio(musicFolderPath));

        yield return spritesCoroutine;
        yield return audioCoroutine;

        spritesLoaded = true;
        audioLoaded = true;

        IsReady = true;

        LoadNextScene();
    }

    IEnumerator LoadSprites(string path)
    {
        Debug.Log($"[LevelPreload] Carregando sprites de: Resources/{path}");

        Object[] rawObjects = Resources.LoadAll(path);
        Debug.Log($"[LevelPreload] Objetos brutos encontrados: {rawObjects.Length}");
        foreach (var obj in rawObjects)
        {
            Debug.Log($"[LevelPreload] -> {obj.name} ({obj.GetType().Name})");
        }

        Sprite[] sprites = Resources.LoadAll<Sprite>(path);
        Debug.Log($"[LevelPreload] Sprites válidos encontrados: {sprites.Length}");

        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogWarning($"[LevelPreload] Nenhum sprite encontrado em: Resources/{path}");
            yield break;
        }

        float frameStartTime = Time.realtimeSinceStartup;
        foreach (Sprite sprite in sprites)
        {
            if (sprite == null) continue;

            if (!SpriteCache.ContainsKey(sprite.name))
            {
                SpriteCache.Add(sprite.name, sprite);
                Debug.Log($"[LevelPreload] Sprite armazenado no cache: {sprite.name}");
            }

            if (interLoadDelay > 0)
                yield return new WaitForSeconds(interLoadDelay);

            // Limita o tempo de carga por frame para evitar travamento
            if ((Time.realtimeSinceStartup - frameStartTime) * 1000f >= maxLoadTimePerFrame)
            {
                yield return null;
                frameStartTime = Time.realtimeSinceStartup;
            }
        }

        Debug.Log($"[LevelPreload] Concluído: {sprites.Length} sprites carregados.");
    }

    IEnumerator LoadAudio(string path)
    {
        Debug.Log($"[LevelPreload] Carregando áudios de: Resources/{path}");

        Object[] rawObjects = Resources.LoadAll(path);
        Debug.Log($"[LevelPreload] Objetos brutos encontrados: {rawObjects.Length}");
        foreach (var obj in rawObjects)
        {
            Debug.Log($"[LevelPreload] -> {obj.name} ({obj.GetType().Name})");
        }

        AudioClip[] clips = Resources.LoadAll<AudioClip>(path);
        Debug.Log($"[LevelPreload] Áudios válidos encontrados: {clips.Length}");

        if (clips == null || clips.Length == 0)
        {
            Debug.LogWarning($"[LevelPreload] Nenhum áudio encontrado em: Resources/{path}");
            yield break;
        }

        float frameStartTime = Time.realtimeSinceStartup;
        foreach (AudioClip clip in clips)
        {
            if (clip == null) continue;

            if (!AudioCache.ContainsKey(clip.name))
            {
                AudioCache.Add(clip.name, clip);
                Debug.Log($"[LevelPreload] Áudio armazenado no cache: {clip.name}");
            }

            if (interLoadDelay > 0)
                yield return new WaitForSeconds(interLoadDelay);

            // Limita o tempo de carga por frame para evitar travamento
            if ((Time.realtimeSinceStartup - frameStartTime) * 1000f >= maxLoadTimePerFrame)
            {
                yield return null;
                frameStartTime = Time.realtimeSinceStartup;
            }
        }

        Debug.Log($"[LevelPreload] Concluído: {clips.Length} áudios carregados.");
    }

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
        // Só limpa cache se for a instância principal (singleton)
        if (_instance == this)
        {
            SpriteCache?.Clear();
            AudioCache?.Clear();
            Resources.UnloadUnusedAssets();
            IsReady = false;
        }
    }
}
