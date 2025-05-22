using UnityEngine;

public class ActionTimer : MonoBehaviour
{
    [Header("Settings")]
    public float targetTimeInSeconds = 30f;   // Time at which the color change should start
    public GameObject colorChangeObject;      // The object to change color

    [Header("Color Change Settings")]
    public float colorChangeDuration = 1.7f;  // Duration of the color change
    public Color targetColor = Color.green;   // The color to fade to

    private MusicTimer musicTimer;            // Reference to the MusicTimer
    private SpriteRenderer spriteRenderer;    // SpriteRenderer of the color change object
    private Color initialColor;               // Initial color of the object
    private float colorChangeStartTime;       // Time when the color change starts (Time.time)
    private float colorChangeStartMusicTime;  // Music time when color change starts
    private bool isColorChanging = false;     // Flag to check if the color change is in progress
    private bool hasStartedColorChange = false; // To prevent starting the color change multiple times

    private BoxCollider2D boxCollider;       // Reference to the BoxCollider2D attached to the object

    private float delay;
    public float Delay => delay;

    [System.Obsolete]
    void Start()
    {
        musicTimer = FindObjectOfType<MusicTimer>();
        if (musicTimer == null)
        {
            Debug.LogWarning("[ActionTimer] No MusicTimer found in the scene.");
            return;
        }

        if (colorChangeObject != null)
        {
            spriteRenderer = colorChangeObject.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                initialColor = spriteRenderer.color;
            }
            else
            {
                Debug.LogWarning("[ActionTimer] No SpriteRenderer found on the target object.");
            }
        }
        else
        {
            Debug.LogWarning("[ActionTimer] No object to change color assigned.");
        }

        boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider == null)
        {
            Debug.LogWarning("[ActionTimer] No BoxCollider2D found on this object.");
        }
    }

    void Update()
    {
        if (musicTimer == null || spriteRenderer == null || boxCollider == null) return;

        float currentTime = musicTimer.CurrentTime;

        // Check if the color change should start
        if (!hasStartedColorChange && currentTime >= targetTimeInSeconds - 2f)
        {
            hasStartedColorChange = true;
            StartColorChange();
        }


        UpdateDelay(currentTime);
        

        if (isColorChanging)
        {
            float elapsedTime = Time.time - colorChangeStartTime;
            float lerpProgress = Mathf.Clamp01(elapsedTime / colorChangeDuration);
            spriteRenderer.color = Color.Lerp(initialColor, targetColor, lerpProgress);

            if (lerpProgress >= 1f)
            {
                isColorChanging = false;
                Debug.Log("[ActionTimer] Color change completed.");
            }
        }
    }

    private void StartColorChange()
    {
        Debug.Log("[ActionTimer] Starting color change.");
        colorChangeStartTime = Time.time;
        colorChangeStartMusicTime = musicTimer.CurrentTime;
        isColorChanging = true;
    }

    public void LogCurrentMusicTime()
    {
        if (musicTimer == null)
        {
            Debug.LogWarning("[ActionTimer] Cannot log time - MusicTimer is missing.");
            return;
        }

        float time = musicTimer.CurrentTime;
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        int milliseconds = Mathf.FloorToInt((time * 1000) % 1000);

        Debug.Log($"[ActionTimer] Music Time: {minutes:00}:{seconds:00}.{milliseconds:000}");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        float currentTime = musicTimer.CurrentTime;

        if (hasStartedColorChange && currentTime >= colorChangeStartMusicTime && currentTime <= colorChangeStartMusicTime + 2f)
        {
            UpdateDelay(currentTime);
            Debug.Log($"[ActionTimer] Collision started, delay: {delay:0.00}ms");
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        Debug.Log($"[ActionTimer] Collision ended, final delay: {delay:0.00}ms");
    }

    private void UpdateDelay(float currentTime)
    {
        // Ensure that we only update delay within the 2-second window after the color change starts
        if (currentTime >= colorChangeStartMusicTime && currentTime <= colorChangeStartMusicTime + 2f)
        {
            delay = (currentTime - colorChangeStartMusicTime) * 100f;  // Update delay based on music time
        }
    }
}
