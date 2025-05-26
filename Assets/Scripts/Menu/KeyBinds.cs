using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KeyBinds : MonoBehaviour
{
    //ButtonTexts
    [Header("Movement")]
    public TMP_Text Up;
    public TMP_Text Left;
    public TMP_Text Down;
    public TMP_Text Right;

    [Header("Actions")]
    public TMP_Text Jump;
    public TMP_Text Grab;
    public TMP_Text Roll;
    public TMP_Text Punch;
    public TMP_Text Kick;

    Dictionary<string, Dictionary<string, string>> UnifiedKeyBinds = new Dictionary<string, Dictionary<string, string>> {
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

    Dictionary<string, Dictionary<string, string>> CurrentKeyBinds = new Dictionary<string, Dictionary<string, string>>() {};

    void Start()
    {
        CurrentKeyBinds = GameData.Load("config/KeyBinds.json");

        if (CurrentKeyBinds == null || CurrentKeyBinds.Count == 0)
        {
            CurrentKeyBinds = UnifiedKeyBinds;
        }
        else
        {
            Up.text = CurrentKeyBinds["Player Binds"]["Up"];
            Left.text = CurrentKeyBinds["Player Binds"]["Left"];
            Down.text = CurrentKeyBinds["Player Binds"]["Down"];
            Right.text = CurrentKeyBinds["Player Binds"]["Right"];
            Jump.text = CurrentKeyBinds["Player Binds"]["Jump"];
            Grab.text = CurrentKeyBinds["Player Binds"]["Grab"];
            Roll.text = CurrentKeyBinds["Player Binds"]["Roll"];
            Punch.text = CurrentKeyBinds["Player Binds"]["Punch"];
            Kick.text = CurrentKeyBinds["Player Binds"]["Kick"];
        }
    }

    public void SaveData()
    {
        CurrentKeyBinds = new Dictionary<string, Dictionary<string, string>>(){
        {
            "Player Binds", new Dictionary<string, string>{
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
        GameData.Save(CurrentKeyBinds, "config/KeyBinds.json");
        Controls.LoadKeyBinds();
    }

    public void CloseKeyBindMenu() {
        SceneManager.UnloadSceneAsync("KeyBindMenu");
    }
}
