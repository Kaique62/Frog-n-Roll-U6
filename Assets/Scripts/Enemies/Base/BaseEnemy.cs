using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private bool canTakeDamage = true;
    private Transform player;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Verifica a posição do player e vira o sprite
        if (player.position.x < transform.position.x) // Player está à esquerda
        {
            spriteRenderer.flipX = false; // Ou transform.localScale = new Vector3(1, 1, 1);
        }
        else // Player está à direita
        {
            spriteRenderer.flipX = true; // Ou transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                //player.Die();
            }
        }
    }

    public void TakeDamage()
    {
        if (!canTakeDamage) return;
        
        canTakeDamage = false;
        Invoke("ResetDamage", 0.5f); // 0.5s de cooldown
        Destroy(gameObject);
        Debug.Log("Morreu");
    }

    void ResetDamage() => canTakeDamage = true;
}