using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KeyBinds : MonoBehaviour
{
    public TMP_Text feedback;

    Dictionary<string, Dictionary<string, string>> DefaultKeyBinds = new Dictionary<string, Dictionary<string, string>> {
        {
            "Menu Binds", new Dictionary<string, string>{
                {"Left", "LeftArrow"},
                {"Right", "RightArrow"},
                {"Up", "UpArrow"},
                {"Down", "DownArrow"},
                {"Confirm", "Enter"},
                {"Cancel", "Escape"},
            }
        },

        {
            "Player Binds", new Dictionary<string, string>{
                {"Jump", "Space"},
                {"Left", "A"},
                {"Right", "D"},
                {"Grab", "E"},
                {"Punch", "J"},
                {"Hook", "I"},
                {"Kick", "L"},
                {"Roll", "K"},
                {"Crouch", "S"}
            }
        }
    };

    Dictionary<string, Dictionary<string, string>> CurrentKeyBinds = new Dictionary<string, Dictionary<string, string>>() {};

    void Start()
    {
        CurrentKeyBinds = GameData.Load("config/KeyBinds.json");

        if (CurrentKeyBinds == null || CurrentKeyBinds.Count == 0)
        {
            CurrentKeyBinds = DefaultKeyBinds;
            feedback.text = "Using default KeyBinds!";
        }
        else 
        {
            feedback.text = "Dictionary contains data";
        }

        SaveData();
    }

    void Update()
    {
        // Currently empty, can be used for updates per frame
    }

    void SaveData(){
        GameData.Save(CurrentKeyBinds, "config/KeyBinds.json");
    }

    void CloseKeyBindMenu() {
        SceneManager.UnloadSceneAsync("PauseMenu");
    }
}
