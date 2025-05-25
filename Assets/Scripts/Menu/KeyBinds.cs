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
    public TMP_Text Roll;

    Dictionary<string, Dictionary<string, string>> UnifiedKeyBinds = new Dictionary<string, Dictionary<string, string>> {
        {
            "Player Binds", new Dictionary<string, string>{
                {"Jump"  ,  "Space"},
                {"Up"    ,  "W"},
                {"Down"  ,  "S" },
                {"Left"  ,  "A"},
                {"Right" ,  "D"},
                {"Grab"  ,  "Space"},
                {"Punch" ,  "J"},
                {"Roll"  ,  "K"},
                {"Kick"  ,  "L"},
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
            Up.text = CurrentKeyBinds["Player Binds"]["Jump"];
        }
    }

    public void SaveData()
    {
        CurrentKeyBinds = new Dictionary<string, Dictionary<string, string>>(){
        {
            "Player Binds", new Dictionary<string, string>{
                {"Left", Left.text},
                {"Jump", Up.text},
                {"Right", Right.text},
                {"Grab", "E"},
                {"Punch", "J"},
                {"Uppercut", "I"},
                {"Kick", "L"},
                {"Roll", "K"},
                {"Crouch", Down.text}
            }
        }
        };
        GameData.Save(CurrentKeyBinds, "config/KeyBinds.json");
        Controls.LoadKeyBinds();
        CloseKeyBindMenu();
    }

    void CloseKeyBindMenu() {
        SceneManager.UnloadSceneAsync("KeyBindMenu");
    }
}
