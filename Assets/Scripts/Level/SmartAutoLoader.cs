using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoSmartLoad : MonoBehaviour
{
    [Header("Auto Smart Load Settings")]
    public string spriteName;  // Name of the sprite to load from the cache.
    public Sprite fallbackSprite; // Fallback sprite to use if the sprite is not found in cache.
    public bool useAsyncLoad = true;  // Flag to enable or disable asynchronous loading

    private SpriteRenderer spriteRenderer;
    private bool isSpriteLoaded = false;

    private static Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();  // Cache of loaded sprites

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("[AutoSmartLoad] No SpriteRenderer found on this GameObject!");
        }
    }

    private void Start()
    {
        // Start loading the sprite with or without async loading
        StartCoroutine(LoadSprite());
    }

    private IEnumerator LoadSprite()
    {
        // Check if the sprite is already cached
        Sprite cachedSprite = GetCachedSprite(spriteName);

        if (cachedSprite != null)
        {
            // If sprite is cached, use it
            spriteRenderer.sprite = cachedSprite;
            Debug.Log($"[AutoSmartLoad] Loaded cached sprite: {spriteName}");
            isSpriteLoaded = true;
        }
        else
        {
            // If sprite is not in cache, try to load it
            Debug.LogWarning($"[AutoSmartLoad] Sprite {spriteName} not found in cache, loading...");

            if (useAsyncLoad)
            {
                // Async load the sprite from Resources
                yield return LoadSpriteAsync();
            }
            else
            {
                // Synchronous load the sprite from Resources
                yield return LoadSpriteSync();
            }
        }

        // If no sprite is loaded, use fallback sprite
        if (!isSpriteLoaded)
        {
            Debug.LogWarning("[AutoSmartLoad] Sprite not loaded. Using fallback sprite.");
            spriteRenderer.sprite = fallbackSprite;
        }

        // Optionally, give feedback that loading is complete (e.g., for UI or animation purposes)
        yield return new WaitForSeconds(1f);
        Debug.Log("[AutoSmartLoad] Finished loading sprite.");
    }

    private IEnumerator LoadSpriteAsync()
    {
        // Load the sprite asynchronously from Resources
        ResourceRequest spriteRequest = Resources.LoadAsync<Sprite>("Preload/" + spriteName);
        yield return spriteRequest;

        if (spriteRequest.isDone && spriteRequest.asset != null)
        {
            // Cache the sprite after loading
            spriteCache[spriteName] = spriteRequest.asset as Sprite;
            spriteRenderer.sprite = spriteRequest.asset as Sprite;
            isSpriteLoaded = true;
            Debug.Log($"[AutoSmartLoad] Asynchronously loaded and cached sprite: {spriteName}");
        }
        else
        {
            Debug.LogWarning($"[AutoSmartLoad] Failed to asynchronously load sprite: {spriteName}");
        }
    }

    private IEnumerator LoadSpriteSync()
    {
        // Load the sprite synchronously from Resources
        Sprite sprite = Resources.Load<Sprite>("Preload/" + spriteName);

        if (sprite != null)
        {
            // Cache the sprite after loading
            spriteCache[spriteName] = sprite;
            spriteRenderer.sprite = sprite;
            isSpriteLoaded = true;
            Debug.Log($"[AutoSmartLoad] Synchronously loaded and cached sprite: {spriteName}");
        }
        else
        {
            Debug.LogWarning($"[AutoSmartLoad] Failed to synchronously load sprite: {spriteName}");
        }

        yield return null;
    }

    private Sprite GetCachedSprite(string name)
    {
        // Return the sprite from the cache, or null if it doesn't exist
        if (spriteCache.ContainsKey(name))
        {
            return spriteCache[name];
        }
        return null;
    }

    // Optional method to clear the cache when no longer needed (e.g., between scenes)
    public static void ClearCache()
    {
        spriteCache.Clear();
        Debug.Log("[AutoSmartLoad] Sprite cache cleared.");
    }

    private void Update()
    {
        // You can add any additional logic here if needed, for example,
        // to handle sprite changes, animations, etc.
        if (!isSpriteLoaded)
        {
          //  Debug.Log("[AutoSmartLoad] Waiting for sprite to load...");
        }
    }
}
