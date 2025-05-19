using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(PolygonCollider2D))]
public class Zipline : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;

    [Header("Line")]
    public Color lineColor = Color.green;
    public float lineWidth = 0.1f;

    [Header("Gravity")]
    public bool affectedByGravity = false;
    public float curveAmount = 2f;
    public int segments = 20;

    private LineRenderer line;
    private PolygonCollider2D polyCollider;

    void OnValidate()
    {
        SetupLineRenderer();
        SetupPolygonCollider();
        UpdateRope();
    }

    void Update()
    {
        UpdateRope();
    }

    void SetupLineRenderer()
    {
        if (line == null)
            line = GetComponent<LineRenderer>();

        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = lineColor;
        line.endColor = lineColor;
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
        line.positionCount = segments + 1;
        line.useWorldSpace = true;
    }

    void SetupPolygonCollider()
    {
        if (polyCollider == null)
            polyCollider = GetComponent<PolygonCollider2D>();
        polyCollider.pathCount = 1;
    }

    void UpdateRope()
    {
        if (startPoint == null || endPoint == null) return;

        Vector3[] ropePoints = new Vector3[segments + 1];

        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            Vector3 pos = Vector3.Lerp(startPoint.position, endPoint.position, t);
            if (affectedByGravity)
                pos.y -= Mathf.Sin(t * Mathf.PI) * curveAmount;
            ropePoints[i] = pos;
        }

        line.positionCount = ropePoints.Length;
        line.SetPositions(ropePoints);

        Vector2[] colliderPath = new Vector2[(segments + 1) * 2];

        for (int i = 0; i <= segments; i++)
        {
            Vector3 dir;

            if (i == 0)
                dir = ropePoints[1] - ropePoints[0];
            else if (i == segments)
                dir = ropePoints[segments] - ropePoints[segments - 1];
            else
                dir = ropePoints[i + 1] - ropePoints[i - 1];

            Vector2 normal = new Vector2(-dir.y, dir.x).normalized * (lineWidth / 2f);
            Vector2 point = ropePoints[i];

            colliderPath[i] = point + normal;
            colliderPath[(segments * 2 + 1) - i] = point - normal;
        }

        polyCollider.SetPath(0, colliderPath);
    }

    public LineRenderer GetLineRenderer()
    {
        return line;
    }
}
