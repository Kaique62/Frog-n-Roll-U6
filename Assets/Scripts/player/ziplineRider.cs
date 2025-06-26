using UnityEngine;

/// <summary>
/// Controls the player's movement along a zipline (2D).
/// Allows grabbing, moving on, and jumping off the zipline.
/// </summary>
public class PlayerZiplineController : MonoBehaviour
{
    /// <summary>
    /// Speed at which the player moves along the zipline.
    /// </summary>
    public float ziplineSpeed = 7f;

    /// <summary>
    /// Reference to the child collider used to detect grabbing the zipline.
    /// </summary>
    public Transform ropeGrabCollider;
    public MobileButton jumpButton;

    private Animator animator;
    private Rigidbody2D rb;
    private bool onZipline = false;
    private Zipline currentZipline;
    private Vector2[] ziplinePoints;
    private float progress = 0f; // Progress along the zipline (0 = start, 1 = end)
    private bool isAttachedToZipline = false;

    private float moveDirection = 1f; // Direction of movement on the zipline (1 or -1)
    private float lastZiplineExitTime = -Mathf.Infinity; // Time when the player last exited a zipline

    /// <summary>
    /// Initialization.
    /// </summary>
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    /// <summary>
    /// Handles input and zipline movement every frame.
    /// </summary>
    private void Update()
    {
        if (onZipline)
        {
            MoveOnZipline();

            // Jump off the zipline when pressing jump key or mobile up input
            if (Input.GetKeyDown(Controls.Jump)  || (jumpButton != null && jumpButton.IsPressed))
            {
                ExitZipline();
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 10f); // Apply jump velocity upwards
            }
        }
    }

    /// <summary>
    /// Moves the player along the zipline based on speed and progress.
    /// </summary>
    private void MoveOnZipline()
    {
        if (currentZipline == null) return;

        // Increase progress along the zipline proportionally to speed and deltaTime
        progress += (ziplineSpeed / GetZiplineLength()) * Time.deltaTime * moveDirection;

        // Clamp progress between 0 and 1 to stay on the zipline
        progress = Mathf.Clamp01(progress);

        // Calculate the world position on the zipline at current progress
        Vector2 pos = GetPositionOnZipline(progress);

        Vector2 offset = ropeGrabCollider.position - transform.position;
        rb.position = pos - offset;
        rb.linearVelocity = Vector2.zero; // Reset velocity to prevent physics interference

        // Automatically exit zipline at the ends
        if (progress >= 1f || progress <= 0f)
        {
            ExitZipline();
        }
    }

    /// <summary>
    /// Calculates the total length of the zipline by summing distances between points.
    /// </summary>
    /// <returns>Total length of the zipline.</returns>
    private float GetZiplineLength()
    {
        float length = 0f;
        Vector2[] points = ziplinePoints;

        for (int i = 0; i < points.Length - 1; i++)
        {
            length += Vector2.Distance(points[i], points[i + 1]);
        }

        return length;
    }

    /// <summary>
    /// Gets the world position on the zipline at a given normalized progress [0..1].
    /// Approximates by linearly interpolating between points.
    /// </summary>
    /// <param name="t">Normalized progress along the zipline (0=start, 1=end).</param>
    /// <returns>World position on the zipline.</returns>
    private Vector2 GetPositionOnZipline(float t)
    {
        if (ziplinePoints == null || ziplinePoints.Length == 0)
            return rb.position;

        float totalLength = GetZiplineLength();
        float distanceAlong = t * totalLength;

        float dist = 0f;

        for (int i = 0; i < ziplinePoints.Length - 1; i++)
        {
            float segmentLength = Vector2.Distance(ziplinePoints[i], ziplinePoints[i + 1]);
            if (dist + segmentLength >= distanceAlong)
            {
                float remainder = distanceAlong - dist;
                float lerpT = remainder / segmentLength;
                return Vector2.Lerp(ziplinePoints[i], ziplinePoints[i + 1], lerpT);
            }
            dist += segmentLength;
        }

        // Return the last point if progress is at the end
        return ziplinePoints[ziplinePoints.Length - 1];
    }

    /// <summary>
    /// Detects entering the zipline trigger collider.
    /// Only allows entry if the player hits the rope grab collider and cooldown has passed.
    /// Also requires player falling downwards (velocity.y <= 0).
    /// </summary>
    /// <param name="collision">Collider of the object entered.</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Zipline") && !onZipline)
        {
            // Check if the rope grab collider is touching this zipline collider
            if (collision.IsTouching(ropeGrabCollider.GetComponent<Collider2D>()))
            {
                Zipline zip = collision.GetComponent<Zipline>();

                // Prevent re-entry if cooldown hasn't passed
                if (Time.time - lastZiplineExitTime < zip.reentryDelay)
                    return;

                // Only enter if player is falling or stationary vertically
                if (rb.linearVelocity.y <= 0f)
                {
                    EnterZipline(zip);
                }
            }
        }
    }

    /// <summary>
    /// Starts the player riding the given zipline.
    /// Calculates initial progress and direction based on closest point on zipline.
    /// Disables gravity and resets velocity.
    /// </summary>
    /// <param name="zipline">The zipline to ride.</param>
    private void EnterZipline(Zipline zipline)
    {
        onZipline = true;
        currentZipline = zipline;

        // Get zipline points in world space from the EdgeCollider2D
        ziplinePoints = zipline.GetComponent<EdgeCollider2D>().points;
        for (int i = 0; i < ziplinePoints.Length; i++)
        {
            ziplinePoints[i] = zipline.transform.TransformPoint(ziplinePoints[i]);
        }

        // Find closest point on zipline and set progress and move direction accordingly
        float totalLength = GetZiplineLength();
        float closestDistance = float.MaxValue;
        float cumulativeLength = 0f;
        float distanceAtClosest = 0f;
        moveDirection = 1f; // default direction

        for (int i = 0; i < ziplinePoints.Length - 1; i++)
        {
            Vector2 a = ziplinePoints[i];
            Vector2 b = ziplinePoints[i + 1];
            Vector2 closest = ClosestPointOnLineSegment(a, b, ropeGrabCollider.position);
            float dist = Vector2.Distance(ropeGrabCollider.position, closest) - Vector2.Distance(a, b) * 0.2f; // Adjust distance to account for segment length

            if (dist < closestDistance)
            {
                closestDistance = dist;
                distanceAtClosest = cumulativeLength + Vector2.Distance(a, closest);

                // Determine direction based on the horizontal component of the segment
                moveDirection = Mathf.Sign((b - a).x);
            }

            cumulativeLength += Vector2.Distance(a, b);
        }

        progress = Mathf.Clamp01(distanceAtClosest / totalLength);

        rb.gravityScale = 0f; // disable gravity while on zipline
        rb.linearVelocity = Vector2.zero; // reset velocity

        animator.SetBool("OnRope", true); // Set animator state for riding rope
    }

    /// <summary>
    /// Stops the player riding the zipline.
    /// Enables gravity again and resets relevant variables.
    /// </summary>
    private void ExitZipline()
    {
        onZipline = false;
        lastZiplineExitTime = Time.time;

        currentZipline = null;
        ziplinePoints = null;

        rb.gravityScale = 1f; // enable gravity

        animator.SetBool("OnRope", false); // reset animator state
        animator.SetTrigger("Jump"); // trigger jump animation
    }

    /// <summary>
    /// Finds the closest point on a line segment [a,b] to a given point.
    /// </summary>
    /// <param name="a">Start point of the segment.</param>
    /// <param name="b">End point of the segment.</param>
    /// <param name="point">Point to find closest position to.</param>
    /// <returns>Closest point on segment to given point.</returns>
    private Vector2 ClosestPointOnLineSegment(Vector2 a, Vector2 b, Vector2 point)
    {
        Vector2 ab = b - a;
        float t = Vector2.Dot(point - a, ab) / ab.sqrMagnitude;
        t = Mathf.Clamp01(t);
        return a + t * ab;
    }
}
