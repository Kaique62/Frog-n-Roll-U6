using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Automatically assigns sprites and audio clips to GameObjects in the active scene
/// based on their names using the LevelPreload system.
/// Can also destroy objects if no matching assets are found.
/// This is Important for the proload system actually works instead of loading the same items twice.
/// This is not really necessary by now since it's a demo, but might be usefull for later.
/// </summary>
public class UniversalSmartLoader : MonoBehaviour
{
    [Header("Settings")]

    /// <summary>
    /// If true, attempts to load and assign sprites to SpriteRenderers based on GameObject names.
    /// </summary>
    public bool processSprites = true;

    /// <summary>
    /// If true, attempts to load and assign audio clips to AudioSources based on GameObject names.
    /// </summary>
    public bool processAudio = true;

    /// <summary>
    /// If true, destroys GameObjects that fail to find a matching sprite or audio clip.
    /// </summary>
    public bool destroyInvalidObjects = false;

    [Header("Debug")]

    /// <summary>
    /// If true, logs detailed loading actions to the console for debugging purposes.
    /// </summary>
    public bool logDetails = true;

    /// <summary>
    /// Called on script start. Begins processing all root objects in the scene.
    /// </summary>
    private void Start()
    {
        ProcessAllObjects();
    }

    /// <summary>
    /// Processes all root GameObjects in the active scene,
    /// skipping the current GameObject and its children.
    /// </summary>
    private void ProcessAllObjects()
    {
        var rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();

        foreach (GameObject rootObj in rootObjects)
        {
            if (rootObj.transform == this.transform || rootObj.transform.IsChildOf(this.transform))
                continue;

            ProcessObject(rootObj);
        }
    }

    /// <summary>
    /// Recursively processes a GameObject and its children for sprite and audio assignment.
    /// </summary>
    /// <param name="targetObj">The GameObject to process.</param>
    private void ProcessObject(GameObject targetObj)
    {
        if (!targetObj.activeInHierarchy)
            return;

        // Process SpriteRenderer
        if (processSprites)
        {
            var spriteRenderer = targetObj.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && !string.IsNullOrEmpty(targetObj.name))
            {
                if (LevelPreload.TryGetSprite(targetObj.name, out var sprite))
                {
                    spriteRenderer.sprite = sprite;
                    if (logDetails)
                        Debug.Log($"[UniversalLoader] Applied sprite '{targetObj.name}' to {targetObj.name}", targetObj);
                }
                // Destroy Objects if the destroyInvalidObjects is set to True.
                else if (destroyInvalidObjects)
                {
                    Debug.LogWarning($"[UniversalLoader] Destroying {targetObj.name} - sprite not found", targetObj);
                    Destroy(targetObj);
                    return;
                }
            }
        }

        // Process AudioSource || Audio Preload
        if (processAudio)
        {
            var audioSource = targetObj.GetComponent<AudioSource>();
            if (audioSource != null && !string.IsNullOrEmpty(targetObj.name))
            {
                if (LevelPreload.TryGetAudio(targetObj.name, out var clip))
                {
                    audioSource.clip = clip;
                    if (logDetails)
                        Debug.Log($"[UniversalLoader] Applied audio '{targetObj.name}' to {targetObj.name}", targetObj);
                }
                else if (destroyInvalidObjects)
                {
                    Debug.LogWarning($"[UniversalLoader] Destroying {targetObj.name} - audio not found", targetObj);
                    Destroy(targetObj);
                    return;
                }
            }
        }

        // Recursively process children
        foreach (Transform child in targetObj.transform)
        {
            ProcessObject(child.gameObject);
        }
    }
}
