using UnityEngine;

public class UniversalSmartLoader : MonoBehaviour
{
    [Header("Settings")]
    public bool processSprites = true;
    public bool processAudio = true;
    public bool destroyInvalidObjects = false;
    
    [Header("Debug")]
    public bool logDetails = true;
    
    private void Start()
    {
        ProcessAllObjects();
    }

    private void ProcessAllObjects()
    {
        // Get all root objects in the scene
        GameObject[] rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();

        foreach (GameObject rootObj in rootObjects)
        {
            // Skip the loader's own hierarchy to avoid infinite loops
            if (rootObj.transform == this.transform || rootObj.transform.IsChildOf(this.transform))
                continue;

            // Process all children recursively
            ProcessObject(rootObj);
        }
    }

    private void ProcessObject(GameObject targetObj)
    {
        // Skip inactive objects
        if (!targetObj.activeInHierarchy)
            return;

        // Process SpriteRenderers
        if (processSprites)
        {
            SpriteRenderer spriteRenderer = targetObj.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && !string.IsNullOrEmpty(targetObj.name))
            {
                if (LevelPreload.TryGetSprite(targetObj.name, out Sprite sprite))
                {
                    spriteRenderer.sprite = sprite;
                    if (logDetails)
                        Debug.Log($"[UniversalLoader] Applied sprite '{targetObj.name}' to {targetObj.name}", targetObj);
                }
                else if (destroyInvalidObjects)
                {
                    Debug.LogWarning($"[UniversalLoader] Destroying {targetObj.name} - sprite not found", targetObj);
                    Destroy(targetObj);
                    return; // Skip further processing if destroyed
                }
            }
        }

        // Process AudioSources
        if (processAudio)
        {
            AudioSource audioSource = targetObj.GetComponent<AudioSource>();
            if (audioSource != null && LevelPreload.MusicClip != null)
            {
                audioSource.clip = LevelPreload.MusicClip;
                audioSource.Play();
                if (logDetails)
                    Debug.Log($"[UniversalLoader] Playing music on {targetObj.name}", targetObj);
            }
        }

        // Recursively process children
        foreach (Transform child in targetObj.transform)
        {
            ProcessObject(child.gameObject);
        }
    }
}