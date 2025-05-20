using UnityEngine;
using UnityEngine.UI;

public static class Controls
{
    // Menu Binds
    public static KeyCode Confirm = KeyCode.Return;
    public static KeyCode Cancel = KeyCode.Escape;

    // Player Binds
    public static KeyCode Jump = KeyCode.W;
    public static KeyCode Left = KeyCode.A;
    public static KeyCode Right = KeyCode.D;
    public static KeyCode Grab = KeyCode.W;
    public static KeyCode Punch = KeyCode.J;
    public static KeyCode Uppercut = KeyCode.I;
    public static KeyCode Kick = KeyCode.L;
    public static KeyCode Roll = KeyCode.K;
    public static KeyCode Down = KeyCode.S;
    
    // Test for mobile interface
    public static Button ConfirmButton;

    private static bool confirmPressed;

    public static bool ConfirmButtonPressed
    {
        get
        {
            if (confirmPressed)
            {
                confirmPressed = false;
                return true;
            }
            return false;
        }
    }

    public static void InitializeUIButtons()
    {
        if (ConfirmButton != null)
        {
            ConfirmButton.onClick.AddListener(() => confirmPressed = true);
        }
    }
}
