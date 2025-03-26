using UnityEngine;

public class PlayerSwingHinge : MonoBehaviour
{
    [Header("Player Movement")]
    public float speed = 5f;
    public float jumpForce = 7f;
    private Rigidbody2D rb;
    private bool isGrounded;
    private Vector2 lastVelocity;

    [Header("Swinging Mechanics")]
    public float swingForceMultiplier = 1.2f; // Apenas para a força na corda
    public Transform ropeDetectionPoint; // Objeto externo para detecção
    public float detectionRadius = 0.5f;
    private HingeJoint2D hingeJoint;
    private bool isSwinging = false;
    private Rigidbody2D connectedRopeSegment;
    private bool canAttach = true;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        hingeJoint = gameObject.AddComponent<HingeJoint2D>();
        hingeJoint.enabled = false;
    }

    private void Update()
    {
        if (isSwinging)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                DetachFromRope();
            }
        }
        else
        {
            HandleMovement();

            if (Input.GetKeyDown(KeyCode.E) && canAttach)
            {
                TryAttachToRope();
            }
        }
    }

    private void HandleMovement()
    {
        float move = Input.GetAxis("Horizontal");
        rb.linearVelocity = new Vector2(move * speed, rb.linearVelocity.y);

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        lastVelocity = rb.linearVelocity;
    }

    private void TryAttachToRope()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(ropeDetectionPoint.position, detectionRadius);
        foreach (var collider in colliders)
        {
            if (collider.CompareTag("Rope"))
            {
                AttachToRope(collider.GetComponent<Rigidbody2D>());
                return;
            }
        }
    }

    private void AttachToRope(Rigidbody2D ropeSegment)
    {
        if (isSwinging) return;

        isSwinging = true;
        canAttach = false;
        connectedRopeSegment = ropeSegment;

        // Aplicar força na corda com o multiplicador
        connectedRopeSegment.linearVelocity = lastVelocity * swingForceMultiplier;

        // Conectar ao hinge joint sem modificar a posição do jogador
        hingeJoint.connectedBody = ropeSegment;
        hingeJoint.autoConfigureConnectedAnchor = false;
        hingeJoint.anchor = transform.InverseTransformPoint(ropeSegment.position); // Mantém a posição do jogador
        hingeJoint.connectedAnchor = Vector2.zero;
        hingeJoint.enabled = true;
    }

    private void DetachFromRope()
    {
        isSwinging = false;
        hingeJoint.enabled = false;
        canAttach = true;

        // Transferir momentum ao soltar sem afetar o salto
        rb.linearVelocity = connectedRopeSegment.linearVelocity;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}
