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
                {"Esquerda", "LeftArrow"},
                {"Direita", "RightArrow"},
                {"Cima", "UpArrow"},
                {"Baixo", "DownArrow"},
                {"Confirm", "Enter"},
                {"Cancel", "Escape"},
            }
        },

        {
            "Player Binds", new Dictionary<string, string>{
                {"Pulo", "Space"},
                {"Esquerda", "A"},
                {"Direita", "D"},
                {"Agarrar", "E"},
                {"Soco", "J"},
                {"Gancho", "I"},
                {"Chute", "L"},
                {"Rolar", "K"},
                {"Agachar", "S"}
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
            feedback.text = "Utilizando KeyBinds Padrões!";
        }
        else 
        {
            feedback.text = "Dicionário contém dados";
        }

        SaveData();
    }


    void Update()
    {
        
    }

    void SaveData(){
        GameData.Save(CurrentKeyBinds, "config/KeyBinds.json");
    }

    void closeKeyBindMenu() {
        SceneManager.UnloadSceneAsync("PauseMenu");
    }

}
