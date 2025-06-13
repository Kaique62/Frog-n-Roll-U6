using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles key binding configuration and UI display.
/// Loads, displays, saves, and resets player key bindings.
/// Supports both desktop and WebGL (browser) platforms.
/// </summary>
public class KeyBinds : MonoBehaviour
{
    // UI references for movement key texts
    [Header("Movement")]
    public TMP_Text Up;
    public TMP_Text Left;
    public TMP_Text Down;
    public TMP_Text Right;

    // UI references for action key texts
    [Header("Actions")]
    public TMP_Text Jump;
    public TMP_Text Grab;
    public TMP_Text Roll;
    public TMP_Text Punch;
    public TMP_Text Kick;

    /// <summary>
    /// Default key bindings used when no saved data is available.
    /// </summary>
    Dictionary<string, Dictionary<string, string>> DefaultKeys = new Dictionary<string, Dictionary<string, string>> {
        {
            "Player Binds", new Dictionary<string, string>{
                {"Up", "W"},
                {"Left", "A"},
                {"Down", "S"},
                {"Right", "D"},
                {"Jump", "SPACE"},
                {"Grab", "E"},
                {"Punch", "J"},
                {"Kick", "L"},
                {"Roll", "K"},
            }
        }
    };

    /// <summary>
    /// Holds the current key binding configuration in memory.
    /// </summary>
    Dictionary<string, Dictionary<string, string>> CurrentKeyBinds;

    /// <summary>
    /// Unity Start method. Loads key binds and updates the UI display.
    /// </summary>
    void Start()
    {
        LoadKeyBinds();
        ApplyToUI();
    }

    /// <summary>
    /// Loads key bindings from PlayerPrefs (WebGL) or JSON file (desktop).
    /// Falls back to default keys if no saved data is found.
    /// </summary>
    void LoadKeyBinds()
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            // Load key binds from PlayerPrefs (WebGL storage)
            CurrentKeyBinds = new Dictionary<string, Dictionary<string, string>> {
                { "Player Binds", new Dictionary<string, string>() }
            };

            foreach (var key in DefaultKeys["Player Binds"].Keys)
            {
                string savedKey = PlayerPrefs.GetString("KeyBind_" + key, DefaultKeys["Player Binds"][key]);
                CurrentKeyBinds["Player Binds"][key] = savedKey;
            }
        }
        else
        {
            // Load key binds from JSON file on disk
            CurrentKeyBinds = GameData.Load("config/KeyBinds.json");
            if (CurrentKeyBinds == null || CurrentKeyBinds.Count == 0)
            {
                CurrentKeyBinds = DefaultKeys;
            }
        }
    }

    /// <summary>
    /// Updates the on-screen text fields with the current key binding values.
    /// </summary>
    void ApplyToUI()
    {
        var binds = CurrentKeyBinds["Player Binds"];
        Up.text = binds["Up"];
        Left.text = binds["Left"];
        Down.text = binds["Down"];
        Right.text = binds["Right"];
        Jump.text = binds["Jump"];
        Grab.text = binds["Grab"];
        Roll.text = binds["Roll"];
        Punch.text = binds["Punch"];
        Kick.text = binds["Kick"];
    }

    /// <summary>
    /// Saves the current key bindings to PlayerPrefs (WebGL) or JSON file (desktop).
    /// Also reloads the controls to apply changes immediately.
    /// </summary>
    public void SaveData()
    {
        CurrentKeyBinds = new Dictionary<string, Dictionary<string, string>> {
            {
                "Player Binds", new Dictionary<string, string> {
                    {"Up", Up.text},
                    {"Left", Left.text},
                    {"Down", Down.text},
                    {"Right", Right.text},
                    {"Jump", Jump.text},
                    {"Grab", Grab.text},
                    {"Punch", Punch.text},
                    {"Kick", Kick.text},
                    {"Roll", Roll.text},
                }
            }
        };

        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            // Save key binds using PlayerPrefs (WebGL safe)
            foreach (var pair in CurrentKeyBinds["Player Binds"])
            {
                PlayerPrefs.SetString("KeyBind_" + pair.Key, pair.Value);
            }
            PlayerPrefs.Save();
        }
        else
        {
            // Save key binds to JSON file
            GameData.Save(CurrentKeyBinds, "config/KeyBinds.json");
        }

        Controls.LoadKeyBinds(); // Refresh controls with new bindings
    }

    /// <summary>
    /// Resets the UI fields to the default key bindings.
    /// Does not save automatically.
    /// </summary>
    public void resetKeys()
    {
        var binds = DefaultKeys["Player Binds"];
        Up.text = binds["Up"];
        Left.text = binds["Left"];
        Down.text = binds["Down"];
        Right.text = binds["Right"];
        Jump.text = binds["Jump"];
        Grab.text = binds["Grab"];
        Roll.text = binds["Roll"];
        Punch.text = binds["Punch"];
        Kick.text = binds["Kick"];
    }

    /// <summary>
    /// Closes the key bind configuration menu by unloading the scene.
    /// </summary>
    public void CloseKeyBindMenu()
    {
        SceneManager.UnloadSceneAsync("KeyBindMenu");
    }
}
