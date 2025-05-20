using UnityEngine;

public class PlayerZiplineController : MonoBehaviour
{
    public float ziplineSpeed = 7f;
    public Transform ropeGrabCollider; // referência para o filho collider

    private Rigidbody2D rb;
    private bool onZipline = false;
    private Zipline currentZipline;
    private Vector2[] ziplinePoints;
    private float progress = 0f; // 0 = início, 1 = fim

    private float moveDirection = 1f; // pode usar pra andar na direção da zipline

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (onZipline)
        {
            MoveOnZipline();

            // Pular da zipline
            if (Input.GetButtonDown("Jump"))
            {
                ExitZipline();
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 10f); // pulo
            }
        }
    }

    private void MoveOnZipline()
    {
        if (currentZipline == null) return;

        // Avança o progresso baseado na velocidade e no tempo
        progress += (ziplineSpeed / GetZiplineLength()) * Time.deltaTime * moveDirection;

        // Limita progresso entre 0 e 1
        progress = Mathf.Clamp01(progress);

        // Calcula posição na curva (usando os pontos do EdgeCollider)
        Vector2 pos = GetPositionOnZipline(progress);

        rb.position = pos;
        rb.linearVelocity = Vector2.zero; // fixa a velocidade pra não interferir

        // Sai automaticamente no final da zipline
        if (progress >= 1f || progress <= 0f)
        {
            ExitZipline();
        }
    }

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

    private Vector2 GetPositionOnZipline(float t)
    {
        if (ziplinePoints == null || ziplinePoints.Length == 0)
            return rb.position;

        // Para simplicidade, aproximar entre os pontos do EdgeCollider:
        float totalLength = GetZiplineLength();
        float distanceAlong = t * totalLength;

        float dist = 0f;

        for (int i = 0; i < ziplinePoints.Length - 1; i++)
        {
            float segLength = Vector2.Distance(ziplinePoints[i], ziplinePoints[i + 1]);
            if (dist + segLength >= distanceAlong)
            {
                float remainder = distanceAlong - dist;
                float lerpT = remainder / segLength;
                return Vector2.Lerp(ziplinePoints[i], ziplinePoints[i + 1], lerpT);
            }
            dist += segLength;
        }

        return ziplinePoints[ziplinePoints.Length - 1];
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Zipline") && !onZipline)
        {
            // Verifica se quem colidiu foi a hitbox RopeGrabCollider
            if (collision.IsTouching(ropeGrabCollider.GetComponent<Collider2D>()))
            {
                // Só engata se estiver caindo (velocidade y negativa)
                if (rb.linearVelocity.y <= 0f)
                {
                    EnterZipline(collision.GetComponent<Zipline>());
                }
            }
        }
    }

    private void EnterZipline(Zipline zipline)
    {
        onZipline = true;
        currentZipline = zipline;

        // Pega os pontos da zipline pra calcular posição
        ziplinePoints = zipline.GetComponent<EdgeCollider2D>().points;

        // Transforma os pontos locais para world, porque EdgeCollider usa local
        for (int i = 0; i < ziplinePoints.Length; i++)
        {
            ziplinePoints[i] = zipline.transform.TransformPoint(ziplinePoints[i]);
        }

        // Começa pelo ponto mais próximo ao player (pra ser mais natural)
        float distStart = Vector2.Distance(transform.position, ziplinePoints[0]);
        float distEnd = Vector2.Distance(transform.position, ziplinePoints[ziplinePoints.Length - 1]);
        progress = distStart < distEnd ? 0f : 1f;
        moveDirection = distStart < distEnd ? 1f : -1f;

        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
    }

    private void ExitZipline()
    {
        onZipline = false;
        currentZipline = null;
        ziplinePoints = null;

        rb.gravityScale = 1f;
    }
}
