using UnityEngine;

/// <summary>
/// A 2D platform that boosts the player by applying an impulse force
/// in a specified direction when the player collides with it.
/// </summary>
public class BoosterPlatform2D : MonoBehaviour
{
    /// <summary>
    /// The direction of the launch impulse.
    /// </summary>
    public Vector2 launchDirection = Vector2.up;

    /// <summary>
    /// The strength of the launch impulse.
    /// </summary>
    public float launchForce = 10f;

    /// <summary>
    /// The tag used to identify the player object.
    /// </summary>
    public string playerTag = "Player";

    /// <summary>
    /// Called when another collider makes contact with this object's collider.
    /// Applies an impulse force to the player's Rigidbody2D if the tag matches.
    /// </summary>
    /// <param name="collision">Collision data associated with the collision event.</param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag(playerTag))
        {
            Rigidbody2D rb = collision.collider.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // Reset the player's velocity before applying the launch force
                rb.linearVelocity = Vector2.zero;

                // Apply an impulse force in the normalized launch direction
                rb.AddForce(launchDirection.normalized * launchForce, ForceMode2D.Impulse);
            }
        }
    }
}
