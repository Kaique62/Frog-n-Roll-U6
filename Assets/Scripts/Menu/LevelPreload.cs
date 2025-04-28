using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelPreload : MonoBehaviour
{
    [Header("Preload Settings")]
    public string Song = "";

    private Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

    private void Start()
    {
        Song = LevelData.song;
        StartCoroutine(PreloadFiles());
    }

    IEnumerator PreloadFiles()
    {
        Debug.Log($"[Preloader] Starting preload from Resources: Preload/{Song}");

        // Load ALL sprites from Preload/Song and ALL subfolders
        Object[] allLoadedAssets = Resources.LoadAll($"Preload/{Song}", typeof(Sprite));

        if (allLoadedAssets == null || allLoadedAssets.Length == 0)
        {
            Debug.LogError($"[Preloader] No assets found at Preload/{Song}");
            yield break;
        }

        Debug.Log($"[Preloader] Found {allLoadedAssets.Length} assets to preload.");

        foreach (var obj in allLoadedAssets)
        {
            if (obj is Sprite sprite)
            {
                if (!spriteCache.ContainsKey(sprite.name))
                {
                    spriteCache.Add(sprite.name, sprite);
                    Debug.Log($"[Preloader] Cached sprite: {sprite.name}");
                }
                else
                {
                    Debug.LogWarning($"[Preloader] Duplicate sprite name detected: {sprite.name}");
                }
            }

            yield return null; // Small delay for smooth loading screens
        }

        Debug.Log("[Preloader] Finished preloading all sprites!");
    }

    public Sprite GetCachedSprite(string spriteName)
    {
        if (spriteCache.TryGetValue(spriteName, out Sprite sprite))
        {
            return sprite;
        }
        else
        {
            Debug.LogWarning($"[Preloader] Sprite not found in cache: {spriteName}");
            return null;
        }
    }
}
