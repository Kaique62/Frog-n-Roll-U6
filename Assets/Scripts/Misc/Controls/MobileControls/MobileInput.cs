using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages mobile input states including Held, Pressed, and Released for various input keys.
/// Tracks input changes frame by frame for use in gameplay.
/// </summary>
public class MobileInput : MonoBehaviour
{
    /// <summary>
    /// Dictionary tracking which inputs are currently held down.
    /// </summary>
    public static Dictionary<string, bool> Held = new Dictionary<string, bool>();

    /// <summary>
    /// Dictionary tracking which inputs were pressed down this frame.
    /// </summary>
    public static Dictionary<string, bool> Pressed = new Dictionary<string, bool>();

    /// <summary>
    /// Dictionary tracking which inputs were released this frame.
    /// </summary>
    public static Dictionary<string, bool> Released = new Dictionary<string, bool>();

    // Tracks input states from the previous frame for comparison
    private Dictionary<string, bool> lastFrame = new Dictionary<string, bool>();

    /// <summary>
    /// Initialize all input keys to false in all state dictionaries.
    /// </summary>
    void Awake()
    {
        string[] keys = {
            // Movement keys
            "Left", "Right", "Up", "Down",
            // Action keys
            "Punch", "Kick", "Roll", "Jump", "Grab"
        };

        foreach (var key in keys)
        {
            Held[key] = false;
            Pressed[key] = false;
            Released[key] = false;
            lastFrame[key] = false;
        }
    }

    /// <summary>
    /// Sets the current Held state for the given input key.
    /// </summary>
    /// <param name="key">The input key to update.</param>
    /// <param name="state">Whether the input is currently held.</param>
    public void SetInput(string key, bool state)
    {
        // Ensure dictionaries contain the key
        if (!Held.ContainsKey(key)) Held[key] = false;
        if (!Pressed.ContainsKey(key)) Pressed[key] = false;
        if (!Released.ContainsKey(key)) Released[key] = false;
        if (!lastFrame.ContainsKey(key)) lastFrame[key] = false;

        Held[key] = state;
    }

    /// <summary>
    /// Updates Pressed and Released states each frame by comparing current and last frame Held states.
    /// </summary>
    void Update()
    {
        foreach (var key in Held.Keys)
        {
            bool isHeld = Held[key];
            bool wasHeld = lastFrame[key];

            Pressed[key] = isHeld && !wasHeld;
            Released[key] = !isHeld && wasHeld;

            lastFrame[key] = isHeld;
        }

        // Example usage logs
        if (Pressed["Down"])
            Debug.Log("Pressed Down this frame");

        if (Released["Down"])
            Debug.Log("Released Down this frame");
    }

    /// <summary>
    /// Returns true if the specified input key was pressed this frame.
    /// </summary>
    public static bool GetPressed(string key) => Pressed.ContainsKey(key) && Pressed[key];

    /// <summary>
    /// Returns true if the specified input key is currently held down.
    /// </summary>
    public static bool GetHeld(string key) => Held.ContainsKey(key) && Held[key];

    /// <summary>
    /// Returns true if the specified input key was released this frame.
    /// </summary>
    public static bool GetReleased(string key) => Released.ContainsKey(key) && Released[key];

    public static void ResetAllInput()
    {
        Debug.Log("Resetting all mobile input states.");
        var keys = new List<string>(Held.Keys);
        foreach (var key in keys)
        {
            Held[key] = false;
            Pressed[key] = false;
            Released[key] = false;
        }
    }
}
