using UnityEngine;
using UnityEngine.Events;

public class EnemyController : MonoBehaviour
{
    public enum MovementType { Static, Patrol, ChasePlayer, FleePlayer, CustomPath }
    public enum MovementTrigger { Always, OnPlayerProximity, OnEvent }

    [Header("Combat Settings")]
    [SerializeField] private bool isDestructible = true;
    [SerializeField] private float damageCooldown = 0.5f;
    [SerializeField] private int health = 1;
    [SerializeField] private bool killsPlayerOnContact = true;
    [SerializeField] private bool ignoreRollingPlayer = false;

    [Header("Movement Settings")]
    [SerializeField] private MovementType movementType = MovementType.Static;
    [SerializeField] private MovementTrigger movementTrigger = MovementTrigger.Always;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private Vector2 movementDirection = Vector2.right;
    [SerializeField] private float patrolDistance = 5f;
    [SerializeField] private float playerDetectionRange = 5f;
    [SerializeField] private float movementCooldown = 1f;
    [SerializeField] private bool rotateTowardsCustomPath = false;
    [SerializeField] private bool lookAtCustomPathPoint = false;
    [SerializeField] private float rotationOffset = 0f;
    [SerializeField] private Transform[] customPathPoints;

    [Header("Visual Settings")]
    [SerializeField] private bool flipSpriteBasedOnPlayer = true;
    [SerializeField] private bool flipUsingScale = false;
    [SerializeField] private float flipThreshold = 0.1f;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Collision Settings")]
    [SerializeField] private Collider2D enemyCollider;
    [SerializeField] private bool restoreColliderAfterRoll = true;

    [Header("Events")]
    public UnityEvent onMovementStart;
    public UnityEvent onMovementEnd;
    public UnityEvent onPlayerDetected;

    // Private variables
    private Transform player;
    private PlayerController playerController;
    private bool canTakeDamage = true;
    private bool isMoving = false;
    private Vector3 originalPosition;
    private Vector3 targetPosition;
    private Vector3 originalScale;
    private int currentPathIndex = 0;
    private float movementTimer = 0f;
    private bool originalColliderState;
    private bool playerInRange = false;

    void Start()
    {
        InitializeReferences();
        originalPosition = transform.position;
        originalColliderState = enemyCollider != null ? enemyCollider.enabled : false;
        originalScale = transform.localScale; // Adicione esta linha
        
        if (movementTrigger == MovementTrigger.Always)
        {
            StartMovement();
        }
    }

    void Update()
    {
        HandleSpriteFlip();
        HandleRollCollision();
        HandleMovement();
        CheckPlayerProximity();
    }

    private void InitializeReferences()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                playerController = playerObj.GetComponent<PlayerController>();
            }
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (enemyCollider == null)
        {
            enemyCollider = GetComponent<Collider2D>();
        }
    }

    private void HandleSpriteFlip()
    {
        if (!flipSpriteBasedOnPlayer || player == null) return;

        float xDifference = player.position.x - transform.position.x;

        if (Mathf.Abs(xDifference) > flipThreshold)
        {
            bool shouldFlip = xDifference > 0;

            if (flipUsingScale)
            {
                Vector3 newScale = originalScale;
                newScale.x = Mathf.Abs(newScale.x) * (shouldFlip ? 1 : -1);
                transform.localScale = newScale;
            }
            else if (spriteRenderer != null)
            {
                spriteRenderer.flipX = !shouldFlip;
            }
        }
    }

    private void HandleRollCollision()
    {
        if (!ignoreRollingPlayer || playerController == null || enemyCollider == null) return;

        if (playerController.isRolling)
        {
            enemyCollider.enabled = false;
        }
        else if (restoreColliderAfterRoll)
        {
            enemyCollider.enabled = originalColliderState;
        }
    }

    private void CheckPlayerProximity()
    {
        if (movementTrigger != MovementTrigger.OnPlayerProximity || player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        bool newPlayerInRange = distanceToPlayer <= playerDetectionRange;

        if (newPlayerInRange && !playerInRange)
        {
            playerInRange = true;
            onPlayerDetected?.Invoke();
            StartMovement();
        }
        else if (!newPlayerInRange && playerInRange)
        {
            playerInRange = false;
            StopMovement();
        }
    }

    private void HandleMovement()
    {
        if (!isMoving) return;

        movementTimer += Time.deltaTime;

        switch (movementType)
        {
            case MovementType.Patrol:
                PatrolMovement();
                break;
            case MovementType.ChasePlayer:
                ChasePlayer();
                break;
            case MovementType.FleePlayer:
                FleePlayer();
                break;
            case MovementType.CustomPath:
                FollowCustomPath();
                break;
        }
    }

    private void PatrolMovement()
    {
        Vector3 targetPosition = originalPosition + (Vector3)(movementDirection.normalized * patrolDistance);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // Verifica se chegou ao ponto de destino ou à posição original
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f || 
            Vector3.Distance(transform.position, originalPosition) < 0.1f)
        {
            movementTimer += Time.deltaTime;
            if (movementTimer >= movementCooldown)
            {
                movementDirection *= -1;
                movementTimer = 0f;
            }
        }
    }

    private void ChasePlayer()
    {
        if (player == null) return;

        Vector2 direction = (player.position - transform.position).normalized;
        transform.Translate(direction * moveSpeed * Time.deltaTime);
    }

    private void FleePlayer()
    {
        if (player == null) return;

        Vector2 direction = (transform.position - player.position).normalized;
        transform.Translate(direction * moveSpeed * Time.deltaTime);
    }

    private void FollowCustomPath()
    {
        if (gameObject == null) return;

        if (customPathPoints == null || customPathPoints.Length == 0) return;

        targetPosition = customPathPoints[currentPathIndex].position;
        Vector2 direction = (targetPosition - transform.position).normalized;

        // ROTACIONA o objeto em direção ao ponto (útil em 3D ou inimigos que giram de verdade)
        if (rotateTowardsCustomPath)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle + rotationOffset);
        }

        // Apenas "vira o sprite visualmente", sem rotacionar o GameObject
        else if (lookAtCustomPathPoint)
        {
            bool shouldFaceRight = direction.x > 0;

            if (flipUsingScale)
            {
                Vector3 newScale = transform.localScale;
                newScale.x = Mathf.Abs(originalScale.x) * (shouldFaceRight ? 1 : -1);
                newScale.y = Mathf.Abs(originalScale.y); // Garante que Y NUNCA vire negativo
                newScale.z = originalScale.z;
                transform.localScale = newScale;
            }
            else if (spriteRenderer != null)
            {
                spriteRenderer.flipX = !shouldFaceRight;
            }
        }

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            currentPathIndex = (currentPathIndex + 1) % customPathPoints.Length;
            movementTimer = 0f;
        }
    }

    public void StartMovement()
    {
        if (isMoving) return;

        isMoving = true;
        onMovementStart?.Invoke();
    }

    public void StopMovement()
    {
        if (!isMoving) return;

        isMoving = false;
        onMovementEnd?.Invoke();
    }

    public void TriggerMovementFromEvent()
    {
        if (movementTrigger == MovementTrigger.OnEvent)
        {
            StartMovement();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (ignoreRollingPlayer && playerController != null && playerController.isRolling)
                return;

            PlayerController pc = collision.gameObject.GetComponent<PlayerController>();
            if (pc != null && killsPlayerOnContact)
            {
                pc.Die();
            }
        }
    }

    public void TakeDamage(int damageAmount = 1)
    {
        if (!canTakeDamage || !isDestructible) return;
        
        canTakeDamage = false;
        health -= damageAmount;

        if (health <= 0)
        {
            Die();
        }
        else
        {
            Invoke("ResetDamage", damageCooldown);
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }

    void ResetDamage() => canTakeDamage = true;

    void OnDestroy()
    {
        if (enemyCollider != null)
        {
            enemyCollider.enabled = originalColliderState;
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw detection range
        if (movementTrigger == MovementTrigger.OnPlayerProximity)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, playerDetectionRange);
        }

        // Draw patrol path
        if (movementType == MovementType.Patrol)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)movementDirection.normalized * patrolDistance);
        }

        // Draw custom path
        if (movementType == MovementType.CustomPath && customPathPoints != null)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < customPathPoints.Length; i++)
            {
                if (customPathPoints[i] != null)
                {
                    Gizmos.DrawSphere(customPathPoints[i].position, 0.25f);
                    if (i > 0 && customPathPoints[i-1] != null)
                    {
                        Gizmos.DrawLine(customPathPoints[i-1].position, customPathPoints[i].position);
                    }
                }
            }
        }
    }
}