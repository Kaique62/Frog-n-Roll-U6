using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelController : MonoBehaviour
{
    [Header("Audio & Countdown")]
    public AudioSource musicAudioSource;
    public float countdownTime = 3f;
    public GameObject countdownUI;

    [Header("Score System")]
    public float score = 0;                          // Current score
    public TMP_Text scoreText;                         // Optional UI Text to display the score
    public TMP_Text scoreTextMultiplier;                         // Optional UI Text to display the score
    
    public float pointMultiplier = 1;
    void Start()
    {
        StartCoroutine(StartLevel());
        UpdateScoreUI(); // Initialize score display
    }

    IEnumerator StartLevel()
    {
        if (countdownUI != null)
            countdownUI.SetActive(true);

        yield return StartCoroutine(Countdown());

        if (musicAudioSource != null && !musicAudioSource.isPlaying)
        {
            Debug.Log("audioStart");
            musicAudioSource.Play();
        }

        // You could trigger gameplay start here
    }

    IEnumerator Countdown()
    {
        float remainingTime = countdownTime;

        while (remainingTime > 0)
        {
            remainingTime -= Time.deltaTime;

            // Optional: update countdown text
            // if (countdownUI != null) { ... }

            yield return null;
        }

        if (countdownUI != null)
            countdownUI.SetActive(false);
    }

    // Call this method to add points to the score
    public void AddScore(float delay)
    {
        Debug.Log("AddScoreFunctionBeingCalled");
        float points = 0;
        if (delay >= 170){ //Perfect
            points += 1000;
            pointMultiplier += 0.3f;
        }
        else if (delay >= 150 && delay < 170){ // Good
            points += 700;
            pointMultiplier += 0.1f;
        }
        else if (delay >= 120 && delay < 150){ // Mid
            points += 500;
            pointMultiplier = 1;
        }
        else {  //Low
            points += 300;
            pointMultiplier = 1;
        }
        points *= pointMultiplier;
        score += points;
        Debug.Log($"[LevelController] Score: {score}");
        UpdateScoreUI();
    }

    // Call this to reset the score (if needed)
    public void ResetScore()
    {
        score = 0;
        UpdateScoreUI();
    }

    // Updates the UI text (if assigned)
    public void UpdateScoreUI()
    {
        if (scoreText != null && scoreTextMultiplier != null)
        {
            scoreText.text = score.ToString();
            scoreTextMultiplier.text = "X" + pointMultiplier.ToString();
        }
    }
}
