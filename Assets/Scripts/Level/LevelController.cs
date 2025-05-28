using TMPro;
using UnityEngine;

public class LevelController : MonoBehaviour
{
    [Header("Score System")]
    public float score = 0;
    public TMP_Text scoreText;
    public TMP_Text scoreTextMultiplier;
    public float pointMultiplier = 1;

    [Header("Music")]
    public AudioSource musicAudioSource;

    bool gameStarted = false;

    void Start()
    {
        UpdateScoreUI();
    }

    void Update()
    {
        if (Input.anyKey && !gameStarted)
        {
            StartLevel();
        }
    }

    void StartLevel()
    {
        gameStarted = true;

        if (musicAudioSource != null && !musicAudioSource.isPlaying)
        {
            float volumeSalvo = LerVolumeSalvo();
            musicAudioSource.volume = volumeSalvo;

            Debug.Log("audioStart com volume: " + volumeSalvo);
            musicAudioSource.Play();
        }
    }

    public void AddScore(float delay)
    {
        float points = 0;
        if (delay >= 170) { points += 1000; pointMultiplier += 0.3f; }
        else if (delay >= 150) { points += 700; pointMultiplier += 0.1f; }
        else if (delay >= 120) { points += 500; pointMultiplier = 1; }
        else { points += 300; pointMultiplier = 1; }

        points *= pointMultiplier;
        score += points;
        UpdateScoreUI();
    }

    public void ResetScore()
    {
        score = 0;
        UpdateScoreUI();
    }

    public void UpdateScoreUI()
    {
        if (scoreText != null && scoreTextMultiplier != null)
        {
            scoreText.text = score.ToString();
            scoreTextMultiplier.text = "X" + pointMultiplier.ToString();
        }
    }

    private float LerVolumeSalvo()
    {
        string valor = ConfiguracoesManager.Ler("volume_MusicVolume");
        if (float.TryParse(valor, out float volume))
            return Mathf.Clamp01(volume);
        return 1f;
    }
}
