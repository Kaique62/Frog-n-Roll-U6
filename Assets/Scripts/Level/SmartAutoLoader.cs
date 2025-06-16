using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Automatically assigns sprites and audio clips to GameObjects in the active scene
/// based on their names using the LevelPreload system.
/// Can also destroy objects if no matching assets are found.
/// This is important for the preload system to actually work instead of loading the same items twice.
/// </summary>
public class UniversalSmartLoader : MonoBehaviour
{
    [Header("Settings")]

    public bool processSprites = true;
    public bool processAudio = true;
    public bool destroyInvalidObjects = false;

    [Header("Debug")]

    public bool logDetails = true;

    // Local caches for lookup to avoid multiple dictionary queries during processing
    private Dictionary<string, Sprite> spriteCache;
    private Dictionary<string, AudioClip> audioCache;

    private void Start()
    {
        // Copy references to local dictionaries for faster access
        spriteCache = LevelPreload.SpriteCache;
        audioCache = LevelPreload.AudioCache;

        ProcessAllObjects();
    }

    private void ProcessAllObjects()
    {
        var rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();

        foreach (GameObject rootObj in rootObjects)
        {
            // Skip self and children
            if (rootObj.transform == this.transform || rootObj.transform.IsChildOf(this.transform))
                continue;

            ProcessObject(rootObj);
        }
    }

    private void ProcessObject(GameObject targetObj)
    {
        if (!targetObj.activeInHierarchy)
            return;

        string objName = targetObj.name;

        // Process SpriteRenderer
        if (processSprites)
        {
            var spriteRenderer = targetObj.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && !string.IsNullOrEmpty(objName))
            {
                if (spriteCache != null && spriteCache.TryGetValue(objName, out var sprite))
                {
                    spriteRenderer.sprite = sprite;
                    if (logDetails)
                        Debug.Log($"[UniversalLoader] Applied sprite '{objName}' to {objName}", targetObj);
                }
                else if (destroyInvalidObjects)
                {
                    Debug.LogWarning($"[UniversalLoader] Destroying {objName} - sprite not found", targetObj);
                    Destroy(targetObj);
                    return;
                }
            }
        }

        // Process AudioSource
        if (processAudio)
        {
            var audioSource = targetObj.GetComponent<AudioSource>();
            if (audioSource != null && !string.IsNullOrEmpty(objName))
            {
                if (audioCache != null && audioCache.TryGetValue(objName, out var clip))
                {
                    audioSource.clip = clip;
                    if (logDetails)
                        Debug.Log($"[UniversalLoader] Applied audio '{objName}' to {objName}", targetObj);
                }
                else if (destroyInvalidObjects)
                {
                    Debug.LogWarning($"[UniversalLoader] Destroying {objName} - audio not found", targetObj);
                    Destroy(targetObj);
                    return;
                }
            }
        }

        // Process children recursively
        foreach (Transform child in targetObj.transform)
        {
            ProcessObject(child.gameObject);
        }
    }
}
