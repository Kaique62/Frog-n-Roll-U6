using TMPro;
using UnityEngine;

/// <summary>
/// Controls the gameplay logic for a level, including score tracking,
/// music playback, and score multiplier calculations based on timing.
/// </summary>
public class LevelController : MonoBehaviour
{
    [Header("Score System")]
    public float score = 0;
    public TMP_Text scoreText;
    public TMP_Text scoreTextMultiplier;
    public float pointMultiplier = 1f;

    [Header("Music")]
    public AudioSource musicAudioSource;

    private bool gameStarted = false;

    /// <summary>
    /// Initializes score display when the level starts.
    /// </summary>
    void Start()
    {
        UpdateScoreUI();
    }

    /// <summary>
    /// Waits for any key press to start the level.
    /// </summary>
    void Update()
    {
        if (!gameStarted && Input.anyKey)
        {
            StartLevel();
        }
    }

    /// <summary>
    /// Begins the level by starting the music and flagging the game as started.
    /// Sets the music volume based on saved settings.
    /// </summary>
    private void StartLevel()
    {
        gameStarted = true;

        if (musicAudioSource != null && !musicAudioSource.isPlaying)
        {
            musicAudioSource.volume = LoadSavedVolume();
            Debug.Log($"Audio started with volume: {musicAudioSource.volume}");
            musicAudioSource.Play();
        }
    }

    /// <summary>
    /// Adds points to the current score based on a given delay and updates the multiplier.
    /// Higher delays reward more points and increase the multiplier.
    /// Missing a time will reset the multiplier
    /// </summary>
    /// <param name="delay">The timing delay used to evaluate the score quality.</param>
    /// <remarks>
    /// The multiplier is not capped, which could lead to exponential score growth.
    /// I May need to cap this later, idk tbh, big numbers are kinda funny to see on a score.
    /// </remarks>
    public void AddScore(float delay)
    {
        float points = 0;

        if (delay >= 170)
        {
            points = 1000;
            pointMultiplier += 0.3f;
        }
        else if (delay >= 150)
        {
            points = 700;
            pointMultiplier += 0.1f;
        }
        else if (delay >= 120)
        {
            points = 500;
            pointMultiplier = 1f;
        }
        else
        {
            points = 300;
            pointMultiplier = 1f;
        }

        points *= pointMultiplier;
        score += points;
        UpdateScoreUI();
    }

    /// <summary>
    /// Resets the current score and multiplier display.
    /// </summary>
    public void ResetScore()
    {
        score = 0;
        UpdateScoreUI();
    }

    /// <summary>
    /// Updates the UI elements for score and multiplier.
    /// </summary>
    private void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = score.ToString();

        if (scoreTextMultiplier != null)
            scoreTextMultiplier.text = $"X{pointMultiplier:F1}";
    }

    /// <summary>
    /// Loads the saved music volume from persistent configuration.
    /// Defaults to 1 if no value is found or parsing fails.
    /// </summary>
    /// <returns>A float value between 0 and 1 representing the volume level.</returns>
    private float LoadSavedVolume()
    {
        string savedVolume = ConfigManager.Read("volume_MusicVolume");
        if (float.TryParse(savedVolume, out float volume))
            return Mathf.Clamp01(volume);

        return 1f;
    }
}
