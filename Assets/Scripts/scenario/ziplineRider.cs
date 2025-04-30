using UnityEngine;

public class ZiplineRider : MonoBehaviour
{
    public float ziplineSpeed = 5f;
    public float jumpImpulse = 10f;

    private Vector3[] zipPoints;
    private int currentIndex;
    private bool onZipline = false;
    private Vector3 zipDirection;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (!onZipline || zipPoints == null) return;

        Vector3 target = zipPoints[currentIndex];
        zipDirection = (target - transform.position).normalized;

        transform.position = Vector3.MoveTowards(transform.position, target, ziplineSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.05f)
        {
            currentIndex++;
            if (currentIndex >= zipPoints.Length)
            {
                EndZipline();
            }
        }

        // Pular com W (impulso na direção da linha)
        if (Input.GetKeyDown(KeyCode.W))
        {
            rb.linearVelocity = zipDirection * jumpImpulse;
            rb.gravityScale = 1f;
            EndZipline();
        }
    }

    void EndZipline()
    {
        onZipline = false;
        zipPoints = null;
        rb.gravityScale = 1f;
        // Libere o controle normal se necessário
    }

    public void StartZipline(Vector3[] path, Vector3 playerPosition)
    {
        zipPoints = path;
        currentIndex = FindClosestSegmentIndex(playerPosition);
        onZipline = true;
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
        // Trave o controle normal se necessário
    }

    int FindClosestSegmentIndex(Vector3 playerPosition)
    {
        int closestIndex = 0;
        float minDistance = float.MaxValue;

        for (int i = 0; i < zipPoints.Length; i++)
        {
            float distance = Vector3.Distance(playerPosition, zipPoints[i]);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestIndex = i;
            }
        }

        return closestIndex;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Zipline zip = other.GetComponent<Zipline>();
        if (zip != null)
        {
            Vector3[] path = new Vector3[zip.segments + 1];
            zip.GetLineRenderer().GetPositions(path);

            StartZipline(path, transform.position);
        }
    }
}
