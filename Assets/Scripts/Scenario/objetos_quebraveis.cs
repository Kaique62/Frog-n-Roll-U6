using UnityEngine;

public class objetos_quebraveis : MonoBehaviour
{
    [SerializeField] private float destroyDelay = 1.5f;
    private bool isBroken = false;

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

        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            Destroy(collider);
        }

        //TODO:
        //Animations; Effects...

        Destroy(gameObject, destroyDelay);
    }
}
