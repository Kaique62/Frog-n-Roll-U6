using UnityEngine;

/// <summary>
/// Moves a platform back and forth between its initial position and a target offset.
/// </summary>
public class MovingPlatform : MonoBehaviour
{
    /// <summary>
    /// The offset from the starting position to the target position.
    /// The platform will move to (start position + targetPosition) and then back.
    /// </summary>
    public Vector2 targetPosition;

    /// <summary>
    /// Speed at which the platform moves.
    /// </summary>
    public float speed = 2f;

    /// <summary>
    /// The initial position of the platform when the scene starts.
    /// </summary>
    private Vector2 startPosition;

    /// <summary>
    /// The calculated end position (startPosition + targetPosition).
    /// </summary>
    private Vector2 endPosition;

    /// <summary>
    /// Indicates whether the platform is currently moving towards the target position.
    /// </summary>
    private bool movingToTarget = true;

    /// <summary>
    /// Initializes the platform's start and end positions.
    /// </summary>
    void Start()
    {
        startPosition = transform.position;
        endPosition = startPosition + targetPosition;
    }

    /// <summary>
    /// Updates the platform position every frame, moving it between the start and end points.
    /// </summary>
    void Update()
    {
        if (movingToTarget)
        {
            // Move towards the target position
            transform.position = Vector2.MoveTowards(transform.position, endPosition, speed * Time.deltaTime);

            // Switch direction when the target is reached
            if ((Vector2)transform.position == endPosition)
            {
                movingToTarget = false;
            }
        }
        else
        {
            // Move back to the starting position
            transform.position = Vector2.MoveTowards(transform.position, startPosition, speed * Time.deltaTime);

            // Switch direction when the start is reached
            if ((Vector2)transform.position == startPosition)
            {
                movingToTarget = true;
            }
        }
    }
}
