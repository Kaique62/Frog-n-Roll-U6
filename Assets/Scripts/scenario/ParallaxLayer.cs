using UnityEngine;

/// <summary>
/// Controls a single layer of parallax scrolling based on camera movement.
/// </summary>
public class ParallaxLayer : MonoBehaviour
{
    /// <summary>
    /// Multiplier for horizontal parallax effect.
    /// Far backgrounds: 0.1–0.3 | Mid layers: 0.4–0.6 | Foreground: 0.7–0.9
    /// </summary>
    [Tooltip("Horizontal parallax. Far: 0.1–0.3 | Mid: 0.4–0.6 | Foreground: 0.7–0.9")]
    public float parallaxMultiplierX = 0.5f;

    /// <summary>
    /// Multiplier for vertical parallax effect.
    /// Far backgrounds: 0.05–0.2 | Mid layers: 0.2–0.4 | Foreground: 0.5–0.7
    /// </summary>
    [Tooltip("Vertical parallax. Far: 0.05–0.2 | Mid: 0.2–0.4 | Foreground: 0.5–0.7")]
    public float parallaxMultiplierY = 0.2f;

    /// <summary>
    /// Reference to the main camera's transform.
    /// </summary>
    private Transform cam;

    /// <summary>
    /// Stores the previous frame's camera position for delta calculation.
    /// </summary>
    private Vector3 previousCamPos;

    /// <summary>
    /// Initializes references and sets the starting camera position.
    /// </summary>
    void Start()
    {
        cam = Camera.main.transform;
        previousCamPos = cam.position;  
    }

    /// <summary>
    /// Updates the layer's position based on camera movement to simulate parallax.
    /// </summary>
    void LateUpdate()
    {
        Vector3 deltaMovement = cam.position - previousCamPos;
        transform.position += new Vector3(
            deltaMovement.x * parallaxMultiplierX,
            deltaMovement.y * parallaxMultiplierY,
            0f
        );
        previousCamPos = cam.position;
    }
}
