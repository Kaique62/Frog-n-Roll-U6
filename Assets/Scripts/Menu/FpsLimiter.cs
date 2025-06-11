using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FpsLimiter : MonoBehaviour
{
    // =================================================================
    // MENU AUDIO
    // =================================================================
    [Header("Menu Audio")]
    public AudioClip menuMusicClip;
    private AudioSource musicPlayer;

    [Header("Volume Controls")]
    public Slider globalVolumeSlider;
    public TextMeshProUGUI globalVolumeText;

    public Slider musicVolumeSlider;
    public TextMeshProUGUI musicVolumeText;

    public Slider sfxVolumeSlider;
    public TextMeshProUGUI sfxVolumeText;

    // --- CHANGE 1: This field now accepts the GameObject of your UIAudioManager ---
    [Header("External Audio References")]
    public GameObject sfxManagerObject; // Drag your "UIAudioManager" GameObject here
    private AudioSource sfxPlayer;

    // =================================================================
    // GRAPHICS AND FPS SETTINGS
    // =================================================================
    [Header("FPS Buttons")]
    public Button btnFps24;
    public Button btnFps30;
    public Button btnFps60;
    public Button btnFps90;
    public Button btnFps120;
    public Button btnFps999;

    private readonly int[] fpsOptions = { 24, 30, 60, 90, 120, -1 };
    private int fpsIndex = 2;

    [Header("Other UI Controls")]
    public Toggle showFpsToggle;
    public Toggle vsyncToggle;
    public GameObject fpsPanel;
    [SerializeField] private GameObject controlEditCanvas;

    // =================================================================
    // INITIALIZATION
    // =================================================================
    void Start()
    {
        SetupAudioSources();
        LoadAllSettings();
        SetupButtonListeners();
    }

    private void SetupAudioSources()
    {
        // Ensure this object has a music audio source
        if (GetComponent<AudioSource>() == null)
        {
            musicPlayer = gameObject.AddComponent<AudioSource>();
        }
        else
        {
            musicPlayer = GetComponent<AudioSource>();
        }
        musicPlayer.playOnAwake = false;

        // --- CHANGE 2: Gets the AudioSource from the dragged SFX GameObject ---
        if (sfxManagerObject != null)
        {
            sfxPlayer = sfxManagerObject.GetComponent<AudioSource>();
            if (sfxPlayer == null)
            {
                Debug.LogError("The SFX Manager GameObject has no AudioSource component!", sfxManagerObject);
            }
        }
    }

    private void LoadAllSettings()
    {
        // AUDIO
        float globalVolume = LoadSavedVolume("GlobalVolume");
        float musicVolume = LoadSavedVolume("MusicVolume");
        float sfxVolume = LoadSavedVolume("SfxVolume");

        AudioListener.volume = globalVolume;

        if (globalVolumeSlider != null) globalVolumeSlider.value = globalVolume;
        if (musicVolumeSlider != null) musicVolumeSlider.value = musicVolume;
        if (sfxVolumeSlider != null) sfxVolumeSlider.value = sfxVolume;

        if (musicPlayer != null) musicPlayer.volume = musicVolume;
        if (sfxPlayer != null) sfxPlayer.volume = sfxVolume;

        UpdateVolumeText(globalVolumeText, globalVolume);
        UpdateVolumeText(musicVolumeText, musicVolume);
        UpdateVolumeText(sfxVolumeText, sfxVolume);

        if (menuMusicClip != null && musicPlayer != null)
        {
            musicPlayer.clip = menuMusicClip;
            musicPlayer.loop = true;
            musicPlayer.Play();
        }

        // GRAPHICS
        int savedIndex = LoadSavedFps();
        ApplyFps(savedIndex);
        fpsIndex = savedIndex;

        bool showFps = ConfigManager.Read("mostrarFps") == "True";
        showFpsToggle.isOn = showFps;
        if (fpsPanel != null) fpsPanel.SetActive(showFps);

        bool vsyncEnabled = ConfigManager.Read("vsync") == "True";
        vsyncToggle.isOn = vsyncEnabled;
        QualitySettings.vSyncCount = vsyncEnabled ? 1 : 0;
    }

    private void SetupButtonListeners()
    {
        // GLOBAL Volume
        if (globalVolumeSlider != null)
        {
            globalVolumeSlider.onValueChanged.AddListener((newVolume) =>
            {
                AudioListener.volume = newVolume;
                UpdateVolumeText(globalVolumeText, newVolume);
                SaveVolume("GlobalVolume", newVolume);
            });
        }

        // MUSIC Volume
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.AddListener((newVolume) =>
            {
                if (musicPlayer != null) musicPlayer.volume = newVolume;
                UpdateVolumeText(musicVolumeText, newVolume);
                SaveVolume("MusicVolume", newVolume);
            });
        }

        // SFX Volume
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.AddListener((newVolume) =>
            {
                if (sfxPlayer != null) sfxPlayer.volume = newVolume;
                UpdateVolumeText(sfxVolumeText, newVolume);
                SaveVolume("SfxVolume", newVolume);
            });
        }

        // FPS Buttons and Toggles
        btnFps24.onClick.AddListener(() => SelectFps(0));
        btnFps30.onClick.AddListener(() => SelectFps(1));
        btnFps60.onClick.AddListener(() => SelectFps(2));
        btnFps90.onClick.AddListener(() => SelectFps(3));
        btnFps120.onClick.AddListener(() => SelectFps(4));
        btnFps999.onClick.AddListener(() => SelectFps(5));

        showFpsToggle.onValueChanged.AddListener(OnToggleShowFps);
        vsyncToggle.onValueChanged.AddListener(OnToggleVSync);
    }

    private void UpdateVolumeText(TextMeshProUGUI text, float value)
    {
        if (text != null)
        {
            int percentage = Mathf.RoundToInt(value * 100);
            text.text = percentage + "%";
        }
    }

    // FPS & Settings Logic
    private void SelectFps(int index)
    {
        if (index < 0 || index >= fpsOptions.Length) return;
        fpsIndex = index;
        ApplyFps(index);
        SaveFps(index);
    }

    private void ApplyFps(int index)
    {
        Application.targetFrameRate = fpsOptions[index];
    }

    public void OnToggleShowFps(bool isOn)
    {
        if (fpsPanel != null) fpsPanel.SetActive(isOn);
        ConfigManager.Save("mostrarFps", isOn.ToString());
    }

    public void OnToggleVSync(bool isOn)
    {
        QualitySettings.vSyncCount = isOn ? 1 : 0;
        ConfigManager.Save("vsync", isOn.ToString());
    }

    public void SaveVolume(string name, float volume)
    {
        ConfigManager.Save("volume_" + name, volume.ToString());
    }

    private float LoadSavedVolume(string name)
    {
        string value = ConfigManager.Read("volume_" + name);
        if (float.TryParse(value, out float volume))
        {
            return volume;
        }
        return 1f;
    }

    private void SaveFps(int index)
    {
        ConfigManager.Save("fpsIndex", index.ToString());
    }

    private int LoadSavedFps()
    {
        string value = ConfigManager.Read("fpsIndex");
        if (int.TryParse(value, out int index) && index >= 0 && index < fpsOptions.Length)
        {
            return index;
        }
        return 2; // default to 60 FPS
    }

    public void SaveAllSettings()
    {
        SaveVolume("GlobalVolume", globalVolumeSlider.value);
        SaveVolume("MusicVolume", musicVolumeSlider.value);
        SaveVolume("SfxVolume", sfxVolumeSlider.value);
        SaveFps(fpsIndex);
        ConfigManager.Save("mostrarFps", showFpsToggle.isOn.ToString());
        ConfigManager.Save("vsync", vsyncToggle.isOn.ToString());
        Debug.Log("All settings have been saved!");
    }

    public void OpenControlSettings()
    {
        if (controlEditCanvas != null)
        {
            controlEditCanvas.SetActive(true);
        }
    }
}
