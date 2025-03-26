using UnityEngine;

public class BaseEnemy : MonoBehaviour
{
    // Variável pública para a vida do inimigo
    public int health = 1;

    // Start é chamado uma vez antes do primeiro frame
    void Start()
    {
        
    }

    // Update é chamado a cada frame
    void Update()
    {
        
    }

    // Detecta colisões com o inimigo
    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log(health);
        // Se colidir com o objeto de ataque do jogador...
        if (collision.gameObject.CompareTag("PlayerAttackHitbox"))
        {
            // Reduz a vida em 1
            health--;

            // Imprime a vida restante (opcional para debug)
            Debug.Log("Inimigo atingido! Vida restante: " + health);

            // Se a vida chegar a 0 ou menos, destrói o inimigo
            if (health <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
