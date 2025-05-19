using UnityEngine;

public class BoosterPlatform : MonoBehaviour
{
    [SerializeField] private float launchForce = 10f;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, launchForce);
            }
        }
    }
}
