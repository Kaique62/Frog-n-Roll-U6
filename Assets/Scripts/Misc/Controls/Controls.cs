using System;
using System.Collections.Generic;
using UnityEngine;

public static class Controls
{
    //Movement
    public static KeyCode Jump = KeyCode.W;
    public static KeyCode Left = KeyCode.A;
    public static KeyCode Down = KeyCode.S;
    public static KeyCode Right = KeyCode.D;
    public static KeyCode Roll = KeyCode.K;
    //Actions
    public static KeyCode Grab = KeyCode.W;
    public static KeyCode Punch = KeyCode.J;
    public static KeyCode Uppercut = KeyCode.I;
    public static KeyCode Kick = KeyCode.L;

    public static void LoadKeyBinds()
    {
        var keybinds = GameData.Load("config/KeyBinds.json");

        if (keybinds == null || keybinds.Count == 0)
        {
            Debug.LogWarning("KeyBinds JSON is missing or empty. Using defaults.");
            return;
        }

        foreach (var category in keybinds)
        {
            foreach (var bind in category.Value)
            {
                string action = bind.Key;
                string keyString = bind.Value;

                if (Enum.TryParse(keyString, out KeyCode key))
                {
                    switch (action)
                    {
                        case "Jump": Jump = key; break;
                        case "Left": Left = key; break;
                        case "Right": Right = key; break;
                        case "Grab": Grab = key; break;
                        case "Punch": Punch = key; break;
                        case "Uppercut": Uppercut = key; break;
                        case "Kick": Kick = key; break;
                        case "Roll": Roll = key; break;
                        case "Crouch": Down = key; break;

                        default:
                            Debug.LogWarning($"Unknown keybind action '{action}' in config.");
                            break;
                    }
                }
                else
                {
                    Debug.LogWarning($"Could not parse '{keyString}' into KeyCode for action '{action}'.");
                }
            }
        }
    }
}
