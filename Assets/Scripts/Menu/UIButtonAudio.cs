using UnityEngine;
using UnityEngine.EventSystems; // Necessário para as interfaces de eventos
using UnityEngine.UI;

// Este componente, quando adicionado a um botão, irá automaticamente
// tocar os sons do UIAudioManager singleton.
[RequireComponent(typeof(Button))] // Garante que este script esteja sempre em um objeto com um botão
public class UIButtonAudio : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    // Este método é chamado quando o mouse entra na área do botão
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Procura a instância singleton do UIAudioManager e toca o som de hover
        if (UIAudioManager.instance != null)
        {
            UIAudioManager.instance.PlayHoverSound();
        }
    }

    // Este método é chamado quando o botão é clicado
    public void OnPointerClick(PointerEventData eventData)
    {
        // Procura a instância singleton do UIAudioManager e toca o som de clique
        if (UIAudioManager.instance != null)
        {
            UIAudioManager.instance.PlayClickSound();
        }
    }
}