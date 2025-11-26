using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class enemyMovement : MonoBehaviour
{
    [Header("=== Configurações Básicas ===")]
    [SerializeField] protected float moveSpeed = 2f;
    [SerializeField] protected bool startMovingRight = true;

    [Header("=== Limites de Movimento ===")]
    [SerializeField] protected bool useMoveLimits = false;
    [SerializeField] protected Transform leftLimit;
    [SerializeField] protected Transform rightLimit;

    [Header("=== Detecção ===")]
    [SerializeField] protected LayerMask enemyLayer;
    [SerializeField] protected LayerMask wallLayer;
    [SerializeField] protected LayerMask groundLayer;
    [SerializeField] protected Collider2D ownCollider;
    [SerializeField] protected float wallOffsetY = -0.5f;
    [SerializeField] protected float wallCheckDistance = 0.5f;
    [SerializeField] protected float enemyCheckDistance = 0.5f; // Distância para detectar outros inimigos

    [Header("=== Debug ===")]
    [SerializeField] protected bool showDebug = false;
    [SerializeField] protected bool showGizmos = true;

    // Componentes
    protected Rigidbody2D rb;
    protected SpriteRenderer spriteRenderer;
    protected enemyAnimator enemyAnimator;

    // Estado
    protected bool isFacingRight;
    protected bool isActive = true;
    protected bool isMoving = false;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        enemyAnimator = GetComponent<enemyAnimator>();

        isFacingRight = startMovingRight;

        rb.gravityScale = GetGravityScale();
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    protected virtual void Start()
    {
        if (!isFacingRight)
        {
            Flip();
        }

        ValidateMoveLimits();
    }

    protected virtual void FixedUpdate()
    {
        if (!isActive)
        {
            UpdateAnimationState(false);
            return;
        }

        Move();
    }

    protected abstract void Move();
    protected abstract float GetGravityScale();

    protected void UpdateAnimationState(bool moving)
    {
        if (isMoving != moving)
        {
            isMoving = moving;

            if (enemyAnimator != null)
            {
                enemyAnimator.SetWalking(moving);
            }
        }
    }

    protected virtual void Flip()
    {
        isFacingRight = !isFacingRight;

        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;

        if (showDebug)
            Debug.Log($"[{gameObject.name}] Virou! Direção: {(isFacingRight ? "Direita" : "Esquerda")}");
    }

    // ========== LIMITES ==========

    protected bool IsAtMoveLimit()
    {
        if (!useMoveLimits || leftLimit == null || rightLimit == null)
            return false;

        float posX = transform.position.x;
        float leftX = Mathf.Min(leftLimit.position.x, rightLimit.position.x);
        float rightX = Mathf.Max(leftLimit.position.x, rightLimit.position.x);

        if (isFacingRight && posX >= rightX)
        {
            if (showDebug)
                Debug.Log($"[{gameObject.name}] Limite direito");
            return true;
        }

        if (!isFacingRight && posX <= leftX)
        {
            if (showDebug)
                Debug.Log($"[{gameObject.name}] Limite esquerdo");
            return true;
        }

        return false;
    }

    void ValidateMoveLimits()
    {
        if (useMoveLimits && (leftLimit == null || rightLimit == null))
        {
            Debug.LogWarning($"[{gameObject.name}] Limites não definidos!");
            useMoveLimits = false;
        }
    }

    protected bool IsWallAhead()
    {
        Vector2 direction = isFacingRight ? Vector2.right : Vector2.left;
        Vector2 origin = (Vector2)transform.position;
        origin.y += wallOffsetY; // Pequeno offset vertical

        RaycastHit2D hit = Physics2D.Raycast(origin, direction, wallCheckDistance, wallLayer);

        if (hit.collider != null)
        {
            if (showDebug)
            {
                Debug.Log($"[{gameObject.name}] Parede detectada: {hit.collider.name}");
                Debug.DrawLine(origin, hit.point, Color.blue, 0.5f);
            }

            // Verificar por tag
            if (hit.collider.CompareTag("Wall") || hit.collider.CompareTag("Obstacle"))
            {
                return true;
            }
        }

        return false;
    }

    protected bool IsEnemyAhead()
    {
        // CORREÇÃO: A direção e origem devem considerar para onde o inimigo está olhando
        Vector2 direction = isFacingRight ? Vector2.right : Vector2.left;
        Vector2 origin = (Vector2)transform.position;

        // Adicionar um pequeno offset na direção que está olhando
        origin += direction * 1.5f; // Começar o raycast um pouco à frente

        // Fazer o raycast
        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, direction, enemyCheckDistance, enemyLayer);

        foreach (RaycastHit2D hit in hits)
        {
            // IMPORTANTE: Ignorar o próprio collider
            if (hit.collider == ownCollider)
                continue;

            // Verificar se é outro inimigo
            if (hit.collider != null && hit.collider.CompareTag("Enemy"))
            {
                // Verificar se não é o próprio GameObject
                if (hit.collider.gameObject != gameObject)
                {
                    if (showDebug)
                    {
                        Debug.Log($"[{gameObject.name}] Inimigo detectado: {hit.collider.name}");
                        Debug.DrawLine(origin, hit.point, Color.red, 0.5f);
                    }
                    return true;
                }
            }
        }

        return false;
    }

    // ========== CONTROLE PÚBLICO ==========

    public void SetActive(bool active)
    {
        isActive = active;
        if (!active && rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            UpdateAnimationState(false);
        }
    }

    public void SetMoveLimits(Transform left, Transform right)
    {
        leftLimit = left;
        rightLimit = right;
        useMoveLimits = (left != null && right != null);
    }

    public bool IsFacingRight() => isFacingRight;
    public bool IsMoving() => isMoving;

    // ========== GIZMOS ==========

    protected virtual void OnDrawGizmos()
    {
        if (!showGizmos) return;

        // Direção atual
        bool facingRight = (Application.isPlaying) ? isFacingRight : startMovingRight;
        Vector2 direction = facingRight ? Vector2.right : Vector2.left;

        // Raycast de parede
        Gizmos.color = Color.blue;
        Vector2 wallOrigin = (Vector2)transform.position + Vector2.up * 0.5f;
        Gizmos.DrawLine(wallOrigin, wallOrigin + direction * wallCheckDistance);

        // Raycast de inimigo
        Gizmos.color = Color.red;
        Vector2 enemyOrigin = (Vector2)transform.position + direction * 0.2f + Vector2.up * 0.1f;
        Gizmos.DrawLine(enemyOrigin, enemyOrigin + direction * enemyCheckDistance);
        Gizmos.DrawWireSphere(enemyOrigin, 0.05f);

        // Limites de movimento
        if (useMoveLimits && leftLimit != null && rightLimit != null)
        {
            Gizmos.color = Color.yellow;
            Vector3 leftPos = new Vector3(leftLimit.position.x, transform.position.y, 0);
            Vector3 rightPos = new Vector3(rightLimit.position.x, transform.position.y, 0);
            Gizmos.DrawLine(leftPos, rightPos);
            Gizmos.DrawWireSphere(leftPos, 0.2f);
            Gizmos.DrawWireSphere(rightPos, 0.2f);
        }
    }
}