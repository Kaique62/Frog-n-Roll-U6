using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SQLite;
using System.IO;

public class FpsLimiter : MonoBehaviour
{
    public GameObject gameCt;
    public GameObject mobileControls;

    [Header("VSync")]
    [SerializeField] private Toggle toggleVSync;

    [Header("FPS")]
    [SerializeField] private Toggle toggleMostrarFps;
    [SerializeField] private TMP_Text fpsText;

    [Header("Volume da M�sica")]
    [SerializeField] private Slider sliderVolume;
    [SerializeField] private TextMeshProUGUI volumePercentText;
    [SerializeField] private string nomeDoAudio = "Demo";

    [Header("Bot�es de FPS")]
    [SerializeField] private Button[] botoesFps;

    private readonly int[] fpsOptions = { 24, 30, 60, 90, 120, 999 };

    private SQLiteConnection db;
    private AudioSource audioSource;
    private int fpsIndex = 2;

    private void Start()
    {
        string path = Path.Combine(Application.persistentDataPath, "config.db");
        db = new SQLiteConnection(path);
        db.CreateTable<Configuracao>();

        // Mostrar FPS
        bool mostrarFps = LerConfig("mostrarFps") == "True";
        toggleMostrarFps.isOn = mostrarFps;
        fpsText.gameObject.SetActive(mostrarFps);
        toggleMostrarFps.onValueChanged.AddListener(OnToggleFps);

        // FPS
        fpsIndex = LerFpsSalvo();
        AplicarFps();

        // Volume
        audioSource = GetComponent<AudioSource>();
        if (sliderVolume != null)
        {
            float volumeSalvo = LerVolumeSalvo();
            sliderVolume.value = volumeSalvo;
            if (volumePercentText != null)
            {
                int percentual = Mathf.RoundToInt(volumeSalvo * 100);
                volumePercentText.text = percentual + "%";
            }

            if (audioSource != null)
            {
                AudioClip clip = Resources.Load<AudioClip>("Musics/" + nomeDoAudio);
                if (clip != null)
                {
                    audioSource.clip = clip;
                    audioSource.volume = volumeSalvo;
                    audioSource.playOnAwake = false;
                    audioSource.Stop();
                }
                else
                {
                    Debug.LogError("�udio n�o encontrado em Resources/Musics/");
                }
            }

            sliderVolume.onValueChanged.AddListener(delegate { AtualizarVolume(); });
        }

                // VSync
        bool vsyncAtivado = LerConfig("vsync") == "True";
        toggleVSync.isOn = vsyncAtivado;
        QualitySettings.vSyncCount = vsyncAtivado ? 1 : 0;
        toggleVSync.onValueChanged.AddListener(OnToggleVSync);


    }

    private void OnToggleFps(bool isOn)
    {
        fpsText.gameObject.SetActive(isOn);
        SalvarConfig("mostrarFps", isOn.ToString());
        AplicarFps();
    }

    private void OnToggleVSync(bool isOn)
    {
        QualitySettings.vSyncCount = isOn ? 1 : 0;
        SalvarConfig("vsync", isOn.ToString());
    }

    private void AplicarFps()
    {
        if (fpsIndex < 0 || fpsIndex >= fpsOptions.Length)
        {
            fpsIndex = 2;
            Debug.LogWarning("�ndice de FPS inv�lido. Usando padr�o: 60");
        }

        Application.targetFrameRate = fpsOptions[fpsIndex];
        AtualizarFpsTexto();
        SalvarFps(fpsIndex);
        SelecionarBotaoFpsAtual();
    }

    private void AtualizarFpsTexto()
    {
        fpsText.text = "FPS: " + fpsOptions[fpsIndex];
    }

    private void SelecionarBotaoFpsAtual()
    {
        if (botoesFps != null && fpsIndex >= 0 && fpsIndex < botoesFps.Length)
        {
            botoesFps[fpsIndex].Select();
        }
    }

    private void AtualizarVolume()
    {
        float volume = Mathf.Max(sliderVolume.value, 0.3f); // mínimo de 30%
        SalvarVolume(volume);

        if (audioSource != null)
        {
            audioSource.volume = volume;
            if (!audioSource.isPlaying && audioSource.clip != null)
            {
                audioSource.Play();
            }
        }

        // Atualiza texto de porcentagem
        if (volumePercentText != null)
        {
            int percentual = Mathf.RoundToInt(volume * 100);
            volumePercentText.text = percentual + "%";
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

    private void SalvarFps(int index)
    {
        SalvarConfig("fpsIndex", index.ToString());
    }

    private int LerFpsSalvo()
    {
        string valor = LerConfig("fpsIndex");
        return int.TryParse(valor, out int index) ? Mathf.Clamp(index, 0, fpsOptions.Length - 1) : 2;
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

    public void AbrirConfiguracaoControles()
    {
        if (gameCt != null) gameCt.SetActive(true);
        if (mobileControls != null) mobileControls.SetActive(true);

        Debug.Log("Modo de edi��o de controles ativado!");
    }

    [Table("configuracao")]
    public class Configuracao
    {
        [PrimaryKey]
        public string Chave { get; set; }
        public string Valor { get; set; }
    }

    public void SalvarTodasAsConfigs()
    {
        SalvarVolume(sliderVolume.value);
        SalvarFps(fpsIndex);
    }

    // M�todos para bot�es de FPS
    public void SelecionarFps24() { fpsIndex = 0; AplicarFps(); }
    public void SelecionarFps30() { fpsIndex = 1; AplicarFps(); }
    public void SelecionarFps60() { fpsIndex = 2; AplicarFps(); }
    public void SelecionarFps90() { fpsIndex = 3; AplicarFps(); }
    public void SelecionarFps120() { fpsIndex = 4; AplicarFps(); }
    public void SelecionarFps999() { fpsIndex = 5; AplicarFps(); }
}
