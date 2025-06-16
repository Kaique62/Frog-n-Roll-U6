using UnityEngine;
using UnityEngine.UI;

// Garante que este script esteja sempre em um objeto com um Slider
[RequireComponent(typeof(Slider))]
public class SfxVolumeSlider : MonoBehaviour
{
    private Slider sfxSlider;

    void Start()
    {
        // Pega o componente Slider deste objeto
        sfxSlider = GetComponent<Slider>();

        // Garante que a instância do UIAudioManager exista antes de continuar
        if (UIAudioManager.instance != null)
        {
            // Define o valor inicial do slider para ser igual ao volume atual
            sfxSlider.value = UIAudioManager.instance.CurrentVolume;

            // Adiciona um "escutador" ao slider via código.
            // Agora, toda vez que o valor do slider mudar, o método 'OnSliderValueChanged' será chamado.
            sfxSlider.onValueChanged.AddListener(OnSliderValueChanged);
        }
        else
        {
            Debug.LogError("SfxVolumeSlider: UIAudioManager.instance não encontrado!");
        }
    }

    // Este método é chamado pelo "escutador" que adicionamos acima
    private void OnSliderValueChanged(float value)
    {
        // Chama o novo método do UIAudioManager para definir e salvar o volume
        if (UIAudioManager.instance != null)
        {
            UIAudioManager.instance.SetSfxVolume(value);
        }
    }

    // Boa prática: remove o "escutador" quando o objeto for destruído
    void OnDestroy()
    {
        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveListener(OnSliderValueChanged);
        }
    }
}