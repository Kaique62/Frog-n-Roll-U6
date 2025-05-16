using UnityEngine;
using System.Collections.Generic;

public class MobileInput : MonoBehaviour
{
    public static Dictionary<string, bool> Held = new Dictionary<string, bool>();
    public static Dictionary<string, bool> Pressed = new Dictionary<string, bool>();
    public static Dictionary<string, bool> Released = new Dictionary<string, bool>();

    private Dictionary<string, bool> lastFrame = new Dictionary<string, bool>();

    void Awake()
    {
        string[] keys = { "Left", "Right", "Up", "Down" };
        foreach (var key in keys)
        {
            Held[key] = false;
            Pressed[key] = false;
            Released[key] = false;
            lastFrame[key] = false;
        }
    }

    public void SetInput(string key, bool state)
    {
        if (!Held.ContainsKey(key)) Held[key] = false;
        if (!Pressed.ContainsKey(key)) Pressed[key] = false;
        if (!Released.ContainsKey(key)) Released[key] = false;
        if (!lastFrame.ContainsKey(key)) lastFrame[key] = false;

        Held[key] = state;
    }

    void Update()
    {
        // Calculate Pressed and Released states at the start of the frame
        foreach (var key in Held.Keys)
        {
            bool isHeld = Held[key];
            bool wasHeld = lastFrame[key];

            Pressed[key] = isHeld && !wasHeld;
            Released[key] = !isHeld && wasHeld;

            lastFrame[key] = isHeld;
        }

        // Example logging (now using current frame's values)
        if (Pressed["Down"])
            Debug.Log("Pressed Down this frame");

        if (Released["Down"])
            Debug.Log("Released Down this frame");
    }

    public static bool GetPressed(string key) => Pressed.ContainsKey(key) && Pressed[key];
    public static bool GetHeld(string key) => Held.ContainsKey(key) && Held[key];
    public static bool GetReleased(string key) => Released.ContainsKey(key) && Released[key];
}