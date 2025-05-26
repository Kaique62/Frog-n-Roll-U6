using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FpsLimiter : MonoBehaviour
{
    [Header("Volume")]
    public Slider sliderVolumeMusica;
    public AudioSource musicaDeFundo;

    [Header("Botões de FPS")]
    public Button btnFps24;
    public Button btnFps30;
    public Button btnFps60;
    public Button btnFps90;
    public Button btnFps120;
    public Button btnFps999;

    private readonly int[] fpsOptions = { 24, 30, 60, 90, 120, -1 }; // -1 = ilimitado
    private int fpsIndex = 2; // padrão 60 FPS

    [Header("Outros")]
    public Toggle toggleMostrarFps;
    public Toggle toggleVSync;
    public GameObject painelFPS;
    [SerializeField] private GameObject canvasEdicaoControles;

    void Start()
    {
        // Volume da música
        float volume = LerVolumeSalvo("MusicVolume");
        sliderVolumeMusica.value = volume;
        if (musicaDeFundo != null)
            musicaDeFundo.volume = volume;

        sliderVolumeMusica.onValueChanged.AddListener((novoVolume) => {
            SalvarVolume("MusicVolume", novoVolume);
            if (musicaDeFundo != null)
                musicaDeFundo.volume = novoVolume;
        });

        // FPS salvo
        int indexSalvo = LerFpsSalvo();
        AplicarFps(indexSalvo);
        fpsIndex = indexSalvo;

        // Mostrar FPS
        bool mostrarFps = ConfiguracoesManager.Ler("mostrarFps") == "True";
        toggleMostrarFps.isOn = mostrarFps;
        painelFPS.SetActive(mostrarFps);
        toggleMostrarFps.onValueChanged.AddListener(OnToggleFps);

        // VSync
        bool vsyncAtivado = ConfiguracoesManager.Ler("vsync") == "True";
        toggleVSync.isOn = vsyncAtivado;
        QualitySettings.vSyncCount = vsyncAtivado ? 1 : 0;
        toggleVSync.onValueChanged.AddListener(OnToggleVSync);

        // Botões de FPS
        btnFps24.onClick.AddListener(() => SelecionarFps(0));
        btnFps30.onClick.AddListener(() => SelecionarFps(1));
        btnFps60.onClick.AddListener(() => SelecionarFps(2));
        btnFps90.onClick.AddListener(() => SelecionarFps(3));
        btnFps120.onClick.AddListener(() => SelecionarFps(4));
        btnFps999.onClick.AddListener(() => SelecionarFps(5));
    }

    private void SelecionarFps(int index)
    {
        fpsIndex = index;
        SalvarFps(index);
    }

    private void AplicarFps(int index)
    {
        int fps = fpsOptions[index];
        Application.targetFrameRate = fps;
        Debug.Log($"FPS configurado para: {(fps == -1 ? "Ilimitado" : fps.ToString())}");
    }

    public void OnToggleFps(bool isOn)
    {
        painelFPS.SetActive(isOn);
        ConfiguracoesManager.Salvar("mostrarFps", isOn.ToString());
    }

    public void OnToggleVSync(bool isOn)
    {
        QualitySettings.vSyncCount = isOn ? 1 : 0;
        ConfiguracoesManager.Salvar("vsync", isOn.ToString());
    }

    public void SalvarVolume(string nomeDoAudio, float volume)
    {
        ConfiguracoesManager.Salvar("volume_" + nomeDoAudio, volume.ToString());
    }

    private float LerVolumeSalvo(string nomeDoAudio)
    {
        string valor = ConfiguracoesManager.Ler("volume_" + nomeDoAudio);
        if (float.TryParse(valor, out float volume))
            return volume;

        return 1f; // volume padrão: 100%
    }

    private void SalvarFps(int index)
    {
        ConfiguracoesManager.Salvar("fpsIndex", index.ToString());
        AplicarFps(index);
    }

    private int LerFpsSalvo()
    {
        string valor = ConfiguracoesManager.Ler("fpsIndex");
        if (int.TryParse(valor, out int index) && index >= 0 && index < fpsOptions.Length)
            return index;

        return 2; // padrão: 60 FPS
    }

    public void SalvarTodasConfiguracoes()
    {
        // Salvar volume da música
        ConfiguracoesManager.Salvar("volume_MusicVolume", sliderVolumeMusica.value.ToString());

        // Salvar FPS selecionado
        ConfiguracoesManager.Salvar("fpsIndex", fpsIndex.ToString());

        // Salvar toggle mostrar FPS
        ConfiguracoesManager.Salvar("mostrarFps", toggleMostrarFps.isOn.ToString());

        // Salvar toggle VSync
        ConfiguracoesManager.Salvar("vsync", toggleVSync.isOn.ToString());

        // Força salvar no arquivo JSON (pode chamar aqui para garantir)
        // Mas como no seu ConfiguracoesManager a função Salvar já chama SalvarEmArquivo,
        // isso não é obrigatório, só se quiser garantir:
        // ConfiguracoesManager.ForceSave(); // (cria essa se quiser)

        Debug.Log("Todas as configurações foram salvas!");
    }


    public void AbrirConfiguracoesDeControles()
    {
        if (canvasEdicaoControles != null)
        {
            canvasEdicaoControles.SetActive(true);
            Debug.Log("Editor de controles ativado.");
        }
        else
        {
            Debug.LogWarning("Canvas de edição de controles não atribuído.");
        }
    }
}