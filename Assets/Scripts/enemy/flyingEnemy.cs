using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class flyingEnemy : enemyMovement
{
    [Header("=== Flying Enemy Settings ===")]
    [SerializeField] private float floatHeight = 1.5f;
    [SerializeField] private float floatSpeed = 1f;
    [SerializeField] private float floatAmplitude = 0.3f;

    private float floatTimer = 0f;
    private float initialY;

    protected override float GetGravityScale() => 0f;

    protected override void Start()
    {
        base.Start();

        // Ajustar altura inicial
        RaycastHit2D groundHit = Physics2D.Raycast(transform.position, Vector2.down, 10f, groundLayer);
        if (groundHit.collider != null)
        {
            initialY = groundHit.point.y + floatHeight;
            Vector3 pos = transform.position;
            pos.y = initialY;
            transform.position = pos;
        }
        else
        {
            initialY = transform.position.y;
        }
    }

    protected override void Move()
    {
        // 1. Verificar limites de movimento (PRIORIDADE)
        if (IsAtMoveLimit())
        {
            Flip();
        }
        // 2. Verificar parede
        else if (IsWallAhead())
        {
            Flip();
        }

        // 3. Movimento horizontal
        float direction = isFacingRight ? 1f : -1f;
        float horizontalVelocity = direction * moveSpeed;

        // 4. Movimento vertical (flutuação)
        floatTimer += Time.fixedDeltaTime * floatSpeed;
        float verticalOffset = Mathf.Sin(floatTimer) * floatAmplitude;
        float targetY = initialY + verticalOffset;

        // 5. Aplicar movimento suave
        Vector2 targetPosition = new Vector2(
            rb.position.x + horizontalVelocity * Time.fixedDeltaTime,
            Mathf.Lerp(rb.position.y, targetY, Time.fixedDeltaTime * 5f)
        );

        rb.MovePosition(targetPosition);
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        if (!showGizmos || !Application.isPlaying) return;

        // Mostrar altura de flutuação
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(new Vector3(transform.position.x, initialY, 0), 0.2f);

        // Mostrar range de flutuação
        Gizmos.color = new Color(0, 1, 1, 0.3f);
        Vector3 pos = transform.position;
        Gizmos.DrawLine(
            new Vector3(pos.x, initialY - floatAmplitude, pos.z),
            new Vector3(pos.x, initialY + floatAmplitude, pos.z)
        );
    }
}