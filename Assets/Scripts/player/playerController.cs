using UnityEngine;
using System.Collections;
using System.Threading.Tasks;

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

    void Update()
    {
        if (boxCollider.gameObject.CompareTag("FireBorder")) Debug.Log("died");
        if (isDead) return;

        if (isSwinging)
        {
            if (Input.GetKeyDown(Controls.Jump) || MobileInput.GetHeld("Up"))
                DetachFromRope();
            return;
        }

        if ((Input.GetKeyDown(Controls.Grab) || MobileInput.GetHeld("Grab")) && canAttach)
            TryAttachToRope();

        HandleTimers();
        HandleInputs();

        if (!isGrounded && !isRolling && !isAttacking)
        {
            PlayAnimation("Jump");
        }
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
        if (isRolling || isAttacking || isSwinging) return;

        float moveInput = (Input.GetKey(Controls.Right) || MobileInput.GetHeld("Right")) ? 1f : (Input.GetKey(Controls.Left) || MobileInput.GetHeld("Left")) ? -1f : 0f;
        Vector2 velocity = rb.linearVelocity;
        velocity.x = moveInput * moveSpeed;
        rb.linearVelocity = velocity;

        if (moveInput != 0)
        {
            if (isGrounded && !isCrouching)
                PlayAnimation("Run");

            transform.localScale = new Vector3(Mathf.Sign(moveInput), 1, 1);
        }
        else
        {
            if (isGrounded && !isCrouching && !isAttacking)
                PlayAnimation("Idle");
        }

        lastVelocity = rb.linearVelocity;
    }

    private void HandleJump()
    {
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            if (isCrouching) ExitCrouch();

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            PlayAnimation("Jump");

            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;
        }
    }

    private void HandleRoll()
    {
        if (isGrounded && (Input.GetKeyDown(Controls.Roll) || MobileInput.GetHeld("Roll")) && !isRolling)
        {
            if (isCrouching) ExitCrouch();
            StartCoroutine(PerformRoll());
        }
    }

    private void HandleAttack()
    {
        if (isAttacking) return;

        if (isCrouching) ExitCrouch();

        if (Input.GetKeyDown(Controls.Uppercut) || MobileInput.GetHeld("Uppercut"))
        {
            StartCoroutine(PerformUppercut());
        }
        else if (!isGrounded && (Input.GetKeyDown(Controls.Kick) || MobileInput.GetHeld("Kick")) && (Input.GetKey(Controls.Down) || MobileInput.GetHeld("Down")))
        {
            StartCoroutine(PerformStomp());
        }
        else if (Input.GetKeyDown(Controls.Punch) || MobileInput.GetHeld("Punch"))
        {
            StartCoroutine(PerformAttack(punchHitbox, "Punch"));
        }
        else if (Input.GetKeyDown(Controls.Kick) || MobileInput.GetHeld("Kick"))
        {
            StartCoroutine(PerformAttack(kickHitbox, "Kick"));
        }
    }

    private void HandleCrouch()
    {
        if (isRolling || isAttacking || !isGrounded) return;

        if (Input.GetKey(Controls.Down) || MobileInput.GetHeld("Down"))
        {
            if (!isCrouching)
            {
                isCrouching = true;
                boxCollider.size = new Vector2(originalColliderSize.x, originalColliderSize.y / 2);
                boxCollider.offset = new Vector2(originalColliderOffset.x, originalColliderOffset.y - originalColliderSize.y / 4);
                PlayAnimation("Crouch");
            }
        }
        else if (isCrouching)
        {
            ExitCrouch();
        }
    }

    private void HandleTimers()
    {
        if (isGrounded) coyoteTimeCounter = coyoteTime;
        else coyoteTimeCounter -= Time.deltaTime;

        if (Input.GetKeyDown(Controls.Jump) || MobileInput.GetHeld("Up"))
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;
    }

    private void ExitCrouch()
    {
        isCrouching = false;
        boxCollider.size = originalColliderSize;
        boxCollider.offset = originalColliderOffset;
        PlayAnimation("Idle");
    }

    private void PlayAnimation(string animationName)
    {
        if (currentAnimation == animationName) return;
        currentAnimation = animationName;
        animator.Play(animationName);
    }

    IEnumerator PerformAttack(GameObject hitbox, string animationName)
    {
        isAttacking = true;
        PlayAnimation(animationName);
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
        PlayAnimation("Punch");
        uppercutHitbox.SetActive(true);
        yield return new WaitForSeconds(attackDuration);
        uppercutHitbox.SetActive(false);
        isAttacking = false;
    }

    IEnumerator PerformStomp()
    {
        isAttacking = true;
        PlayAnimation("Stomp");
        stompHitbox.SetActive(true);
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, -stompFallSpeed);
        yield return new WaitForSeconds(attackDuration);
        stompHitbox.SetActive(false);
        isAttacking = false;
    }

    IEnumerator PerformRoll()
    {
        isRolling = true;
        PlayAnimation("Roll");

        boxCollider.size = new Vector2(originalColliderSize.x, originalColliderSize.y / 2);
        boxCollider.offset = new Vector2(originalColliderOffset.x, originalColliderOffset.y - originalColliderSize.y / 4);

        float speed = rollDistance / rollDuration;
        rb.linearVelocity = new Vector2(transform.localScale.x * speed, rb.linearVelocity.y);
        yield return new WaitForSeconds(rollDuration);

        boxCollider.size = originalColliderSize;
        boxCollider.offset = originalColliderOffset;
        rb.linearVelocity = Vector2.zero;
        isRolling = false;
    }

    public void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        PlayAnimation("Die");
        Destroy(gameObject);
        Time.timeScale = 0;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground"))
            isGrounded = true;
        if (col.gameObject.CompareTag("FireBorder"))
        {
            Debug.Log("died");
            Die();
        }
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
