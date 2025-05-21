using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SQLite;
using System.IO;

public class FpsLimiter : MonoBehaviour
{
    [Header("FPS")]
    [SerializeField] private Toggle toggleMostrarFps;
    [SerializeField] private TMP_Text fpsText;

    [Header("Volume da Música")]
    [SerializeField] private Slider sliderVolume;
    [SerializeField] private string nomeDoAudio = "Demo";  // Nome do áudio sem a extensão
    private readonly int[] fpsOptions = { 24, 30, 60, 90, 120, 999 };
    [Header("Botões de FPS")]
    [SerializeField] private Button[] botoesFps; // Ordem: 24, 30, 60, 90, 120, 999

    private SQLiteConnection db;
    private AudioSource audioSource;
    private int fpsIndex = 1; // padrão: 60 FPS
    public GameObject gameCt; // Arraste aqui o painel de edição de controles
    public GameObject mobileControls; // O GameObject que contém os botões móveis (MobileControls)

    private void Start()
    {
        string path = Path.Combine(Application.persistentDataPath, "config.db");
        db = new SQLiteConnection(path);

        db.CreateTable<Configuracao>();

        // Mostrar FPS
        string mostrar = LerConfig("mostrarFps");
        bool mostrarFps = mostrar == "True";
        toggleMostrarFps.isOn = mostrarFps;
        fpsText.gameObject.SetActive(mostrarFps);

        // FPS
        fpsIndex = LerFpsSalvo();
        AplicarFps(); // já seleciona o botão certo

        toggleMostrarFps.onValueChanged.AddListener(OnToggleFps);

        // Volume
        audioSource = GetComponent<AudioSource>();
        if (sliderVolume != null)
        {
            float volumeSalvo = LerVolumeSalvo();
            sliderVolume.value = volumeSalvo;

            if (audioSource != null)
            {
                AudioClip clip = Resources.Load<AudioClip>("Musics/Demo");
                if (clip != null)
                {
                    audioSource.clip = clip;
                    audioSource.volume = volumeSalvo;
                    audioSource.playOnAwake = false;
                    audioSource.Stop();
                }
                else
                {
                    Debug.LogError("Áudio não encontrado em Resources/Musics!");
                }
            }

            sliderVolume.onValueChanged.AddListener(delegate { AtualizarVolume(); });
        }
    }

    private void OnToggleFps(bool isOn)
    {
        fpsText.gameObject.SetActive(isOn);
        SalvarConfig("mostrarFps", isOn.ToString());
        AplicarFps(); // já existe e está correta
    }

    private void AplicarFps()
    {
        if (fpsIndex < 0 || fpsIndex >= fpsOptions.Length)
        {
            Debug.LogWarning($"fpsIndex {fpsIndex} está fora do intervalo! Usando padrão 60 FPS.");
            fpsIndex = 2;
        }

        int fps = fpsOptions[fpsIndex];
        Application.targetFrameRate = fps;
        AtualizarFpsTexto();
        SalvarFps(fpsIndex);
        SelecionarBotaoFpsAtual(); // Atualiza a seleção visual
    }

    private void SelecionarBotaoFpsAtual()
    {
        if (botoesFps != null && fpsIndex >= 0 && fpsIndex < botoesFps.Length)
        {
            botoesFps[fpsIndex].Select();
        }
    }

    private void AtualizarFpsTexto()
    {
        fpsText.text = "FPS: " + fpsOptions[fpsIndex];
    }

    private void AtualizarVolume()
    {
        float volume = sliderVolume.value;
        SalvarVolume(volume);

        if (audioSource != null)
        {
            audioSource.volume = volume;
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

    private void SalvarFps(int index)
    {
        SalvarConfig("fpsIndex", index.ToString());
    }

    private int LerFpsSalvo()
    {
        string valor = LerConfig("fpsIndex");
        return int.TryParse(valor, out int index) ? Mathf.Clamp(index, 0, fpsOptions.Length - 1) : 2; // 60 FPS padrão
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

    public void AbrirConfiguracaoControles()
    {
        if (gameCt != null) gameCt.SetActive(true);
        if (mobileControls != null) mobileControls.SetActive(true);

        // Permitir edição dos botões filhos (prefabs) dentro de MobileControls
        foreach (Transform child in mobileControls.transform)
        {
            GameObject botao = child.gameObject;

            // Garante que o botão tenha um componente que permita edição (componente customizado)
            if (!botao.TryGetComponent<BotaoEditavel>(out _))
            {
                botao.AddComponent<BotaoEditavel>(); // Permite mover e redimensionar
            }
        }

        Debug.Log("Modo de edição de controles ativado!");
    }


    // Botões para selecionar FPS
    public void SelecionarFps24()
    {
        fpsIndex = 0;
        AplicarFps();
    }

    public void SelecionarFps30()
    {
        fpsIndex = 1;
        AplicarFps();
    }

    public void SelecionarFps60()
    {
        fpsIndex = 2;
        AplicarFps();
    }

    public void SelecionarFps90()
    {
        fpsIndex = 3;
        AplicarFps();
    }

    public void SelecionarFps120()
    {
        fpsIndex = 4;
        AplicarFps();
    }

    public void SelecionarFps999()
    {
        fpsIndex = 5;
        AplicarFps();
    }
}
