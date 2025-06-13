using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Static class to manage configurable input key bindings.
/// Supports loading keybinds from PlayerPrefs (preferred) or fallback JSON config.
/// Provides helper methods for querying input states by action name.
/// </summary>
public static class Controls
{
    // Movement key bindings with default values
    public static KeyCode Up = KeyCode.W;
    public static KeyCode Left = KeyCode.A;
    public static KeyCode Down = KeyCode.S;
    public static KeyCode Right = KeyCode.D;

    // Action key bindings with default values
    public static KeyCode Punch = KeyCode.J;
    public static KeyCode Kick = KeyCode.L;
    public static KeyCode Grab = KeyCode.E;
    public static KeyCode Jump = KeyCode.Space;
    public static KeyCode Roll = KeyCode.K;

    /// <summary>
    /// Loads key bindings from PlayerPrefs if available,
    /// otherwise falls back to loading from JSON config file.
    /// </summary>
    public static void LoadKeyBinds()
    {
        // Try loading from PlayerPrefs first (new preferred system)
        if (TryLoadFromPlayerPrefs())
        {
            Debug.Log("Loaded keybinds from PlayerPrefs");
            return;
        }

        // Fallback: load from JSON config (old system)
        LoadFromJson();
    }

    /// <summary>
    /// Attempts to load keybinds from PlayerPrefs.
    /// Returns true if any keybind was loaded successfully.
    /// </summary>
    private static bool TryLoadFromPlayerPrefs()
    {
        bool loadedAny = false;

        // Iterate over all actions and try to load their keybind strings
        foreach (var action in new[] { "Up", "Left", "Down", "Right", "Jump", "Grab", "Punch", "Kick", "Roll" })
        {
            string prefKey = "KeyBind_" + action;
            if (PlayerPrefs.HasKey(prefKey))
            {
                string keyString = PlayerPrefs.GetString(prefKey);
                if (Enum.TryParse(keyString, out KeyCode keyCode))
                {
                    SetKey(action, keyCode);
                    loadedAny = true;
                }
            }
        }

        return loadedAny;
    }

    /// <summary>
    /// Loads keybinds from a JSON file.
    /// The JSON structure should be a dictionary of categories with action-key mappings.
    /// </summary>
    private static void LoadFromJson()
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            Debug.LogWarning("Skipping JSON load on WebGL — not supported.");
            return;
        }

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
                    SetKey(action, key);
                    Debug.Log($"Keybind updated: {action} = {key}");
                }
                else
                {
                    Debug.LogWarning($"Could not parse '{keyString}' into KeyCode for action '{action}'.");
                }
            }
        }
    }

    /// <summary>
    /// Assigns a KeyCode to the specified action.
    /// Logs a warning if the action is unknown.
    /// </summary>
    private static void SetKey(string action, KeyCode key)
    {
        switch (action)
        {
            case "Up": Up = key; break;
            case "Left": Left = key; break;
            case "Down": Down = key; break;
            case "Right": Right = key; break;
            case "Jump": Jump = key; break;
            case "Grab": Grab = key; break;
            case "Punch": Punch = key; break;
            case "Kick": Kick = key; break;
            case "Roll": Roll = key; break;
            default:
                Debug.LogWarning($"Unknown keybind action '{action}' in config.");
                break;
        }
    }

    /// <summary>
    /// Returns true if the key for the given action is currently held down.
    /// </summary>
    public static bool IsKeyDown(string action)
    {
        return Input.GetKey(GetKeyCode(action));
    }

    /// <summary>
    /// Returns true if the key for the given action was pressed this frame.
    /// </summary>
    public static bool IsKeyPressed(string action)
    {
        return Input.GetKeyDown(GetKeyCode(action));
    }

    /// <summary>
    /// Returns true if the key for the given action was released this frame.
    /// </summary>
    public static bool IsKeyReleased(string action)
    {
        return Input.GetKeyUp(GetKeyCode(action));
    }

    /// <summary>
    /// Gets the KeyCode associated with the specified action.
    /// Returns KeyCode.None if action is unknown.
    /// </summary>
    public static KeyCode GetKeyCode(string action)
    {
        return action switch
        {
            "Up" => Up,
            "Left" => Left,
            "Down" => Down,
            "Right" => Right,
            "Jump" => Jump,
            "Grab" => Grab,
            "Punch" => Punch,
            "Kick" => Kick,
            "Roll" => Roll,
            _ => KeyCode.None,
        };
    }
}
