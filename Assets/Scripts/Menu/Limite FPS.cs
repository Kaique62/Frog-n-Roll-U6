using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SQLite;
using System.IO;
//using Unity.VisualScripting.Dependencies.Sqlite;

public class FpsLimiter : MonoBehaviour
{
    [Header("FPS")]
    [SerializeField] private Toggle toggleMostrarFps;
    [SerializeField] private TMP_Text fpsText;
    [SerializeField] private Slider sliderFps;

    [Header("Volume da Música")]
    [SerializeField] private Slider sliderVolume;
    [SerializeField] private string nomeDoAudio = "Demo";  // Nome do áudio sem a extensão

    private SQLiteConnection db;
    private AudioSource audioSource;

    private void Start()
    {
        string path = Path.Combine(Application.persistentDataPath, "config.db");
        db = new SQLiteConnection(path);
        db.CreateTable<Configuracao>();

        // FPS
        sliderFps.minValue = 30;
        sliderFps.maxValue = 301;
        sliderFps.wholeNumbers = false;
        sliderFps.value = LerFpsSalvo();

        toggleMostrarFps.onValueChanged.AddListener(OnToggleFps);
        sliderFps.onValueChanged.AddListener(delegate { AtualizarFps(); });

        AtualizarFps();

        // Volume
        audioSource = GetComponent<AudioSource>();
        if (sliderVolume != null)
        {
            float volumeSalvo = LerVolumeSalvo();
            sliderVolume.value = volumeSalvo;

            if (audioSource != null)
            {
                // Carregar o áudio da pasta Resources
                AudioClip clip = Resources.Load<AudioClip>("Musics/Demo");  // Nome correto do arquivo sem a extensão
                if (clip != null)
                {
                    audioSource.clip = clip;
                    audioSource.volume = volumeSalvo;
                    audioSource.playOnAwake = false;
                    audioSource.Stop();
                }
                else
                {
                    Debug.LogError("Áudio não encontrado em Resources/Audio!");
                }
            }

            sliderVolume.onValueChanged.AddListener(delegate { AtualizarVolume(); });
        }
    }

    private void OnToggleFps(bool isOn)
    {
        fpsText.gameObject.SetActive(isOn);
        sliderFps.gameObject.SetActive(isOn);
        AtualizarFps();
    }

    private void AtualizarFps()
    {
        float valorSlider = sliderFps.value;

        if (valorSlider >= 301f)
        {
            Application.targetFrameRate = -1;
            fpsText.text = "FPS: Ilimitado";
        }
        else
        {
            int fps = Mathf.RoundToInt(valorSlider);
            Application.targetFrameRate = fps;
            fpsText.text = "FPS: " + fps;
        }

        SalvarFps(valorSlider);
    }

    private void AtualizarVolume()
    {
        float volume = sliderVolume.value;
        SalvarVolume(volume);

        if (audioSource != null)
        {
            audioSource.volume = volume;

            // Só toca se ainda não estiver tocando
            if (!audioSource.isPlaying && audioSource.clip != null)
            {
                audioSource.Play();
            }
        }
    }

    private void SalvarVolume(float volume)
    {
        SalvarConfig("volume_" + nomeDoAudio, volume.ToString());
    }

    private float LerVolumeSalvo()
    {
        string valor = LerConfig("volume_" + nomeDoAudio);
        return float.TryParse(valor, out float resultado) ? resultado : 1f;
    }

    private void SalvarFps(float fps)
    {
        SalvarConfig("fps", fps.ToString());
    }

    private float LerFpsSalvo()
    {
        string valor = LerConfig("fps");
        return float.TryParse(valor, out float resultado) ? resultado : 60f;
    }

    private void SalvarConfig(string chave, string valor)
    {
        var config = db.Find<Configuracao>(chave);
        if (config != null)
        {
            config.Valor = valor;
            db.Update(config);
        }
        else
        {
            db.Insert(new Configuracao { Chave = chave, Valor = valor });
        }
    }

    private string LerConfig(string chave)
    {
        var config = db.Find<Configuracao>(chave);
        return config?.Valor;
    }

    [Table("configuracoes")]
    public class Configuracao
    {
        [PrimaryKey]
        public string Chave { get; set; }
        public string Valor { get; set; }
    }
    public void SalvarTodasAsConfigs()
    {
        SalvarVolume(sliderVolume.value);
        SalvarFps(sliderFps.value);
    }
}
