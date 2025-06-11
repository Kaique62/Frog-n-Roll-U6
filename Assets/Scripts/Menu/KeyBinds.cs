using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Handles customizable key bindings for player controls and UI updates.
/// Supports loading, saving, rebinding, and resetting keys.
/// </summary>
public class KeyBinds : MonoBehaviour
{
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
    
    [Header("UI Elements")]
    public GameObject keyBindPanel;
    public GameObject mobileControlsPanel;
    public Button closeButton;

    /// <summary>
    /// Default key bindings.
    /// </summary>
    private Dictionary<string, KeyCode> DefaultKeys = new Dictionary<string, KeyCode> {
        {"Up", KeyCode.W},
        {"Left", KeyCode.A},
        {"Down", KeyCode.S},
        {"Right", KeyCode.D},
        {"Jump", KeyCode.Space},
        {"Grab", KeyCode.E},
        {"Punch", KeyCode.J},
        {"Kick", KeyCode.L},
        {"Roll", KeyCode.K},
    };

    private Dictionary<string, KeyCode> currentKeyBinds = new Dictionary<string, KeyCode>();
    private bool isRebinding = false;
    private string currentRebindAction = "";
    private TMP_Text currentRebindText;

    /// <summary>
    /// Initializes key binding UI and loads saved or default keys.
    /// </summary>
    void Start()
    {
        keyBindPanel.SetActive(Application.platform != RuntimePlatform.Android && 
                              Application.platform != RuntimePlatform.IPhonePlayer);
        
        mobileControlsPanel.SetActive(Application.platform == RuntimePlatform.Android || 
                                     Application.platform == RuntimePlatform.IPhonePlayer);

        LoadKeyBinds();
        UpdateUI();
        closeButton.onClick.AddListener(CloseKeyBindMenu);
    }

    /// <summary>
    /// Handles rebinding process when waiting for new key input.
    /// </summary>
    void Update()
    {
        if (isRebinding && Input.anyKeyDown)
        {
            foreach(KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(keyCode))
                {
                    if (keyCode == KeyCode.Escape) break;

                    currentKeyBinds[currentRebindAction] = keyCode;
                    currentRebindText.text = keyCode.ToString();

                    isRebinding = false;
                    currentRebindText.color = Color.white;
                    SaveKeyBinds();
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Loads key bindings from PlayerPrefs or defaults if not found.
    /// </summary>
    private void LoadKeyBinds()
    {
        currentKeyBinds = new Dictionary<string, KeyCode>();
        foreach (var key in DefaultKeys.Keys)
        {
            string savedKey = PlayerPrefs.GetString("KeyBind_" + key, DefaultKeys[key].ToString());

            if (System.Enum.TryParse(savedKey, out KeyCode keyCode))
                currentKeyBinds[key] = keyCode;
            else
                currentKeyBinds[key] = DefaultKeys[key];
        }
    }

    /// <summary>
    /// Saves current key bindings to PlayerPrefs and updates controls.
    /// </summary>
    private void SaveKeyBinds()
    {
        foreach (var key in currentKeyBinds.Keys)
            PlayerPrefs.SetString("KeyBind_" + key, currentKeyBinds[key].ToString());

        PlayerPrefs.Save();
        Controls.LoadKeyBinds();
    }

    /// <summary>
    /// Updates the UI texts to reflect current key bindings.
    /// </summary>
    private void UpdateUI()
    {
        Up.text = currentKeyBinds["Up"].ToString();
        Left.text = currentKeyBinds["Left"].ToString();
        Down.text = currentKeyBinds["Down"].ToString();
        Right.text = currentKeyBinds["Right"].ToString();
        Jump.text = currentKeyBinds["Jump"].ToString();
        Grab.text = currentKeyBinds["Grab"].ToString();
        Roll.text = currentKeyBinds["Roll"].ToString();
        Punch.text = currentKeyBinds["Punch"].ToString();
        Kick.text = currentKeyBinds["Kick"].ToString();
    }

    /// <summary>
    /// Begins rebinding a specific action if the platform supports keyboard input.
    /// </summary>
    /// <param name="action">Action name to rebind.</param>
    /// <param name="textElement">UI Text element to update during rebinding.</param>
    public void StartRebind(string action, TMP_Text textElement)
    {
        if (Application.platform == RuntimePlatform.Android || 
            Application.platform == RuntimePlatform.IPhonePlayer)
        {
            return;
        }

        if (!isRebinding)
        {
            isRebinding = true;
            currentRebindAction = action;
            currentRebindText = textElement;
            textElement.text = "PRESS KEY...";
            textElement.color = Color.yellow;
        }
    }

    /// <summary>
    /// Resets all key bindings to their default values.
    /// </summary>
    public void ResetKeys()
    {
        foreach (var key in DefaultKeys.Keys)
            currentKeyBinds[key] = DefaultKeys[key];

        UpdateUI();
        SaveKeyBinds();
    }

    /// <summary>
    /// Closes the key binding menu scene.
    /// </summary>
    public void CloseKeyBindMenu()
    {
        SceneManager.UnloadSceneAsync("KeyBindMenu");
    }

    // UI Button helper methods
    public void RebindUp() => StartRebind("Up", Up);
    public void RebindLeft() => StartRebind("Left", Left);
    public void RebindDown() => StartRebind("Down", Down);
    public void RebindRight() => StartRebind("Right", Right);
    public void RebindJump() => StartRebind("Jump", Jump);
    public void RebindGrab() => StartRebind("Grab", Grab);
    public void RebindRoll() => StartRebind("Roll", Roll);
    public void RebindPunch() => StartRebind("Punch", Punch);
    public void RebindKick() => StartRebind("Kick", Kick);
}
