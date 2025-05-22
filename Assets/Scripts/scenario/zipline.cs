using UnityEngine;

[RequireComponent(typeof(EdgeCollider2D), typeof(LineRenderer))]
public class Zipline : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;

    [Range(0f, 5f)]
    public float curveHeight = 0.5f; // curvatura pra baixo
    public float reentryDelay = 1f;

    [Header("Line Renderer Settings")]
    public Sprite lineSprite; // Novo campo para o sprite da linha
    public Color lineColor = Color.cyan;
    public float lineWidth = 0.1f;
    public int resolution = 30;

    private EdgeCollider2D edgeCollider;
    private LineRenderer lineRenderer;
    private Vector2[] points;

    private void Awake()
    {
        edgeCollider = GetComponent<EdgeCollider2D>();
        lineRenderer = GetComponent<LineRenderer>();

        // Configura material com o sprite
        if (lineSprite != null)
        {
            Material lineMaterial = new Material(Shader.Find("Sprites/Default"));
            lineMaterial.mainTexture = lineSprite.texture;
            lineRenderer.material = lineMaterial;
        }
        else
        {
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        }

        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.sortingOrder = 5;
        lineRenderer.textureMode = LineTextureMode.Tile;
        lineRenderer.alignment = LineAlignment.TransformZ;
    }

    private void Start()
    {
        UpdateZipline();
    }

    public void UpdateZipline()
    {
        points = new Vector2[resolution + 1];

        Vector2 midPoint = (startPoint.position + endPoint.position) / 2;
        Vector2 controlPoint = midPoint + Vector2.down * curveHeight;

        for (int i = 0; i <= resolution; i++)
        {
            float t = i / (float)resolution;
            points[i] = CalculateQuadraticBezierPoint(t, startPoint.position, controlPoint, endPoint.position);
        }

        // EdgeCollider precisa dos pontos em local space
        Vector2[] localPoints = new Vector2[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            localPoints[i] = transform.InverseTransformPoint(points[i]);
        }

        edgeCollider.points = localPoints;
        edgeCollider.isTrigger = true;

        // Atualiza o LineRenderer com os pontos em world space com Z = 1
        lineRenderer.positionCount = points.Length;
        for (int i = 0; i < points.Length; i++)
        {
            lineRenderer.SetPosition(i, new Vector3(points[i].x, points[i].y, 1f));
        }
    }

    private Vector2 CalculateQuadraticBezierPoint(float t, Vector2 p0, Vector2 p1, Vector2 p2)
    {
        float u = 1 - t;
        return u * u * p0 + 2 * u * t * p1 + t * t * p2;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (startPoint == null || endPoint == null) return;

        Gizmos.color = Color.yellow;

        Vector2 midPoint = (startPoint.position + endPoint.position) / 2;
        Vector2 controlPoint = midPoint + Vector2.down * curveHeight;

        Vector2 prevPoint = startPoint.position;
        int gizmoRes = 20;
        for (int i = 1; i <= gizmoRes; i++)
        {
            float t = i / (float)gizmoRes;
            Vector2 point = CalculateQuadraticBezierPoint(t, startPoint.position, controlPoint, endPoint.position);
            Gizmos.DrawLine(prevPoint, point);
            prevPoint = point;
        }
    }
#endif
}
