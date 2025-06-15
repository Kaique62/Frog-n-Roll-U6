using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public enum PlayerState
    {
        Free,          // Default state - can move freely
        Attacking,     // Performing any attack
        Rolling,       // Performing roll
        Stomping,      // Performing stomp attack
        Swinging,      // Attached to rope
        Dead           // Player is dead
    }

    [Header("Game Over")]
    public GameObject musicTimer;
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
    public float attackCancelWindow = 0.2f; // Time when attack can be canceled

    [Header("Variable Jump Settings")]
    public float minJumpHeight = 1.5f;
    public float maxJumpHeight = 3.5f;
    public float jumpApexThreshold = 0.1f;

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

    // Components
    private Rigidbody2D rb;
    private Animator animator;
    private BoxCollider2D boxCollider;

    // State management
    public PlayerState currentState { get; private set; } = PlayerState.Free;
    private Vector2 originalColliderSize;
    private Vector2 originalColliderOffset;
    private Vector2 lastVelocity;

    // Rope mechanics
    private HingeJoint2D ropeHinge;
    private Rigidbody2D connectedRopeSegment;
    private bool canAttach = true;

    // Ground/jump mechanics
    private bool isGrounded;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    
    // Variable jump
    private bool isJumpHeld;
    private bool isJumping;
    private float jumpStartY;
    private float currentJumpHeight;

    // Action management
    private Coroutine activeActionRoutine;
    private bool canCancelAttack;
    private float attackCancelTimer;
    private bool isStomping;
    

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();

        // Store original collider properties
        originalColliderSize = boxCollider.size;
        originalColliderOffset = boxCollider.offset;

        // Initialize hitboxes
        punchHitbox.SetActive(false);
        kickHitbox.SetActive(false);
        uppercutHitbox.SetActive(false);
        stompHitbox.SetActive(false);

        // Setup rope hinge
        ropeHinge = gameObject.AddComponent<HingeJoint2D>();
        ropeHinge.enabled = false;
    }

    public void Die()
    {
        if (currentState == PlayerState.Dead) return;
        
        currentState = PlayerState.Dead;
        rb.isKinematic = true;
        rb.linearVelocity = Vector2.zero;
        GetComponent<Collider2D>().enabled = false;
        animator.SetTrigger("Die");

        // Game over UI handling
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
                Debug.LogError("GameOverManager script not found!");
            }
        }
        else
        {
            Debug.LogError("Game Over prefab or Canvas Transform not assigned!");
        }
    }

    void Update()
    {
        if (currentState == PlayerState.Dead) return;
            
        // Track jump input state
        isJumpHeld = Input.GetKey(Controls.Jump) || MobileInput.GetHeld("Up");
        
        if (currentState == PlayerState.Swinging)
        {
            if (Input.GetKeyDown(Controls.Jump) || MobileInput.GetHeld("Up"))
                DetachFromRope();
            return;
        }
        
        if ((Input.GetKeyDown(Controls.Grab) || MobileInput.GetHeld("Grab")) && canAttach)
            TryAttachToRope();
            
        HandleTimers(); 
        
        // Handle state transitions and input processing
        switch (currentState)
        {
            case PlayerState.Free:
                HandleMovement();
                HandleJump();
                HandleRoll();
                HandleAttack();
                break;
                
            case PlayerState.Attacking:
                // Allow movement during attacks
                HandleMovement();
                
                // Allow attack cancellation during cancel window
                if (canCancelAttack)
                {
                    if (Input.GetKeyDown(Controls.Roll) || MobileInput.GetHeld("Roll"))
                    {
                        CancelCurrentAction();
                        StartCoroutine(PerformRoll());
                    }
                    else if (Input.GetKeyDown(Controls.Jump) || MobileInput.GetHeld("Up"))
                    {
                        CancelCurrentAction();
                        HandleJump();
                    }
                    else if (Input.GetKeyDown(Controls.Punch) || MobileInput.GetHeld("Punch") || 
                             Input.GetKeyDown(Controls.Kick) || MobileInput.GetHeld("Kick"))
                    {
                        CancelCurrentAction();
                        HandleAttack();
                    }
                }
                break;
                
            case PlayerState.Rolling:
                // Can't cancel rolls once started
                break;
                
            case PlayerState.Stomping:
                // Allow horizontal movement during stomp
                HandleMovement();
                
                // Apply downward velocity
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -stompFallSpeed);
                
                // Check for jump cancellation
                if (Input.GetKeyDown(Controls.Jump) || MobileInput.GetHeld("Up"))
                {
                    CancelCurrentAction();
                    HandleJump();
                }
                break;
        }
        
        // Handle variable jump physics
        HandleJumpPhysics();
    }

    private void ChangeState(PlayerState newState)
    {
        // Exit current state
        switch (currentState)
        {
            case PlayerState.Attacking:
                // Clean up attack state
                punchHitbox.SetActive(false);
                kickHitbox.SetActive(false);
                uppercutHitbox.SetActive(false);
                break;
                
            case PlayerState.Rolling:
                animator.SetBool("isRolling", false); // Ensure it's reset when changing state
                boxCollider.size = originalColliderSize;
                boxCollider.offset = originalColliderOffset;
                break;

            case PlayerState.Stomping:
                stompHitbox.SetActive(false);
                isStomping = false;
                break;
        }
        
        // Enter new state
        switch (newState)
        {
            case PlayerState.Free:
                // Reset triggers and booleans
                animator.ResetTrigger("Punch");
                animator.ResetTrigger("Kick");
                animator.ResetTrigger("Uppercut");
                animator.ResetTrigger("Roll");
                animator.ResetTrigger("Stomp");
                animator.ResetTrigger("Jump");
                break;
                
            case PlayerState.Rolling:
                // Initialize roll state
                boxCollider.size = new Vector2(originalColliderSize.x, originalColliderSize.y / 2);
                boxCollider.offset = new Vector2(
                    originalColliderOffset.x, 
                    originalColliderOffset.y - originalColliderSize.y / 4
                );
                break;
                
            case PlayerState.Stomping:
                isStomping = true;
                animator.SetTrigger("Stomp");
                break;
        }
        
        currentState = newState;
    }

    private void HandleMovement()
    {
        float moveInput = (Input.GetKey(Controls.Right) || MobileInput.GetHeld("Right")) ? 1f :
                          (Input.GetKey(Controls.Left) || MobileInput.GetHeld("Left")) ? -1f : 0f;
                          
        Vector2 velocity = rb.linearVelocity;
        velocity.x = moveInput * moveSpeed;
        rb.linearVelocity = velocity;
        
        // Update animator parameters
        animator.SetBool("isRunning", moveInput != 0 && isGrounded);
        animator.SetBool("isGrounded", isGrounded);
        
        if (moveInput != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(moveInput), 1, 1);
        }
        
        lastVelocity = rb.linearVelocity;
    }

    private void HandleJump()
    {
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            animator.SetTrigger("Jump");
            
            // Initiate variable jump
            isJumping = true;
            jumpStartY = transform.position.y;
            currentJumpHeight = minJumpHeight;
            
            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;
        }
    }

    private void HandleJumpPhysics()
    {
        if (!isJumping) return;

        float jumpProgress = transform.position.y - jumpStartY;
        
        if (isJumpHeld && jumpProgress < maxJumpHeight)
        {
            currentJumpHeight = Mathf.Lerp(minJumpHeight, maxJumpHeight, 
                                          jumpProgress / maxJumpHeight);
        }
        
        if (jumpProgress >= currentJumpHeight || !isJumpHeld)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Min(rb.linearVelocity.y, 0));
            isJumping = false;
        }
        
        if (rb.linearVelocity.y <= 0)
        {
            isJumping = false;
        }
    }

    private void HandleRoll()
    {
        if (isGrounded && (Input.GetKeyDown(Controls.Roll) || MobileInput.GetHeld("Roll")))
        {
            StartCoroutine(PerformRoll());
        }
    }

    private void HandleAttack()
    {
        if ((Input.GetKeyDown(Controls.Punch) || MobileInput.GetHeld("Punch")) && 
            (Input.GetKey(Controls.Up) || MobileInput.GetHeld("Up")))
        {
            StartCoroutine(PerformAttack(uppercutHitbox, "Uppercut"));
        }
        else if (!isGrounded && 
                (Input.GetKeyDown(Controls.Kick) || MobileInput.GetHeld("Kick")) && 
                (Input.GetKey(Controls.Down) || MobileInput.GetHeld("Down")))
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

    private void HandleTimers()
    {
        // Coyote time and jump buffer
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
            
        if (Input.GetKeyDown(Controls.Jump) || MobileInput.GetHeld("Up"))
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;
            
        // Attack cancel timer
        if (canCancelAttack)
        {
            attackCancelTimer -= Time.deltaTime;
            if (attackCancelTimer <= 0)
                canCancelAttack = false;
        }
    }

    IEnumerator PerformAttack(GameObject hitbox, string triggerName)
    {
        // Log timing and start attack
        musicTimer.GetComponent<MusicTimer>().DebugTime();
        ChangeState(PlayerState.Attacking);
        animator.SetTrigger(triggerName);
        hitbox.SetActive(true);
        
        // Detect hits
        Collider2D[] hits = Physics2D.OverlapBoxAll(
            hitbox.transform.position, 
            hitbox.GetComponent<BoxCollider2D>().size, 
            0
        );
        
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
        
        // Enable attack cancellation window
        canCancelAttack = true;
        attackCancelTimer = attackCancelWindow;
        
        // Store active routine
        activeActionRoutine = StartCoroutine(FinishAttack(hitbox));
        
        yield return null;
    }
    
    IEnumerator FinishAttack(GameObject hitbox)
    {
        yield return new WaitForSeconds(attackDuration);
        
        if (currentState == PlayerState.Attacking)
        {
            hitbox.SetActive(false);
            ChangeState(PlayerState.Free);
        }
    }

    private void CancelCurrentAction()
    {
        if (activeActionRoutine != null)
        {
            StopCoroutine(activeActionRoutine);
        }
        
        // Clean up based on current state
        switch (currentState)
        {
            case PlayerState.Attacking:
                punchHitbox.SetActive(false);
                kickHitbox.SetActive(false);
                uppercutHitbox.SetActive(false);
                break;
                
            case PlayerState.Rolling:
                break;
                
            case PlayerState.Stomping:
                stompHitbox.SetActive(false);
                isStomping = false;
                break;
        }
        
        // Reset state to free to allow new actions
        ChangeState(PlayerState.Free);
    }

    IEnumerator PerformStomp()
    {
        ChangeState(PlayerState.Stomping);
        stompHitbox.SetActive(true);
        
        // Stomp continues until we hit the ground
        while (!isGrounded)
        {
            yield return null;
        }
        
        // Ground hit - end stomp
        stompHitbox.SetActive(false);
        ChangeState(PlayerState.Free);
    }

    IEnumerator PerformRoll()
    {
        ChangeState(PlayerState.Rolling);

        animator.SetBool("isRolling", true); // Set rolling animation active

        float speed = rollDistance / rollDuration;
        rb.linearVelocity = new Vector2(transform.localScale.x * speed, rb.linearVelocity.y);

        yield return new WaitForSeconds(rollDuration);

        rb.linearVelocity = Vector2.zero;
        animator.SetBool("isRolling", false); // Stop roll animation
        ChangeState(PlayerState.Free);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;

            // End stomp immediately when hitting ground
            if (isStomping)
            {
                stompHitbox.SetActive(false);
                ChangeState(PlayerState.Free);
            }
        }
            
        if (col.gameObject.CompareTag("FireBorder"))
        {
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
        Collider2D[] cols = Physics2D.OverlapCircleAll(
            ropeDetectionPoint.position, 
            detectionRadius
        );
        
        foreach (var col in cols)
        {
            if (col.CompareTag("Rope"))
            {
                Rigidbody2D rb2 = col.GetComponent<Rigidbody2D>();
                if (rb2 != null)
                {
                    AttachToRope(rb2);
                    break;
                }
            }
        }
    }

    void AttachToRope(Rigidbody2D ropeSegment)
    {
        ChangeState(PlayerState.Swinging);
        canAttach = false;
        connectedRopeSegment = ropeSegment;
        connectedRopeSegment.linearVelocity = lastVelocity * swingForceMultiplier;
        
        ropeHinge.connectedBody = ropeSegment;
        ropeHinge.autoConfigureConnectedAnchor = false;
        ropeHinge.anchor = transform.InverseTransformPoint(ropeDetectionPoint.position);
        ropeHinge.connectedAnchor = Vector2.zero;
        ropeHinge.enabled = true;
        
        if (isStomping)
        {
            stompHitbox.SetActive(false);
            ChangeState(PlayerState.Free);
        }

        // Reset jump state
        isJumping = false;
    }

    void DetachFromRope()
    {
        ChangeState(PlayerState.Free);
        ropeHinge.enabled = false;
        canAttach = true;
        
        rb.linearVelocity = connectedRopeSegment.linearVelocity;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y + releaseJumpBoost);
    }
}