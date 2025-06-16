using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class breakableObject : MonoBehaviour
{
    [SerializeField] private float destroyDelay = 1.5f;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private Vector2 pushDirection = new Vector2(-1f, 1f);
    [SerializeField] private float pushSpeed = 2f;
    [SerializeField] private float rotationSpeed = 180f; // graus por segundo

    private bool isBroken = false;
    private SpriteRenderer spriteRenderer;
    private Collider2D myCollider;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        myCollider = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isBroken && collision.CompareTag("PlayerAttackHitbox"))
        {
            BreakObject();
        }
    }

    private void BreakObject()
    {
        isBroken = true;

        if (myCollider != null)
            myCollider.enabled = false;

        // Começa movimento e fade-out
        StartCoroutine(BreakEffect());

        // Destroi o objeto depois do tempo definido
        Destroy(gameObject, destroyDelay);
    }

    private System.Collections.IEnumerator BreakEffect()
    {
        float elapsed = 0f;
        Color originalColor = spriteRenderer.color;

        while (elapsed < fadeDuration)
        {
            // Movimento
            transform.position += (Vector3)(pushDirection.normalized * pushSpeed * Time.deltaTime);
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

            // Fade-out
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Garante opacidade zero no final
        spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
    }
}
