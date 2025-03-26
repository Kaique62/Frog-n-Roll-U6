using UnityEngine;
using System.Collections;
using System;
using Unity.VisualScripting;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float rollDistance = 3f;
    public float rollDuration = 0.5f;
    public float attackDuration = 0.3f;
    public float stompFallSpeed = 15f;
    public float uppercutJumpForce = 7f;
    public float coyoteTime = 0.15f;  // Tempo que o jogador ainda pode pular depois de sair do chão
    public float jumpBufferTime = 0.1f;  // Tempo que o input de pulo fica "armazenado" antes de tocar no chão
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private bool wasGroundedLastFrame;
    private Rigidbody2D rb;
    private Animator animator;
    private bool isGrounded;
    private bool isRolling;
    private bool isAttacking;
    private bool isCrouching;
    private bool isDead;

    // Hitboxes
    public GameObject punchHitbox;
    public GameObject kickHitbox;
    public GameObject uppercutHitbox;
    public GameObject stompHitbox;

    // Colisor original
    private Vector2 originalColliderSize;
    private Vector2 originalColliderOffset;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        punchHitbox.SetActive(false);
        kickHitbox.SetActive(false);
        uppercutHitbox.SetActive(false);
        stompHitbox.SetActive(false);

        // Salvar o tamanho e offset original do colisor
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        originalColliderSize = collider.size;
        originalColliderOffset = collider.offset;
    }

    void Update()
    {
        if (isDead) return;

        HandleCoyoteTime();
        HandleCrouch();
        HandleMovement();
        HandleJump();
        HandleAttack();
        HandleRoll();
    }

    void HandleMovement()
    {
        if (isRolling || isAttacking) return;

        float moveInput = 0f;
        if (Input.GetKey(KeyCode.A)) // Mover para a esquerda
        {
            moveInput = -1f;
        }
        else if (Input.GetKey(KeyCode.D)) // Mover para a direita
        {
            moveInput = 1f;
        }

        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        animator.SetBool("isRunning", moveInput != 0);

        // Flip do sprite
        if (moveInput > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (moveInput < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    void HandleJump()
    {
        // Verifica se pode pular usando Coyote Time e Jump Buffer
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            if (isCrouching) ExitCrouch();
            
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            animator.SetTrigger("Jump");
            
            // Reseta os contadores
            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;
        }
    }

    void HandleCrouch()
    {
        if (isRolling || !isGrounded) return;

        if (Input.GetKey(KeyCode.S))
        {
            if (!isCrouching)
            {
                isCrouching = true;
                animator.SetBool("isCrouching", true);

                BoxCollider2D collider = GetComponent<BoxCollider2D>();
                collider.size = new Vector2(originalColliderSize.x, originalColliderSize.y / 2);
                collider.offset = new Vector2(originalColliderOffset.x, originalColliderOffset.y - originalColliderSize.y / 4);
            }
        }
        else if (isCrouching)
        {
            ExitCrouch();
        }
    }

    void HandleAttack()
    {
        if (isAttacking) return;

        if (isCrouching) ExitCrouch();

        // Soco para cima (I)
        if (Input.GetKeyDown(KeyCode.I))
        {
            StartCoroutine(PerformUppercut());
            animator.SetTrigger("Uppercut");
        }
        // Chute para baixo (S + L no ar)
        else if (!isGrounded && Input.GetKeyDown(KeyCode.L) && Input.GetKey(KeyCode.S)  )
        {
            StartCoroutine(PerformStomp());
            animator.SetTrigger("Stomp");
        }
        // Soco normal (J)
        else if (Input.GetKeyDown(KeyCode.J))
        {
            StartCoroutine(PerformAttack(punchHitbox));
            animator.SetTrigger("Punch");
        }
        // Chute normal (L)
        else if (Input.GetKeyDown(KeyCode.L))
        {
            StartCoroutine(PerformAttack(kickHitbox));
            animator.SetTrigger("Kick");
        }
    }

    void HandleRoll()
    {
        if (!isGrounded) return;

        if (Input.GetKeyDown(KeyCode.K))
        {
            if (isCrouching) ExitCrouch();
            StartCoroutine(PerformRoll());
            animator.SetTrigger("Roll");
        }
    }

    void ExitCrouch()
    {
        isCrouching = false;
        animator.SetBool("isCrouching", false);
        
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        collider.size = originalColliderSize;
        collider.offset = originalColliderOffset;
    }

    void HandleCoyoteTime()
    {
        // Atualiza o Coyote Time
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        // Atualiza o Jump Buffer
        if (Input.GetKeyDown(KeyCode.W))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        wasGroundedLastFrame = isGrounded;
    }

    IEnumerator PerformAttack(GameObject hitbox)
    {
        isAttacking = true;
        hitbox.SetActive(true);
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

        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        collider.size = new Vector2(originalColliderSize.x, originalColliderSize.y / 2);
        collider.offset = new Vector2(originalColliderOffset.x, originalColliderOffset.y - originalColliderSize.y / 4);

        float rollSpeed = rollDistance / rollDuration;
        rb.linearVelocity = new Vector2(transform.localScale.x * rollSpeed, rb.linearVelocity.y);

        yield return new WaitForSeconds(rollDuration);

        collider.size = originalColliderSize;
        collider.offset = originalColliderOffset;

        rb.linearVelocity = Vector2.zero;
        isRolling = false;
    }

    public void Die()
    {
        isDead = true;
        animator.SetTrigger("Die");
        rb.linearVelocity = Vector2.zero;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
        
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("derrota");
        }
    }


    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}