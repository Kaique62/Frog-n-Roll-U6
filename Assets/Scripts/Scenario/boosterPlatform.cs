using UnityEngine;

public class BoosterPlatform2D : MonoBehaviour
{
    public Vector2 launchDirection = Vector2.up; // Direção do impulso
    public float launchForce = 10f; // Força do impulso
    public string playerTag = "Player"; // Tag do jogador

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag(playerTag))
        {
            Rigidbody2D rb = collision.collider.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero; // Correto: zera a velocidade antes do impulso
                rb.AddForce(launchDirection.normalized * launchForce, ForceMode2D.Impulse);
            }
        }
    }
}
