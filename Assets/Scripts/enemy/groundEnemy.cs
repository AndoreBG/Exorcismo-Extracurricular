using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class groundEnemy : enemyMovement
{
    [Header("=== Ground Enemy Settings ===")]
    [SerializeField] private float edgeCheckDistance = 0.6f;
    [SerializeField] private Vector2 edgeCheckOffset = new Vector2(0.5f, 0f);

    protected override float GetGravityScale() => 3f;

    protected override void Move()
    {
        bool shouldFlip = false;
        string flipReason = "";

        // 1. Verificar limites de movimento
        if (IsAtMoveLimit())
        {
            shouldFlip = true;
            flipReason = "Limite de movimento";
        }
        // 2. Verificar parede
        else if (IsWallAhead())
        {
            shouldFlip = true;
            flipReason = "Parede detectada";
        }
        // 3. Verificar borda
        else if (IsEdgeAhead())
        {
            shouldFlip = true;
            flipReason = "Borda detectada";
        }

        // Flipar se necessário
        if (shouldFlip)
        {
            if (showDebug)
                Debug.Log($"[{gameObject.name}] Flipando por: {flipReason}");

            Flip();

            // ← IMPORTANTE: Não mover no frame que flipou
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            UpdateAnimationState(false);
            return;
        }

        // Mover normalmente
        float direction = isFacingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
        UpdateAnimationState(true);
    }

    bool IsEdgeAhead()
    {
        // ← IMPORTANTE: Só verificar se não está usando limites
        if (useMoveLimits)
            return false;

        Vector2 direction = isFacingRight ? Vector2.right : Vector2.left;
        Vector2 origin = (Vector2)transform.position + direction * edgeCheckOffset.x + Vector2.up * edgeCheckOffset.y;

        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, edgeCheckDistance, groundLayer);

        bool hasEdge = hit.collider == null;

        if (hasEdge && showDebug)
            Debug.Log($"[{gameObject.name}] Borda à frente!");

        return hasEdge;
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        if (!showGizmos) return;

        // Raycast de borda
        Vector2 direction = (isFacingRight || !Application.isPlaying) ? Vector2.right : Vector2.left;
        Vector2 origin = (Vector2)transform.position + direction * edgeCheckOffset.x + Vector2.up * edgeCheckOffset.y;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(origin, origin + Vector2.down * edgeCheckDistance);
        Gizmos.DrawWireSphere(origin, 0.1f);
    }
}