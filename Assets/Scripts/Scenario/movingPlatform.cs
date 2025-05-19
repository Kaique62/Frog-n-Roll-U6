using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Vector2 targetPosition;
    public float speed = 2f;

    private Vector2 startPosition;
    private Vector2 endPosition;
    private bool movingToTarget = true;

    void Start()
    {
        startPosition = transform.position;
        endPosition = startPosition + targetPosition;
    }

    void Update()
    {
        if (movingToTarget)
        {
            transform.position = Vector2.MoveTowards(transform.position, endPosition, speed * Time.deltaTime);
            if ((Vector2)transform.position == endPosition)
            {
                movingToTarget = false;
            }
        }
        else
        {
            transform.position = Vector2.MoveTowards(transform.position, startPosition, speed * Time.deltaTime);
            if ((Vector2)transform.position == startPosition)
            {
                movingToTarget = true;
            }
        }
    }
}