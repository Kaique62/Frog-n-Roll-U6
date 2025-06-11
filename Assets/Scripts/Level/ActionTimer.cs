using UnityEngine;

/// <summary>
/// Controls a timed color change on a target GameObject synchronized with a music timer,
/// and tracks collision delay during the color change window.
/// </summary>
public class ActionTimer : MonoBehaviour
{
    [Header("Settings")]
    /// <summary>Music time (in seconds) to start the color change.</summary>
    public float targetTimeInSeconds = 30f;

    /// <summary>The GameObject whose color will be changed.</summary>
    public GameObject colorChangeObject;

    [Header("Color Change Settings")]
    /// <summary>Duration (in seconds) of the color transition.</summary>
    public float colorChangeDuration = 1.7f;

    /// <summary>The color to fade to during the color change.</summary>
    public Color targetColor = Color.green;

    private MusicTimer musicTimer;            // Reference to the MusicTimer
    private SpriteRenderer spriteRenderer;    // SpriteRenderer of the colorChangeObject
    private Color initialColor;                // Initial color of the object
    private float colorChangeStartTime;       // Real time when color change starts
    private float colorChangeStartMusicTime;  // Music time when color change starts
    private bool isColorChanging = false;     // Is the color change in progress?
    private bool hasStartedColorChange = false; // Prevent multiple starts

    private BoxCollider2D boxCollider;       // Collider on this object

    private float delay;
    /// <summary>Delay measured during collision, in milliseconds.</summary>
    public float Delay => delay;

    [System.Obsolete]
    private void Start()
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
                initialColor = spriteRenderer.color;
            else
                Debug.LogWarning("[ActionTimer] No SpriteRenderer found on the target object.");
        }
        else
        {
            Debug.LogWarning("[ActionTimer] No object to change color assigned.");
        }

        boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider == null)
            Debug.LogWarning("[ActionTimer] No BoxCollider2D found on this object.");
    }

    private void Update()
    {
        if (musicTimer == null || spriteRenderer == null || boxCollider == null) return;

        float currentTime = musicTimer.CurrentTime;

        // Start color change 2 seconds before target time
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

    /// <summary>
    /// Initializes variables and starts the color change interpolation.
    /// </summary>
    private void StartColorChange()
    {
        Debug.Log("[ActionTimer] Starting color change.");
        colorChangeStartTime = Time.time;
        colorChangeStartMusicTime = musicTimer.CurrentTime;
        isColorChanging = true;
    }

    /// <summary>
    /// Logs the current music time in MM:SS.mmm format.
    /// </summary>
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

    /// <summary>
    /// Called when a collision starts. Updates and logs delay if within color change window.
    /// </summary>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        float currentTime = musicTimer.CurrentTime;

        if (hasStartedColorChange && currentTime >= colorChangeStartMusicTime && currentTime <= colorChangeStartMusicTime + 2f)
        {
            UpdateDelay(currentTime);
            Debug.Log($"[ActionTimer] Collision started, delay: {delay:0.00}ms");
        }
    }

    /// <summary>
    /// Called when a collision ends. Logs the final delay.
    /// </summary>
    private void OnCollisionExit2D(Collision2D collision)
    {
        Debug.Log($"[ActionTimer] Collision ended, final delay: {delay:0.00}ms");
    }

    /// <summary>
    /// Updates the delay value based on the elapsed music time within the 2-second window.
    /// </summary>
    /// <param name="currentTime">Current music time.</param>
    private void UpdateDelay(float currentTime)
    {
        if (currentTime >= colorChangeStartMusicTime && currentTime <= colorChangeStartMusicTime + 2f)
        {
            delay = (currentTime - colorChangeStartMusicTime) * 100f;
        }
    }
}
