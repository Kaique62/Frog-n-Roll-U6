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
        var rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();

        foreach (GameObject rootObj in rootObjects)
        {
            if (rootObj.transform == this.transform || rootObj.transform.IsChildOf(this.transform))
                continue;

            ProcessObject(rootObj);
        }
    }

    private void ProcessObject(GameObject targetObj)
    {
        if (!targetObj.activeInHierarchy)
            return;

        // SpriteRenderer logic
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
                else if (destroyInvalidObjects)
                {
                    Debug.LogWarning($"[UniversalLoader] Destroying {targetObj.name} - sprite not found", targetObj);
                    Destroy(targetObj);
                    return;
                }
            }
        }

        // AudioSource logic
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

        // Recursive processing
        foreach (Transform child in targetObj.transform)
        {
            ProcessObject(child.gameObject);
        }
    }
}
