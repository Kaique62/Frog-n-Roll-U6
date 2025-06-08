using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FpsLimiter : MonoBehaviour
{
    // =================================================================
    // ÁUDIO DO MENU
    // =================================================================
    [Header("Áudio do Menu")]
    public AudioClip musicaDoMenuClip;
    private AudioSource tocadorDeMusica;

    [Header("Controles de Volume")]
    public Slider sliderVolumeGlobal;
    public TextMeshProUGUI textoPorcentagemGlobal;
    
    public Slider sliderVolumeMusica;
    public TextMeshProUGUI textoPorcentagemMusica;
    
    public Slider sliderVolumeSfx;
    public TextMeshProUGUI textoPorcentagemSfx;
    
    // --- MUDANÇA 1: O campo agora aceita o GameObject do seu UIAudioManager ---
    [Header("Referências de Áudio Externo")]
    public GameObject sfxManagerObject; // Arraste seu GameObject "UIAudioManager" aqui
    private AudioSource tocadorDeSfx;

    // =================================================================
    // CONFIGURAÇÕES GRÁFICAS E DE FPS
    // =================================================================
    [Header("Botões de FPS")]
    public Button btnFps24;
    public Button btnFps30;
    public Button btnFps60;
    public Button btnFps90;
    public Button btnFps120;
    public Button btnFps999;

    private readonly int[] fpsOptions = { 24, 30, 60, 90, 120, -1 };
    private int fpsIndex = 2;

    [Header("Outros Controles de UI")]
    public Toggle toggleMostrarFps;
    public Toggle toggleVSync;
    public GameObject painelFPS;
    [SerializeField] private GameObject canvasEdicaoControles;

    // =================================================================
    // INICIALIZAÇÃO
    // =================================================================
    void Start()
    {
        SetupTocadoresDeAudio();
        CarregarTodasConfiguracoes();
        SetupListenersDosBotoes();
    }
    
    private void SetupTocadoresDeAudio()
    {
        // Garante que o tocador de música deste objeto exista
        if (GetComponent<AudioSource>() == null)
        {
            tocadorDeMusica = gameObject.AddComponent<AudioSource>();
        }
        else
        {
            tocadorDeMusica = GetComponent<AudioSource>();
        }
        tocadorDeMusica.playOnAwake = false;

        // --- MUDANÇA 2: Pega o componente AudioSource do GameObject que você arrastou ---
        if (sfxManagerObject != null)
        {
            tocadorDeSfx = sfxManagerObject.GetComponent<AudioSource>();
            if (tocadorDeSfx == null)
            {
                Debug.LogError("O GameObject do SFX Manager não possui um componente AudioSource!", sfxManagerObject);
            }
        }
    }


    private void CarregarTodasConfiguracoes()
    {
        // ÁUDIO
        float volumeGlobal = LerVolumeSalvo("GlobalVolume");
        float volumeMusica = LerVolumeSalvo("MusicVolume");
        float volumeSfx = LerVolumeSalvo("SfxVolume");

        AudioListener.volume = volumeGlobal;

        if (sliderVolumeGlobal != null) sliderVolumeGlobal.value = volumeGlobal;
        if (sliderVolumeMusica != null) sliderVolumeMusica.value = volumeMusica;
        if (sliderVolumeSfx != null) sliderVolumeSfx.value = volumeSfx;

        if (tocadorDeMusica != null) tocadorDeMusica.volume = volumeMusica;
        if (tocadorDeSfx != null) tocadorDeSfx.volume = volumeSfx;
        
        AtualizarTextoDePorcentagem(textoPorcentagemGlobal, volumeGlobal);
        AtualizarTextoDePorcentagem(textoPorcentagemMusica, volumeMusica);
        AtualizarTextoDePorcentagem(textoPorcentagemSfx, volumeSfx);

        if (musicaDoMenuClip != null && tocadorDeMusica != null)
        {
            tocadorDeMusica.clip = musicaDoMenuClip;
            tocadorDeMusica.loop = true;
            tocadorDeMusica.Play();
        }

        // GRÁFICOS
        int indexSalvo = LerFpsSalvo();
        AplicarFps(indexSalvo);
        fpsIndex = indexSalvo;
        bool mostrarFps = ConfiguracoesManager.Ler("mostrarFps") == "True";
        toggleMostrarFps.isOn = mostrarFps;
        if (painelFPS != null) painelFPS.SetActive(mostrarFps);
        bool vsyncAtivado = ConfiguracoesManager.Ler("vsync") == "True";
        toggleVSync.isOn = vsyncAtivado;
        QualitySettings.vSyncCount = vsyncAtivado ? 1 : 0;
    }
    
    private void SetupListenersDosBotoes()
    {
        // Listener do Volume GLOBAL
        if (sliderVolumeGlobal != null)
        {
            sliderVolumeGlobal.onValueChanged.AddListener((novoVolume) =>
            {
                AudioListener.volume = novoVolume;
                AtualizarTextoDePorcentagem(textoPorcentagemGlobal, novoVolume);
                SalvarVolume("GlobalVolume", novoVolume);
            });
        }
        // Listener do Volume da MÚSICA
        if (sliderVolumeMusica != null)
        {
            sliderVolumeMusica.onValueChanged.AddListener((novoVolume) =>
            {
                if (tocadorDeMusica != null) tocadorDeMusica.volume = novoVolume;
                AtualizarTextoDePorcentagem(textoPorcentagemMusica, novoVolume);
                SalvarVolume("MusicVolume", novoVolume);
            });
        }
        // Listener do Volume dos EFEITOS (SFX)
        if (sliderVolumeSfx != null)
        {
            sliderVolumeSfx.onValueChanged.AddListener((novoVolume) =>
            {
                if (tocadorDeSfx != null) tocadorDeSfx.volume = novoVolume;
                AtualizarTextoDePorcentagem(textoPorcentagemSfx, novoVolume);
                SalvarVolume("SfxVolume", novoVolume);
            });
        }

        // Listeners de FPS e Toggles
        btnFps24.onClick.AddListener(() => SelecionarFps(0));
        btnFps30.onClick.AddListener(() => SelecionarFps(1));
        btnFps60.onClick.AddListener(() => SelecionarFps(2));
        btnFps90.onClick.AddListener(() => SelecionarFps(3));
        btnFps120.onClick.AddListener(() => SelecionarFps(4));
        btnFps999.onClick.AddListener(() => SelecionarFps(5));
        toggleMostrarFps.onValueChanged.AddListener(OnToggleFps);
        toggleVSync.onValueChanged.AddListener(OnToggleVSync);
    }

    private void AtualizarTextoDePorcentagem(TextMeshProUGUI texto, float valor)
    {
        if (texto != null)
        {
            int porcentagem = Mathf.RoundToInt(valor * 100);
            texto.text = porcentagem + "%";
        }
    }
    
    // O resto do script permanece funcionalmente igual
    private void SelecionarFps(int index) { if (index < 0 || index >= fpsOptions.Length) return; fpsIndex = index; AplicarFps(index); SalvarFps(index); }
    private void AplicarFps(int index) { Application.targetFrameRate = fpsOptions[index]; }
    public void OnToggleFps(bool isOn) { if (painelFPS != null) painelFPS.SetActive(isOn); ConfiguracoesManager.Salvar("mostrarFps", isOn.ToString()); }
    public void OnToggleVSync(bool isOn) { QualitySettings.vSyncCount = isOn ? 1 : 0; ConfiguracoesManager.Salvar("vsync", isOn.ToString()); }
    public void SalvarVolume(string nomeDoAudio, float volume) { ConfiguracoesManager.Salvar("volume_" + nomeDoAudio, volume.ToString()); }
    private float LerVolumeSalvo(string nomeDoAudio) { string valor = ConfiguracoesManager.Ler("volume_" + nomeDoAudio); if (float.TryParse(valor, out float volume)) { return volume; } return 1f; }
    private void SalvarFps(int index) { ConfiguracoesManager.Salvar("fpsIndex", index.ToString()); }
    private int LerFpsSalvo() { string valor = ConfiguracoesManager.Ler("fpsIndex"); if (int.TryParse(valor, out int index) && index >= 0 && index < fpsOptions.Length) { return index; } return 2; }
    public void SalvarTodasConfiguracoes() { SalvarVolume("GlobalVolume", sliderVolumeGlobal.value); SalvarVolume("MusicVolume", sliderVolumeMusica.value); SalvarVolume("SfxVolume", sliderVolumeSfx.value); SalvarFps(fpsIndex); ConfiguracoesManager.Salvar("mostrarFps", toggleMostrarFps.isOn.ToString()); ConfiguracoesManager.Salvar("vsync", toggleVSync.isOn.ToString()); Debug.Log("Todas as configurações foram salvas!"); }
    public void AbrirConfiguracoesDeControles() { if (canvasEdicaoControles != null) { canvasEdicaoControles.SetActive(true); } }
}
