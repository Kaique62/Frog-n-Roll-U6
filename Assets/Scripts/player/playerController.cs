using UnityEngine;
using System.Collections;
using System.Threading.Tasks;

public class PlayerController : MonoBehaviour
{
    [Header("Game Over")]
    /// <summary>
    /// Prefab to instantiate when the player dies and the game is over.
    /// </summary>
    public GameObject gameOverPrefab;

    /// <summary>
    /// Parent transform (usually a UI Canvas) where the Game Over prefab will be instantiated.
    /// </summary>
    public Transform canvasTransform;

    [Header("Player Movement")]
    /// <summary>
    /// Horizontal movement speed of the player.
    /// </summary>
    public float moveSpeed = 5f;

    /// <summary>
    /// Upward force applied when the player jumps.
    /// </summary>
    public float jumpForce = 10f;

    /// <summary>
    /// Distance covered during a roll action.
    /// </summary>
    public float rollDistance = 3f;

    /// <summary>
    /// Duration of the roll animation/action.
    /// </summary>
    public float rollDuration = 0.5f;

    /// <summary>
    /// Duration of an attack animation/action.
    /// </summary>
    public float attackDuration = 0.3f;

    /// <summary>
    /// Downward speed applied during a stomp attack.
    /// </summary>
    public float stompFallSpeed = 15f;

    /// <summary>
    /// Upward force applied during an uppercut jump.
    /// </summary>
    public float uppercutJumpForce = 7f;

    /// <summary>
    /// Duration in seconds that the player can still jump after leaving the ground (coyote time).
    /// </summary>
    public float coyoteTime = 0.15f;

    /// <summary>
    /// Duration in seconds during which a jump input is buffered and will be executed as soon as possible.
    /// </summary>
    public float jumpBufferTime = 0.1f;

    [Header("Rope Swing Settings")]
    /// <summary>
    /// Transform point used to detect nearby ropes to attach to.
    /// </summary>
    public Transform ropeDetectionPoint;

    /// <summary>
    /// Radius around the rope detection point to search for rope objects.
    /// </summary>
    public float detectionRadius = 0.5f;

    /// <summary>
    /// Multiplier applied to the player's velocity when swinging on a rope.
    /// </summary>
    public float swingForceMultiplier = 1.2f;

    /// <summary>
    /// Additional vertical velocity applied when the player releases the rope and jumps off.
    /// </summary>
    public float releaseJumpBoost = 5f;

    [Header("Hitboxes")]
    /// <summary>
    /// Hitbox game object used for punch attacks.
    /// </summary>
    public GameObject punchHitbox;

    /// <summary>
    /// Hitbox game object used for kick attacks.
    /// </summary>
    public GameObject kickHitbox;

    /// <summary>
    /// Hitbox game object used for uppercut attacks.
    /// </summary>
    public GameObject uppercutHitbox;

    /// <summary>
    /// Hitbox game object used for stomp attacks.
    /// </summary>
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

    /// <summary>
    /// Initialize component references and set up initial states.
    /// </summary>
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

    /// <summary>
    /// Handles player death: disables physics and collisions, plays death animation, and shows the Game Over UI.
    /// </summary>
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
                Debug.LogError("Game Over prefab or its children do not have a GameOverManager script!");
            }
        }
        else
        {
            Debug.LogError("Game Over prefab or Canvas Transform not assigned in the Player inspector!");
        }
    }

    /// <summary>
    /// Called every frame to process input and update the player state.
    /// </summary>
    [System.Obsolete]
    void Update()
    {
        if (boxCollider.gameObject.CompareTag("FireBorder"))
            Debug.Log("died");

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
            PlayAnimation("Jump");
    }

    /// <summary>
    /// Processes player input handlers.
    /// </summary>
    [System.Obsolete]
    private void HandleInputs()
    {
        HandleMovement();
        HandleJump();
        HandleRoll();
        HandleAttack();
    }

    /// <summary>
    /// Handles player horizontal movement input and applies velocity.
    /// </summary>
    private void HandleMovement()
    {
        if (isRolling || isAttacking || isSwinging) return;

        float moveInput = (Input.GetKey(Controls.Right) || MobileInput.GetHeld("Right")) ? 1f :
                          (Input.GetKey(Controls.Left) || MobileInput.GetHeld("Left")) ? -1f : 0f;

        Vector2 velocity = rb.linearVelocity;
        velocity.x = moveInput * moveSpeed;
        rb.linearVelocity = velocity;

        if (moveInput != 0)
        {
            if (isGrounded)
                PlayAnimation("Run");
            transform.localScale = new Vector3(Mathf.Sign(moveInput), 1, 1);
        }
        else
        {
            if (isGrounded && !isAttacking)
                PlayAnimation("Idle");
        }

        lastVelocity = rb.linearVelocity;
    }

    /// <summary>
    /// Handles jump input buffering and applies jump force if conditions are met.
    /// </summary>
    private void HandleJump()
    {
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            PlayAnimation("Jump");
            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;
        }
    }

    /// <summary>
    /// Handles the roll input and starts roll coroutine.
    /// </summary>
    private void HandleRoll()
    {
        if (isGrounded && (Input.GetKeyDown(Controls.Roll) || MobileInput.GetHeld("Roll")) && !isRolling)
        {
            StartCoroutine(PerformRoll());
        }
    }

    /// <summary>
    /// Handles attack inputs and triggers the appropriate attack coroutine.
    /// </summary>
    [System.Obsolete]
    private void HandleAttack()
    {
        if (isAttacking) return;

        if ((Input.GetKeyDown(Controls.Punch) || MobileInput.GetHeld("Punch")) &&
            (Input.GetKey(Controls.Up) || MobileInput.GetHeld("Up")))
        {
            if (Mathf.Abs(rb.linearVelocity.x) > 0.1f)
                StartCoroutine(PerformAttack(uppercutHitbox, "UpPunch-Run"));
            else
                StartCoroutine(PerformAttack(uppercutHitbox, "UpPunch"));
        }
        else if (!isGrounded &&
                 (Input.GetKeyDown(Controls.Kick) || MobileInput.GetHeld("Kick")) &&
                 (Input.GetKey(Controls.Down) || MobileInput.GetHeld("Down")))
        {
            StartCoroutine(PerformStomp());
        }
        else if (Input.GetKeyDown(Controls.Punch) || MobileInput.GetHeld("Punch"))
        {
            if (!isGrounded)
                StartCoroutine(PerformAttack(punchHitbox, "Punch-Jump"));
            else if (Mathf.Abs(rb.linearVelocity.x) > 0.1f)
                StartCoroutine(PerformAttack(punchHitbox, "Punch-Run"));
            else
                StartCoroutine(PerformAttack(punchHitbox, "Punch"));
        }
        else if (Input.GetKeyDown(Controls.Kick) || MobileInput.GetHeld("Kick"))
        {
            if (!isGrounded)
                StartCoroutine(PerformAttack(kickHitbox, "Kick-Jump"));
            else if (Mathf.Abs(rb.linearVelocity.x) > 0.1f)
                StartCoroutine(PerformAttack(kickHitbox, "Kick-Run"));
            else
                StartCoroutine(PerformAttack(kickHitbox, "Kick"));
        }
    }

    /// <summary>
    /// Updates timers for coyote time and jump buffering.
    /// </summary>
    private void HandleTimers()
    {
        if (isGrounded)
            coyoteTimeCounter = coyoteTime;
        else
            coyoteTimeCounter -= Time.deltaTime;

        if (Input.GetKeyDown(Controls.Jump) || MobileInput.GetHeld("Up"))
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;
    }

    /// <summary>
    /// Plays the specified animation if it is not already playing.
    /// </summary>
    /// <param name="animationName">The name of the animation to play.</param>
    private void PlayAnimation(string animationName)
    {
        if (currentAnimation == animationName) return;
        animator.Play(animationName);
        currentAnimation = animationName;
    }

    /// <summary>
    /// Coroutine to perform a roll movement.
    /// </summary>
    private IEnumerator PerformRoll()
    {
        isRolling = true;
        PlayAnimation("Roll");

        Vector2 direction = new Vector2(transform.localScale.x, 0);
        float distanceCovered = 0f;

        // Adjust collider for rolling stance
        boxCollider.size = new Vector2(originalColliderSize.x, originalColliderSize.y / 2);
        boxCollider.offset = new Vector2(originalColliderOffset.x, originalColliderOffset.y / 2);

        while (distanceCovered < rollDistance)
        {
            float moveStep = moveSpeed * Time.deltaTime * 2f; // Roll speed is faster
            rb.MovePosition(rb.position + direction * moveStep);
            distanceCovered += moveStep;
            yield return null;
        }

        // Restore collider size after roll
        boxCollider.size = originalColliderSize;
        boxCollider.offset = originalColliderOffset;

        isRolling = false;
    }

    /// <summary>
    /// Coroutine to perform an attack with a specific hitbox and animation.
    /// </summary>
    /// <param name="hitbox">The hitbox GameObject to activate during the attack.</param>
    /// <param name="animationName">The name of the animation to play during the attack.</param>
    private IEnumerator PerformAttack(GameObject hitbox, string animationName)
    {
        isAttacking = true;
        PlayAnimation(animationName);

        hitbox.SetActive(true);

        yield return new WaitForSeconds(attackDuration);

        hitbox.SetActive(false);

        isAttacking = false;
    }

    /// <summary>
    /// Coroutine to perform the stomp attack.
    /// </summary>
    private IEnumerator PerformStomp()
    {
        isAttacking = true;
        PlayAnimation("Stomp");
        stompHitbox.SetActive(true);

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, -stompFallSpeed);

        while (!isGrounded)
        {
            yield return null;
        }

        stompHitbox.SetActive(false);
        isAttacking = false;
    }

    /// <summary>
    /// Attempts to attach the player to a nearby rope.
    /// </summary>
    private void TryAttachToRope()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(ropeDetectionPoint.position, detectionRadius);

        foreach (var hit in hits)
        {
            if (hit.CompareTag("RopeSegment"))
            {
                ropeHinge.enabled = true;
                ropeHinge.connectedBody = hit.GetComponent<Rigidbody2D>();
                ropeHinge.anchor = transform.InverseTransformPoint(hit.transform.position);

                isSwinging = true;
                canAttach = false;

                rb.linearVelocity *= swingForceMultiplier;
                rb.gravityScale = 2f;

                connectedRopeSegment = ropeHinge.connectedBody;

                break;
            }
        }
    }

    /// <summary>
    /// Detaches the player from the rope, applies jump boost and resets physics.
    /// </summary>
    private void DetachFromRope()
    {
        isSwinging = false;
        ropeHinge.enabled = false;
        rb.gravityScale = 6f;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, releaseJumpBoost);

        connectedRopeSegment = null;

        StartCoroutine(EnableRopeAttachAfterDelay());
    }

    /// <summary>
    /// Prevents immediate reattachment to rope after detaching; adds a short cooldown.
    /// </summary>
    IEnumerator EnableRopeAttachAfterDelay()
    {
        yield return new WaitForSeconds(0.1f);
        canAttach = true;
    }

    /// <summary>
    /// FixedUpdate checks ground status using raycast.
    /// </summary>
    void FixedUpdate()
    {
        Vector2 origin = new Vector2(transform.position.x, transform.position.y - boxCollider.size.y / 2);
        isGrounded = Physics2D.Raycast(origin, Vector2.down, 0.1f, LayerMask.GetMask("Ground"));

        if (isGrounded && !isAttacking && !isRolling && !isSwinging)
        {
            if (Mathf.Abs(rb.linearVelocity.x) > 0.1f)
                PlayAnimation("Run");
            else
                PlayAnimation("Idle");
        }
    }
}
