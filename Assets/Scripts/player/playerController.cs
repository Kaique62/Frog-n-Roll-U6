using UnityEngine;
using System.Collections;
using System.Threading.Tasks;

public class PlayerController : MonoBehaviour
{
    // --- MUDANÇA: Voltamos a usar os campos públicos para arrastar no Inspector ---
    [Header("Game Over")]
    public GameObject gameOverPrefab;
    public Transform canvasTransform;

    [Header("Player Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float rollDistance = 3f;
    public float rollDuration = 0.5f;
    public float attackDuration = 0.3f;
    public float stompFallSpeed = 15f;
    public float uppercutJumpForce = 7f;
    public float coyoteTime = 0.15f;
    public float jumpBufferTime = 0.1f;

    [Header("Rope Swing Settings")]
    public Transform ropeDetectionPoint;
    public float detectionRadius = 0.5f;
    public float swingForceMultiplier = 1.2f;
    public float releaseJumpBoost = 5f;

    [Header("Hitboxes")]
    public GameObject punchHitbox;
    public GameObject kickHitbox;
    public GameObject uppercutHitbox;
    public GameObject stompHitbox;

    private Rigidbody2D rb;
    private Animator animator;
    private BoxCollider2D boxCollider;

    private Vector2 originalColliderSize;
    private Vector2 originalColliderOffset;
    private Vector2 lastVelocity;

    private HingeJoint2D ropeHinge;
    private Rigidbody2D connectedRopeSegment;

    private bool isGrounded;
    public bool isRolling;
    private bool isAttacking;
    private bool isSwinging;
    private bool canAttach = true;
    private bool isDead;

    private float coyoteTimeCounter;
    private float jumpBufferCounter;

    private string currentAnimation = "";

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();

        originalColliderSize = boxCollider.size;
        originalColliderOffset = boxCollider.offset;

        punchHitbox.SetActive(false);
        kickHitbox.SetActive(false);
        uppercutHitbox.SetActive(false);
        stompHitbox.SetActive(false);

        ropeHinge = gameObject.AddComponent<HingeJoint2D>();
        ropeHinge.enabled = false;
    }

    // O método Die() usa as referências públicas que vamos configurar
    public void Die()
    {
        if (isDead) return;
        isDead = true;
        rb.isKinematic = true;
        rb.linearVelocity = Vector2.zero;
        GetComponent<Collider2D>().enabled = false;
        PlayAnimation("Die");

        if (gameOverPrefab != null && canvasTransform != null)
        {
            GameObject gameOverInstance = Instantiate(gameOverPrefab, canvasTransform);
            GameOverManager gameOverManager = gameOverInstance.GetComponentInChildren<GameOverManager>();
            if (gameOverManager != null)
            {
                gameOverManager.ShowGameOver();
            }
            else
            {
                Debug.LogError("O prefab de Game Over (e seus filhos) não possui o script GameOverManager!");
            }
        }
        else
        {
            Debug.LogError("O Prefab de Game Over ou o Canvas Transform não foram atribuídos no Inspector do Player!");
        }
    }


    // ... O resto do seu código (Update, HandleInputs, OnCollisionEnter2D, etc.) permanece aqui ...
    [System.Obsolete]
    void Update()
    {
        if (boxCollider.gameObject.CompareTag("FireBorder")) Debug.Log("died");
        if (isDead) return;
        if (isSwinging) { if (Input.GetKeyDown(Controls.Jump) || MobileInput.GetHeld("Up")) DetachFromRope(); return; }
        if ((Input.GetKeyDown(Controls.Grab) || MobileInput.GetHeld("Grab")) && canAttach) TryAttachToRope();
        HandleTimers();
        HandleInputs();
        if (!isGrounded && !isRolling && !isAttacking) { PlayAnimation("Jump"); }
    }
    [System.Obsolete]
    private void HandleInputs() { HandleMovement(); HandleJump(); HandleRoll(); HandleAttack(); }
    private void HandleMovement() { if (isRolling || isAttacking || isSwinging) return; float moveInput = (Input.GetKey(Controls.Right) || MobileInput.GetHeld("Right")) ? 1f : (Input.GetKey(Controls.Left) || MobileInput.GetHeld("Left")) ? -1f : 0f; Vector2 velocity = rb.linearVelocity; velocity.x = moveInput * moveSpeed; rb.linearVelocity = velocity; if (moveInput != 0) { if (isGrounded) PlayAnimation("Run"); transform.localScale = new Vector3(Mathf.Sign(moveInput), 1, 1); } else { if (isGrounded && !isAttacking) PlayAnimation("Idle"); } lastVelocity = rb.linearVelocity; }
    private void HandleJump() { if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f) { rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce); PlayAnimation("Jump"); jumpBufferCounter = 0f; coyoteTimeCounter = 0f; } }
    private void HandleRoll() { if (isGrounded && (Input.GetKeyDown(Controls.Roll) || MobileInput.GetHeld("Roll")) && !isRolling) { StartCoroutine(PerformRoll()); } }
    [System.Obsolete]
    private void HandleAttack() { if (isAttacking) return; if ((Input.GetKeyDown(Controls.Punch) || MobileInput.GetHeld("Punch")) && (Input.GetKey(Controls.Up) || MobileInput.GetHeld("Up"))) { if (Mathf.Abs(rb.linearVelocity.x) > 0.1f) { StartCoroutine(PerformAttack(uppercutHitbox, "UpPunch-Run")); } else { StartCoroutine(PerformAttack(uppercutHitbox, "UpPunch")); } } else if (!isGrounded && (Input.GetKeyDown(Controls.Kick) || MobileInput.GetHeld("Kick")) && (Input.GetKey(Controls.Down) || MobileInput.GetHeld("Down"))) { StartCoroutine(PerformStomp()); } else if (Input.GetKeyDown(Controls.Punch) || MobileInput.GetHeld("Punch")) { if (!isGrounded) StartCoroutine(PerformAttack(punchHitbox, "Punch-Jump")); else if (Mathf.Abs(rb.linearVelocity.x) > 0.1f) StartCoroutine(PerformAttack(punchHitbox, "Punch-Run")); else StartCoroutine(PerformAttack(punchHitbox, "Punch")); } else if (Input.GetKeyDown(Controls.Kick) || MobileInput.GetHeld("Kick")) { if (!isGrounded) StartCoroutine(PerformAttack(kickHitbox, "Kick-Jump")); else if (Mathf.Abs(rb.linearVelocity.x) > 0.1f) StartCoroutine(PerformAttack(kickHitbox, "Kick-Run")); else StartCoroutine(PerformAttack(kickHitbox, "Kick")); } }
    private void HandleTimers() { if (isGrounded) coyoteTimeCounter = coyoteTime; else coyoteTimeCounter -= Time.deltaTime; if (Input.GetKeyDown(Controls.Jump) || MobileInput.GetHeld("Up")) jumpBufferCounter = jumpBufferTime; else jumpBufferCounter -= Time.deltaTime; }
    private void PlayAnimation(string animationName) { if (currentAnimation == animationName) return; currentAnimation = animationName; animator.Play(animationName); }
    [System.Obsolete]
    IEnumerator PerformAttack(GameObject hitbox, string animationName) { isAttacking = true; PlayAnimation(animationName); hitbox.SetActive(true); Collider2D[] hits = Physics2D.OverlapBoxAll(hitbox.transform.position, hitbox.GetComponent<BoxCollider2D>().size, 0); foreach (var hit in hits) { if (hit.CompareTag("Enemy")) { var timer = hit.GetComponent<ActionTimer>(); if (timer != null) FindObjectOfType<LevelController>().AddScore(timer.Delay); hit.GetComponent<EnemyController>()?.TakeDamage(); } } yield return new WaitForSeconds(attackDuration); hitbox.SetActive(false); isAttacking = false; }
    IEnumerator PerformStomp() { isAttacking = true; PlayAnimation("Stomp"); stompHitbox.SetActive(true); rb.linearVelocity = new Vector2(rb.linearVelocity.x, -stompFallSpeed); yield return new WaitForSeconds(attackDuration); stompHitbox.SetActive(false); isAttacking = false; }
    IEnumerator PerformRoll() { isRolling = true; PlayAnimation("Roll"); boxCollider.size = new Vector2(originalColliderSize.x, originalColliderSize.y / 2); boxCollider.offset = new Vector2(originalColliderOffset.x, originalColliderOffset.y - originalColliderSize.y / 4); float speed = rollDistance / rollDuration; rb.linearVelocity = new Vector2(transform.localScale.x * speed, rb.linearVelocity.y); yield return new WaitForSeconds(rollDuration); boxCollider.size = originalColliderSize; boxCollider.offset = originalColliderOffset; rb.linearVelocity = Vector2.zero; isRolling = false; }
    void OnCollisionEnter2D(Collision2D col) { if (col.gameObject.CompareTag("Ground")) isGrounded = true; if (col.gameObject.CompareTag("FireBorder")) { Debug.Log("died"); Die(); } }
    void OnCollisionExit2D(Collision2D col) { if (col.gameObject.CompareTag("Ground")) isGrounded = false; }
    void TryAttachToRope() { var cols = Physics2D.OverlapCircleAll(ropeDetectionPoint.position, detectionRadius); foreach (var col in cols) { if (col.CompareTag("Rope")) { var rb2 = col.GetComponent<Rigidbody2D>(); if (rb2 != null) { AttachToRope(rb2); break; } } } }
    void AttachToRope(Rigidbody2D ropeSegment) { isSwinging = true; canAttach = false; connectedRopeSegment = ropeSegment; connectedRopeSegment.linearVelocity = lastVelocity * swingForceMultiplier; ropeHinge.connectedBody = ropeSegment; ropeHinge.autoConfigureConnectedAnchor = false; ropeHinge.anchor = transform.InverseTransformPoint(ropeDetectionPoint.position); ropeHinge.connectedAnchor = Vector2.zero; ropeHinge.enabled = true; }
    void DetachFromRope() { isSwinging = false; ropeHinge.enabled = false; canAttach = true; rb.linearVelocity = connectedRopeSegment.linearVelocity; rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y + releaseJumpBoost); }
}
