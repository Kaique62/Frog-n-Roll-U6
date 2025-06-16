using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{

    public enum PlayerState
    {
        Free,       // Default state - can move freely
        Attacking,  // Performing any attack
        Rolling,    // Performing roll
        Stomping,   // Performing stomp attack
        Swinging,   // Attached to rope
        Dead        // Player is dead
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

    // State Management
    public PlayerState currentState { get; private set; } = PlayerState.Free;
    private Vector2 originalColliderSize;
    private Vector2 originalColliderOffset;
    private Vector2 lastVelocity;

    // Rope Mechanics
    private HingeJoint2D ropeHinge;
    private Rigidbody2D connectedRopeSegment;
    private bool canAttach = true;

    // Ground & Jump Mechanics
    private bool isGrounded;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    
    // Variable Jump
    private bool isJumpHeld;
    private bool isJumping;
    private float jumpStartY;
    private float currentJumpHeight;

    // Action Management
    private Coroutine activeActionRoutine;
    private bool canCancelAttack;
    private float attackCancelTimer;
    private bool isStomping; // Specific flag needed within Stomping state
    private float airTime = 0f;
    private bool wasGrounded = true;


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

    void Update()
    {
        if (currentState == PlayerState.Dead) return;

        if (!isGrounded)
        {
            airTime += Time.deltaTime;
            if (wasGrounded && airTime >= 0.1f)
            {
                animator.SetTrigger("Jump");
            }
        }
        else
        {
            animator.SetBool("isStomping", false);
            isStomping = false;
            airTime = 0f;
            wasGrounded = true;
        }

        // Track jump input state
        isJumpHeld = Input.GetKey(Controls.Jump) || MobileInput.GetHeld("Up");
        
        // Handle rope detachment separately as it overrides other states
        if (currentState == PlayerState.Swinging)
        {
            if (Input.GetKeyDown(Controls.Jump) || MobileInput.GetHeld("Up"))
            {
                DetachFromRope();
            }
            return;
        }

        // Handle rope attachment
        if ((Input.GetKeyDown(Controls.Grab) || MobileInput.GetHeld("Grab")) && canAttach)
        {
            TryAttachToRope();
        }
            
        HandleTimers(); 
        
        // Handle state transitions and input processing based on the current state
        switch (currentState)
        {
            case PlayerState.Free:
                HandleMovement();
                HandleJump();
                HandleRoll();
                HandleAttack();
                break;
                
            case PlayerState.Attacking:
                HandleMovement(); // Allow movement during attacks
                HandleAttackCancellation(); // Check if the player wants to cancel the attack
                break;
                
            case PlayerState.Rolling:
                // No actions can be taken while rolling
                break;
                
            case PlayerState.Stomping:
                HandleMovement(); // Allow horizontal movement during stomp
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -stompFallSpeed); // Apply downward velocity
                
                // Check for jump cancellation
                if (Input.GetKeyDown(Controls.Jump) || MobileInput.GetHeld("Up"))
                {
                    CancelCurrentAction();
                    HandleJump();
                }
                break;
        }
        
        HandleJumpPhysics();
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            wasGrounded = true;

            // End stomp immediately when hitting ground
            if (currentState == PlayerState.Stomping)
            {
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
        {
            isGrounded = false;
            wasGrounded = false;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Only deal damage if the player is in the Stomping state and the hitbox is active
        if (currentState == PlayerState.Stomping && stompHitbox.activeInHierarchy)
        {
            if (other.CompareTag("Enemy"))
            {
                var timer = other.GetComponent<ActionTimer>();
                if (timer != null)
                {
                    float currentTime = FindObjectOfType<MusicTimer>().CurrentTime;
                    FindObjectOfType<LevelController>().AddScore(currentTime, other.gameObject.GetComponent<ActionTimer>());   
                }

                var enemy = other.GetComponent<EnemyController>();
                if (enemy != null)
                    enemy.TakeDamage();
            }
        }
    }

    private void ChangeState(PlayerState newState)
    {
        // Exit logic for the current state
        switch (currentState)
        {
            case PlayerState.Attacking:
                punchHitbox.SetActive(false);
                kickHitbox.SetActive(false);
                uppercutHitbox.SetActive(false);
                break;
                
            case PlayerState.Rolling:
                animator.SetBool("isRolling", false); // Ensure animation is reset
                boxCollider.size = originalColliderSize;
                boxCollider.offset = originalColliderOffset;
                break;

            case PlayerState.Stomping:
                isStomping = true;
                animator.SetBool("isStomping", true); // Changed to boolean
                break;
        }
        
        // Set the new state
        currentState = newState;
        
        // Enter logic for the new state
        switch (newState)
        {
            case PlayerState.Free:
                animator.ResetTrigger("Punch");
                animator.ResetTrigger("Kick");
                animator.ResetTrigger("Uppercut");
                animator.ResetTrigger("Roll");
                animator.ResetTrigger("Stomp");
                animator.ResetTrigger("Jump");
                break;
                
            case PlayerState.Rolling:
                // Shrink collider for the roll
                boxCollider.size = new Vector2(originalColliderSize.x, originalColliderSize.y / 2);
                boxCollider.offset = new Vector2(originalColliderOffset.x, originalColliderOffset.y - originalColliderSize.y / 4);
                break;
                
            case PlayerState.Stomping:
                isStomping = true;
                animator.SetTrigger("Stomp");
                break;
        }
    }

    public void Die()
    {
        if (currentState == PlayerState.Dead) return;
        
        ChangeState(PlayerState.Dead);
        
        rb.isKinematic = true;
        rb.linearVelocity = Vector2.zero;
        GetComponent<Collider2D>().enabled = false;
        animator.SetTrigger("Die");

        // Game over UI handling
        if (gameOverPrefab != null && canvasTransform != null)
        {
            GameObject gameOverInstance = Instantiate(gameOverPrefab, canvasTransform);
            gameOverInstance.SetActive(true); 

            GameOverManager gameOverManager = gameOverInstance.GetComponentInChildren<GameOverManager>();
            if (gameOverManager != null)
            {
                gameOverManager.ShowGameOver();
            }
            else
            {
                Debug.LogError("GameOverManager script not found on the GameOver prefab!");
            }
        }
        else
        {
            Debug.LogError("Game Over prefab or Canvas Transform not assigned in the Inspector!");
        }
    }

    private void CancelCurrentAction()
    {
        if (activeActionRoutine != null)
        {
            StopCoroutine(activeActionRoutine);
            activeActionRoutine = null;
        }
        
        // Reset state to free to allow new actions, ChangeState handles cleanup
        ChangeState(PlayerState.Free);
    }

    private void HandleMovement()
    {
        float moveInput = (Input.GetKey(Controls.Right) || MobileInput.GetHeld("Right")) ? 1f :
                          (Input.GetKey(Controls.Left) || MobileInput.GetHeld("Left")) ? -1f : 0f;
                          
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        
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

            // Initiate variable jump tracking
            isJumping = true;
            jumpStartY = transform.position.y;
            currentJumpHeight = minJumpHeight;

            jumpBufferCounter = 0f; // Consume the buffer
            coyoteTimeCounter = 0f; // Consume coyote time
            
            if (currentState != PlayerState.Stomping)
            {
                animator.SetTrigger("Jump");
            }
        }
    }
    
    private void HandleRoll()
    {
        if (isGrounded && (Input.GetKeyDown(Controls.Roll) || MobileInput.GetHeld("Roll")))
        {
            activeActionRoutine = StartCoroutine(PerformRoll());
        }
    }

    private void HandleAttack()
    {
        if ((Input.GetKeyDown(Controls.Punch) || MobileInput.GetHeld("Punch")) && (Input.GetKey(Controls.Up) || MobileInput.GetHeld("Up")))
        {
            activeActionRoutine = StartCoroutine(PerformAttack(uppercutHitbox, "Uppercut"));
        }
        else if (!isGrounded && (Input.GetKeyDown(Controls.Kick) || MobileInput.GetHeld("Kick")) && (Input.GetKey(Controls.Down) || MobileInput.GetHeld("Down")))
        {
            activeActionRoutine = StartCoroutine(PerformStomp());
        }
        else if (Input.GetKeyDown(Controls.Punch) || MobileInput.GetHeld("Punch"))
        {
            activeActionRoutine = StartCoroutine(PerformAttack(punchHitbox, "Punch"));
        }
        else if (Input.GetKeyDown(Controls.Kick) || MobileInput.GetHeld("Kick"))
        {
            activeActionRoutine = StartCoroutine(PerformAttack(kickHitbox, "Kick"));
        }
    }

    private void HandleAttackCancellation()
    {
        if (!canCancelAttack) return;
        
        // Cancel into a Roll
        if (Input.GetKeyDown(Controls.Roll) || MobileInput.GetHeld("Roll"))
        {
            CancelCurrentAction();
            activeActionRoutine = StartCoroutine(PerformRoll());
        }
        // Cancel into a Jump
        else if (Input.GetKeyDown(Controls.Jump) || MobileInput.GetHeld("Up"))
        {
            CancelCurrentAction();
            HandleJump();
        }
        // Cancel into another Attack (combo)
        else if (Input.GetKeyDown(Controls.Punch) || MobileInput.GetHeld("Punch") || Input.GetKeyDown(Controls.Kick) || MobileInput.GetHeld("Kick"))
        {
            CancelCurrentAction();
            HandleAttack();
        }
    }

    private void HandleTimers()
    {
        // Coyote time
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
            
        // Jump buffer
        if (Input.GetKeyDown(Controls.Jump) || MobileInput.GetHeld("Up"))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }
            
        // Attack cancel timer
        if (canCancelAttack)
        {
            attackCancelTimer -= Time.deltaTime;
            if (attackCancelTimer <= 0)
            {
                canCancelAttack = false;
            }
        }
    }

    IEnumerator PerformAttack(GameObject hitbox, string triggerName)
    {
        ChangeState(PlayerState.Attacking);
        animator.SetTrigger(triggerName);
        hitbox.SetActive(true);
        
        // Log timing and detect hits
        musicTimer.GetComponent<MusicTimer>().DebugTime();
        Collider2D[] hits = Physics2D.OverlapBoxAll(hitbox.transform.position, hitbox.GetComponent<BoxCollider2D>().size, 0);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                var timer = hit.GetComponent<ActionTimer>();
                if (timer != null)
                {
                    float currentTime = FindObjectOfType<MusicTimer>().CurrentTime;
                    FindObjectOfType<LevelController>().AddScore(currentTime, hit.gameObject.GetComponent<ActionTimer>());

                }
                hit.GetComponent<EnemyController>()?.TakeDamage();
            }
        }
        
        // Enable attack cancellation window
        canCancelAttack = true;
        attackCancelTimer = attackCancelWindow;
        
        // Wait for the attack duration to finish
        yield return new WaitForSeconds(attackDuration);
        
        // If the attack wasn't canceled, return to Free state
        if (currentState == PlayerState.Attacking)
        {
            ChangeState(PlayerState.Free);
        }
    }

    IEnumerator PerformRoll()
    {
        ChangeState(PlayerState.Rolling);
        animator.SetBool("isRolling", true);

        float speed = rollDistance / rollDuration;
        rb.linearVelocity = new Vector2(transform.localScale.x * speed, rb.linearVelocity.y);

        yield return new WaitForSeconds(rollDuration);

        // If the roll wasn't interrupted, return to Free state
        if (currentState == PlayerState.Rolling)
        {
            rb.linearVelocity = Vector2.zero;
            ChangeState(PlayerState.Free);
        }
    }

    IEnumerator PerformStomp()
    {
        ChangeState(PlayerState.Stomping);
        stompHitbox.SetActive(true);
        
        // Immediately show stomp animation
        animator.SetBool("isStomping", true);
        
        yield return null;
    }

    private void HandleJumpPhysics()
    {
        if (!isJumping) return;

        float jumpProgress = transform.position.y - jumpStartY;
        
        // Allow for variable jump height by holding the jump button
        if (isJumpHeld && jumpProgress < maxJumpHeight)
        {
            currentJumpHeight = Mathf.Lerp(minJumpHeight, maxJumpHeight, jumpProgress / maxJumpHeight);
        }
        
        // Cut the jump short if the button is released or max height is reached
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
    
    void TryAttachToRope()
    {
        Collider2D[] cols = Physics2D.OverlapCircleAll(ropeDetectionPoint.position, detectionRadius);
        foreach (var col in cols)
        {
            if (col.CompareTag("Rope"))
            {
                Rigidbody2D ropeRB = col.GetComponent<Rigidbody2D>();
                if (ropeRB != null)
                {
                    AttachToRope(ropeRB);
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
        
        // Transfer player's momentum to the rope
        connectedRopeSegment.linearVelocity = lastVelocity * swingForceMultiplier;
        
        // Configure and enable the HingeJoint2D
        ropeHinge.connectedBody = ropeSegment;
        ropeHinge.autoConfigureConnectedAnchor = false;
        ropeHinge.anchor = transform.InverseTransformPoint(ropeDetectionPoint.position);
        ropeHinge.connectedAnchor = Vector2.zero;
        ropeHinge.enabled = true;
        
        // Reset jump state
        isJumping = false;
    }

    void DetachFromRope()
    {
        ChangeState(PlayerState.Free);
        ropeHinge.enabled = false;
        canAttach = true;
        
        // Apply velocity from the rope plus a jump boost
        rb.linearVelocity = connectedRopeSegment.linearVelocity;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y + releaseJumpBoost);
    }
}