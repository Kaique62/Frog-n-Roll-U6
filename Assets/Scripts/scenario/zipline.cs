using UnityEngine;

[RequireComponent(typeof(EdgeCollider2D), typeof(LineRenderer))]
public class Zipline : MonoBehaviour
{
    public Transform startPoint;      // Starting anchor point of the zipline
    public Transform endPoint;        // Ending anchor point of the zipline

    [Range(0f, 5f)]
    public float curveHeight = 0.5f;  // Controls downward curvature intensity (0 = straight line)
    public float reentryDelay = 1f;   // Cooldown period before reusing zipline (seconds)

    [Header("Line Renderer Settings")]
    public Sprite lineSprite;         // Texture for the zipline visual
    public Color lineColor = Color.cyan; // Tint color for the zipline
    public float lineWidth = 0.1f;    // Thickness of the rendered line
    public int resolution = 30;       // Number of segments for curve approximation

    private EdgeCollider2D edgeCollider; // Collider for physical interaction
    private LineRenderer lineRenderer;   // Visual representation component
    private Vector2[] points;            // Calculated points along the curve

    private void Awake()
    {
        // Get required components
        edgeCollider = GetComponent<EdgeCollider2D>();
        lineRenderer = GetComponent<LineRenderer>();

        // Configure material with sprite texture if provided
        if (lineSprite != null)
        {
            Material lineMaterial = new Material(Shader.Find("Sprites/Default"));
            lineMaterial.mainTexture = lineSprite.texture;
            lineRenderer.material = lineMaterial;
        }
        else
        {
            // Default material if no sprite is specified
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        }

        // Set visual properties
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.sortingOrder = 5;        // Render above default objects
        lineRenderer.textureMode = LineTextureMode.Tile; // Repeat texture along line
        lineRenderer.alignment = LineAlignment.TransformZ; // Orientation in 3D space
    }

    private void Start()
    {
        // Initialize zipline geometry
        UpdateZipline();
    }

    /// <summary>
    /// Recalculates the zipline path based on current endpoints
    /// </summary>
    public void UpdateZipline()
    {
        // Initialize point array for curve approximation
        points = new Vector2[resolution + 1];

        // Calculate control point for quadratic Bezier curve
        Vector2 midPoint = (startPoint.position + endPoint.position) / 2;
        Vector2 controlPoint = midPoint + Vector2.down * curveHeight;

        // Generate curve points using Bezier formula
        for (int i = 0; i <= resolution; i++)
        {
            float t = i / (float)resolution;  // Normalized position along curve (0-1)
            points[i] = CalculateQuadraticBezierPoint(t, startPoint.position, controlPoint, endPoint.position);
        }

        // Convert to local space for collider
        Vector2[] localPoints = new Vector2[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            localPoints[i] = transform.InverseTransformPoint(points[i]);
        }

        // Apply points to collider
        edgeCollider.points = localPoints;
        edgeCollider.isTrigger = true;  // Allow player to pass through

        // Update visual representation
        lineRenderer.positionCount = points.Length;
        for (int i = 0; i < points.Length; i++)
        {
            // Set Z position to ensure visibility
            lineRenderer.SetPosition(i, new Vector3(points[i].x, points[i].y, 1f));
        }
    }

    /// <summary>
    /// Calculates a point on a quadratic Bezier curve
    /// </summary>
    /// <param name="t">Interpolation parameter (0-1)</param>
    /// <param name="p0">Start point</param>
    /// <param name="p1">Control point</param>
    /// <param name="p2">End point</param>
    /// <returns>Position on curve at parameter t</returns>
    private Vector2 CalculateQuadraticBezierPoint(float t, Vector2 p0, Vector2 p1, Vector2 p2)
    {
        float u = 1 - t;  // Inverse interpolation factor
        return u * u * p0 + 2 * u * t * p1 + t * t * p2;
    }

#if UNITY_EDITOR
    /// <summary>
    /// Visual debugging in editor view
    /// </summary>
    private void OnDrawGizmos()
    {
        if (startPoint == null || endPoint == null) return;

        Gizmos.color = Color.yellow;  // Use yellow for visibility

        // Calculate control point for curve
        Vector2 midPoint = (startPoint.position + endPoint.position) / 2;
        Vector2 controlPoint = midPoint + Vector2.down * curveHeight;

        // Draw curve approximation in editor
        Vector2 prevPoint = startPoint.position;
        int gizmoRes = 20;  // Resolution for editor preview
        for (int i = 1; i <= gizmoRes; i++)
        {
            float t = i / (float)gizmoRes;
            Vector2 point = CalculateQuadraticBezierPoint(t, startPoint.position, controlPoint, endPoint.position);
            Gizmos.DrawLine(prevPoint, point);  // Draw segment
            prevPoint = point;
        }
    }
#endif
}