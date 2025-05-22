using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelController : MonoBehaviour
{
    [Header("Score System")]
    public float score = 0;                          // Current score
    public TMP_Text scoreText;                         // Optional UI Text to display the score
    public TMP_Text scoreTextMultiplier;                         // Optional UI Text to display the score
    
    public float pointMultiplier = 1;

    [Header("Music")]
    public AudioSource musicAudioSource;
    static bool GameStarted = false;
    void Start()
    {
        UpdateScoreUI();
    }

    void Update()
    {
        if (Input.anyKey && !GameStarted)
        {
            StartLevel();
        }
    }

    void StartLevel()
    {
        GameStarted = true;
        if (musicAudioSource != null && !musicAudioSource.isPlaying)
        {
            Debug.Log("audioStart");
            musicAudioSource.Play();
        }
    }

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
