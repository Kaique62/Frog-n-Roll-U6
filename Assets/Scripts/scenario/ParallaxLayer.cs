using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    [Tooltip("Horizontal parallax. Far: 0.1–0.3 | Mid: 0.4–0.6 | Foreground: 0.7–0.9")]
    public float parallaxMultiplierX = 0.5f;

    [Tooltip("Vertical parallax. Far: 0.05–0.2 | Mid: 0.2–0.4 | Foreground: 0.5–0.7")]
    public float parallaxMultiplierY = 0.2f;

    private Transform cam;
    private Vector3 previousCamPos;

    void Start()
    {
        cam = Camera.main.transform;
        previousCamPos = cam.position;  
    }

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
