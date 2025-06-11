using UnityEngine;

/// <summary>
/// Represents an object that can be broken when hit by the player's attack hitbox.
/// Upon breaking, the object's collider is destroyed and the object is destroyed after a delay.
/// </summary>
public class breakableObject : MonoBehaviour
{
    /// <summary>
    /// Delay in seconds before the object is destroyed after breaking.
    /// </summary>
    [SerializeField] private float destroyDelay = 1.5f;

    /// <summary>
    /// Indicates whether the object has already been broken.
    /// </summary>
    private bool isBroken = false;

    /// <summary>
    /// Called when another collider enters this trigger collider.
    /// Checks if the collider belongs to the player's attack hitbox and breaks the object if not already broken.
    /// </summary>
    /// <param name="collision">The collider that entered the trigger.</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isBroken && collision.CompareTag("PlayerAttackHitbox"))
        {
            BreakObject();
        }
    }

    /// <summary>
    /// Handles the breaking logic: marks the object as broken, destroys the BoxCollider2D component,
    /// triggers animations or effects (TODO), and schedules destruction of the object after a delay.
    /// </summary>
    private void BreakObject()
    {
        isBroken = true;

        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            Destroy(collider);
        }

        // TODO:
        // Add animations and/or effects here

        Destroy(gameObject, destroyDelay);
    }
}
