using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
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
    private bool isCrouching;
    private bool isSwinging;
    private bool canAttach = true;
    private bool isDead;

    private float coyoteTimeCounter;
    private float jumpBufferCounter;

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

    void Update()
    {
        if (isDead) return;

        if (isSwinging)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W))
                DetachFromRope();
            return;
        }

        if (Input.GetKeyDown(KeyCode.E) && canAttach)
            TryAttachToRope();

        HandleTimers();
        HandleInputs();
    }

    private void HandleInputs()
    {
        HandleCrouch();
        HandleMovement();
        HandleJump();
        HandleRoll();
        HandleAttack();
    }

    private void HandleMovement()
    {
        if (isRolling) return;

        float moveInput = Input.GetKey(KeyCode.D) ? 1f : Input.GetKey(KeyCode.A) ? -1f : 0f;
        Vector2 velocity = rb.linearVelocity;
        velocity.x = moveInput * moveSpeed;
        rb.linearVelocity = velocity;

        animator.SetBool("isRunning", moveInput != 0);

        if (moveInput != 0)
            transform.localScale = new Vector3(Mathf.Sign(moveInput), 1, 1);

        lastVelocity = rb.linearVelocity;
    }

    private void HandleJump()
    {
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            if (isCrouching) ExitCrouch();

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            TriggerAnimation("Jump");

            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;
        }

        animator.SetBool("isJumping", rb.linearVelocity.y > 0.1f);
    }

    private void HandleRoll()
    {
        if (isGrounded && Input.GetKeyDown(KeyCode.K) && !isRolling)
        {
            if (isCrouching) ExitCrouch();
            StartCoroutine(PerformRoll());
            TriggerAnimation("Roll");
        }
    }

    private void HandleAttack()
    {
        if (isAttacking) return;

        if (isCrouching) ExitCrouch();

        if (Input.GetKeyDown(KeyCode.I))
        {
            StartCoroutine(PerformUppercut());
            TriggerAnimation("Uppercut");
        }
        else if (!isGrounded && Input.GetKeyDown(KeyCode.L) && Input.GetKey(KeyCode.S))
        {
            StartCoroutine(PerformStomp());
            TriggerAnimation("Stomp");
        }
        else if (Input.GetKeyDown(KeyCode.J))
        {
            StartCoroutine(PerformAttack(punchHitbox));
            TriggerAnimation("Punch");
        }
        else if (Input.GetKeyDown(KeyCode.L))
        {
            StartCoroutine(PerformAttack(kickHitbox));
            TriggerAnimation("Kick");
        }
    }

    private void HandleCrouch()
    {
        if (isRolling || !isGrounded) return;

        if (Input.GetKey(KeyCode.S) && !isCrouching)
        {
            isCrouching = true;
            animator.SetBool("isCrouching", true);
            boxCollider.size = new Vector2(originalColliderSize.x, originalColliderSize.y / 2);
            boxCollider.offset = new Vector2(originalColliderOffset.x, originalColliderOffset.y - originalColliderSize.y / 4);
        }
        else if (!Input.GetKey(KeyCode.S) && isCrouching)
        {
            ExitCrouch();
        }
    }

    private void HandleTimers()
    {
        if (isGrounded) coyoteTimeCounter = coyoteTime;
        else coyoteTimeCounter -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space))
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;
    }

    private void ExitCrouch()
    {
        isCrouching = false;
        animator.SetBool("isCrouching", false);
        boxCollider.size = originalColliderSize;
        boxCollider.offset = originalColliderOffset;
    }

    private void TriggerAnimation(string trigger)
    {
        animator.ResetTrigger("Punch");
        animator.ResetTrigger("Kick");
        animator.ResetTrigger("Uppercut");
        animator.ResetTrigger("Stomp");
        animator.ResetTrigger("Jump");
        animator.ResetTrigger("Roll");
        animator.SetTrigger(trigger);
    }

    IEnumerator PerformAttack(GameObject hitbox)
    {
        isAttacking = true;
        hitbox.SetActive(true);

        Collider2D[] hits = Physics2D.OverlapBoxAll(hitbox.transform.position, hitbox.GetComponent<BoxCollider2D>().size, 0);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                var timer = hit.GetComponent<ActionTimer>();
                if (timer != null)
                    FindObjectOfType<LevelController>().AddScore(timer.Delay);

                hit.GetComponent<EnemyController>()?.TakeDamage();
            }
        }

        yield return new WaitForSeconds(attackDuration);
        hitbox.SetActive(false);
        isAttacking = false;
    }

    IEnumerator PerformUppercut()
    {
        isAttacking = true;
        uppercutHitbox.SetActive(true);
        yield return new WaitForSeconds(attackDuration);
        uppercutHitbox.SetActive(false);
        isAttacking = false;
    }

    IEnumerator PerformStomp()
    {
        isAttacking = true;
        stompHitbox.SetActive(true);
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, -stompFallSpeed);
        yield return new WaitForSeconds(attackDuration);
        stompHitbox.SetActive(false);
        isAttacking = false;
    }

    IEnumerator PerformRoll()
    {
        isRolling = true;
        // Enter crouched-collider state
        boxCollider.size = new Vector2(originalColliderSize.x, originalColliderSize.y / 2);
        boxCollider.offset = new Vector2(originalColliderOffset.x, originalColliderOffset.y - originalColliderSize.y / 4);

        // Roll movement
        float speed = rollDistance / rollDuration;
        rb.linearVelocity = new Vector2(transform.localScale.x * speed, rb.linearVelocity.y);
        yield return new WaitForSeconds(rollDuration);

        // After roll duration, only extend if something above blocks standing
        bool blocked;
        do
        {
            blocked = Physics2D.BoxCast(
                (Vector2)transform.position + boxCollider.offset,
                originalColliderSize,
                0f,
                Vector2.up,
                0.01f,
                LayerMask.GetMask("Default")
            ).collider != null;
            if (blocked)
                yield return null;
        } while (blocked);

        // Restore original collider
        boxCollider.size = originalColliderSize;
        boxCollider.offset = originalColliderOffset;
        rb.linearVelocity = Vector2.zero;
        isRolling = false;
    }

    public void Die()
    {
        isDead = true;
        TriggerAnimation("Die");
        rb.linearVelocity = Vector2.zero;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground"))
            isGrounded = true;
    }

    void OnCollisionExit2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground"))
            isGrounded = false;
    }

    void TryAttachToRope()
    {
        var cols = Physics2D.OverlapCircleAll(ropeDetectionPoint.position, detectionRadius);
        foreach (var col in cols)
        {
            if (col.CompareTag("Rope"))
            {
                var rb2 = col.GetComponent<Rigidbody2D>();
                if (rb2 != null) { AttachToRope(rb2); break; }
            }
        }
    }

    void AttachToRope(Rigidbody2D ropeSegment)
    {
        isSwinging = true;
        canAttach = false;
        connectedRopeSegment = ropeSegment;
        connectedRopeSegment.linearVelocity = lastVelocity * swingForceMultiplier;

        ropeHinge.connectedBody = ropeSegment;
        ropeHinge.autoConfigureConnectedAnchor = false;
        ropeHinge.anchor = transform.InverseTransformPoint(ropeDetectionPoint.position);
        ropeHinge.connectedAnchor = Vector2.zero;
        ropeHinge.enabled = true;
    }

    void DetachFromRope()
    {
        isSwinging = false;
        ropeHinge.enabled = false;
        canAttach = true;
        rb.linearVelocity = connectedRopeSegment.linearVelocity;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y + releaseJumpBoost);
    }
}
