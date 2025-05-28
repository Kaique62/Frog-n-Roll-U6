using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class KeyBinds : MonoBehaviour
{
    // Button Texts
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

    // Default key bindings
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

    // Current key bindings
    private Dictionary<string, KeyCode> currentKeyBinds = new Dictionary<string, KeyCode>();
    private bool isRebinding = false;
    private string currentRebindAction = "";
    private TMP_Text currentRebindText;

    void Start()
    {
        // Only show keyboard bindings on platforms that support them
        keyBindPanel.SetActive(Application.platform != RuntimePlatform.Android && 
                              Application.platform != RuntimePlatform.IPhonePlayer);
        
        // Show mobile controls on mobile platforms
        mobileControlsPanel.SetActive(Application.platform == RuntimePlatform.Android || 
                                    Application.platform == RuntimePlatform.IPhonePlayer);

        // Load saved key bindings or use defaults
        LoadKeyBinds();

        // Initialize UI with current bindings
        UpdateUI();
        
        // Setup close button
        closeButton.onClick.AddListener(CloseKeyBindMenu);
    }

    void Update()
    {
        // Handle key rebinding
        if (isRebinding && Input.anyKeyDown)
        {
            foreach(KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(keyCode))
                {
                    // Avoid binding escape key as it's used for closing
                    if (keyCode == KeyCode.Escape) break;
                    
                    // Set the new key binding
                    currentKeyBinds[currentRebindAction] = keyCode;
                    currentRebindText.text = keyCode.ToString();
                    
                    // End rebinding
                    isRebinding = false;
                    currentRebindText.color = Color.white;
                    SaveKeyBinds();
                    break;
                }
            }
        }
    }

    // Load key bindings from PlayerPrefs
    private void LoadKeyBinds()
    {
        currentKeyBinds = new Dictionary<string, KeyCode>();
        
        // Load each key binding or use default
        foreach (var key in DefaultKeys.Keys)
        {
            string savedKey = PlayerPrefs.GetString("KeyBind_" + key, DefaultKeys[key].ToString());
            
            if (System.Enum.TryParse(savedKey, out KeyCode keyCode))
            {
                currentKeyBinds[key] = keyCode;
            }
            else
            {
                currentKeyBinds[key] = DefaultKeys[key];
            }
        }
    }

    // Save key bindings to PlayerPrefs
    private void SaveKeyBinds()
    {
        foreach (var key in currentKeyBinds.Keys)
        {
            PlayerPrefs.SetString("KeyBind_" + key, currentKeyBinds[key].ToString());
        }
        PlayerPrefs.Save();
        
        // Update controls in game
        Controls.LoadKeyBinds();
    }

    // Update UI with current key bindings
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

    // Start rebinding process for a specific action
    public void StartRebind(string action, TMP_Text textElement)
    {
        // Only allow rebinding on keyboard-supported platforms
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

    // Reset all keys to defaults
    public void ResetKeys()
    {
        foreach (var key in DefaultKeys.Keys)
        {
            currentKeyBinds[key] = DefaultKeys[key];
        }
        UpdateUI();
        SaveKeyBinds();
    }

    // Close the key binding menu
    public void CloseKeyBindMenu()
    {
        SceneManager.UnloadSceneAsync("KeyBindMenu");
    }

    // Helper methods for UI buttons
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