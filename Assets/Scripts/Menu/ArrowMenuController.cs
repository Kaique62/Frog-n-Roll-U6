using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            menuButtons[currentIndex].onClick.Invoke();
        }
    }

    void HighlightButton(int index)
    {
        EventSystem.current.SetSelectedGameObject(menuButtons[index].gameObject);
    }
}
