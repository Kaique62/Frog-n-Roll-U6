// CURRENTLY NOT BEING USED
/// <summary> 
/// This code was planned for the menus, so you can control it with arrow keys or with a controller.
/// But the game currently does not support controller Joystick
/// </summary>

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class ArrowMenuController : MonoBehaviour
{
    public Button[] menuButtons;
    private int currentIndex = 0;

    void Start()
    {
        if (menuButtons.Length > 0)
        {
            HighlightButton(currentIndex);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentIndex = (currentIndex + 1) % menuButtons.Length;
            HighlightButton(currentIndex);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentIndex = (currentIndex - 1 + menuButtons.Length) % menuButtons.Length;
            HighlightButton(currentIndex);
        }
        else if (Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            menuButtons[currentIndex].onClick.Invoke();
        }
        else if (Input.GetKeyDown(KeyCode.Backspace)){
            SceneManager.UnloadSceneAsync(gameObject.scene);
        }
    }

    void HighlightButton(int index)
    {
        EventSystem.current.SetSelectedGameObject(menuButtons[index].gameObject);
    }
}
