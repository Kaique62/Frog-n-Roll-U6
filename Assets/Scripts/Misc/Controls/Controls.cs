using UnityEngine;
using UnityEngine.UI;

public static class Controls
{
    //Binds do Menu
    public static KeyCode Confirm = KeyCode.Return;
    public static KeyCode Cancel = KeyCode.Escape;

    //Binds do Jogador
    public static KeyCode Pulo = KeyCode.Space;
    public static KeyCode Esquerda = KeyCode.A;
    public static KeyCode Direita = KeyCode.D;
    public static KeyCode Agarrar = KeyCode.E;
    public static KeyCode Soco = KeyCode.J;
    public static KeyCode Gancho = KeyCode.I;
    public static KeyCode Chute = KeyCode.L;
    public static KeyCode Rolar = KeyCode.K;
    public static KeyCode Agachar = KeyCode.S;
    

    //Teste para interface mobile
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
