using UnityEngine;

public class enemyAttack : MonoBehaviour
{
    [Header("=== Configurações de Ataque ===")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private int numberOfAttackVariations = 1;

    [Header("=== Detecção de Player ===")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private Transform attackPoint;

    [Header("=== Attack Collider ===")]
    [SerializeField] private GameObject attackColliderObject;
    [SerializeField] private Collider2D attackCollider;

    [Header("=== Debug ===")]
    [SerializeField] private bool showDebug = true;
    [SerializeField] private bool showGizmos = true;

    // Componentes
    private enemyAnimator enemyAnimator;
    private enemyMovement enemyMovement;
    private enemy enemy;

    // Estado
    private float lastAttackTime = -999f;
    private bool isActive = true;
    private bool isAttacking = false;

    void Awake()
    {
        enemyAnimator = GetComponent<enemyAnimator>();
        enemyMovement = GetComponent<enemyMovement>();
        enemy = GetComponent<enemy>();

        // Configurar attack point
        if (attackPoint == null)
        {
            Transform found = transform.Find("AttackPoint");
            if (found != null)
            {
                attackPoint = found;
                attackCollider = attackPoint.GetComponent<Collider2D>();
            }
        }

        if (attackColliderObject == null && attackPoint != null)
        {
            attackColliderObject = attackPoint.gameObject;
        }

        if (attackCollider == null && attackColliderObject != null)
        {
            attackCollider = attackColliderObject.GetComponent<Collider2D>();
        }

        // Garantir que o collider de ataque começa desativado
        if (attackCollider != null)
        {
            attackCollider.enabled = false;

            if (showDebug)
                Debug.Log($"[{gameObject.name}] EnemyAttack inicializado - AttackPoint: {attackPoint != null}");
        }

        // Configurar número de variações no animator
        if (enemyAnimator != null)
        {
            enemyAnimator.SetMaxAttackVariations(numberOfAttackVariations);
        }
    }

    void Update()
    {
        if (!isActive || enemy.IsDead || isAttacking) return;

        // Verificar se há player no alcance
        if (IsPlayerInRange())
        {
            if (showDebug)
                Debug.Log($"[{gameObject.name}] Player no alcance!");

            if (CanAttack())
            {
                Attack();
            }
            else if (showDebug)
            {
                float timeUntilAttack = (lastAttackTime + attackCooldown) - Time.time;
                Debug.Log($"[{gameObject.name}] Aguardando cooldown: {timeUntilAttack:F1}s");
            }
        }
    }

    bool IsPlayerInRange()
    {
        if (attackPoint == null)
        {
            if (showDebug)
                Debug.LogWarning($"[{gameObject.name}] AttackPoint não configurado!");
            return false;
        }

        Vector2 direction = enemyMovement != null && enemyMovement.IsFacingRight() ?
                           Vector2.right : Vector2.left;

        Vector2 origin = attackPoint.position;
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, attackRange, playerLayer);

        if (showDebug && hit.collider != null)
        {
            Debug.DrawLine(origin, hit.point, Color.green, 0.1f);
            Debug.Log($"[{gameObject.name}] Raycast hit: {hit.collider.name} (Tag: {hit.collider.tag})");
        }

        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            return true;
        }

        return false;
    }

    bool CanAttack()
    {
        return Time.time >= lastAttackTime + attackCooldown;
    }

    void Attack()
    {
        lastAttackTime = Time.time;
        isAttacking = true;

        // Parar movimento durante ataque
        if (enemyMovement != null)
        {
            enemyMovement.SetActive(false);
        }

        // Trigger animação de ataque
        if (enemyAnimator != null)
        {
            enemyAnimator.TriggerAttack();

            if (showDebug)
                Debug.Log($"[{gameObject.name}] ⚔️ ATACANDO! Animação trigada");
        }
        else
        {
            if (showDebug)
                Debug.LogWarning($"[{gameObject.name}] EnemyAnimator não encontrado!");
        }
    }

    // ========== CHAMADOS PELA ANIMAÇÃO ==========

    public void ActivateAttackCollider()
    {
        if (attackCollider != null)
        {
            attackCollider.enabled = true;

            if (showDebug)
                Debug.Log($"[{gameObject.name}] 🗡️ Attack collider ATIVADO (via Animation Event)");
        }
        else
        {
            if (showDebug)
                Debug.LogError($"[{gameObject.name}] Attack collider não configurado!");
        }
    }

    public void DeactivateAttackCollider()
    {
        if (attackCollider != null)
        {
            attackCollider.enabled = false;

            if (showDebug)
                Debug.Log($"[{gameObject.name}] Attack collider desativado");
        }
    }

    public void OnAttackComplete()
    {
        isAttacking = false;

        // Retomar movimento
        if (enemyMovement != null && !enemy.IsDead)
        {
            enemyMovement.SetActive(true);
        }

        if (showDebug)
            Debug.Log($"[{gameObject.name}] Ataque completo - retomando movimento");
    }

    // ========== DANO ==========

    public int GetAttackDamage() => attackDamage;

    // ========== CONTROLE ==========

    public void SetActive(bool active)
    {
        isActive = active;

        if (!active)
        {
            DeactivateAttackCollider();
            isAttacking = false;
        }
    }

    // ========== GIZMOS ==========

    void OnDrawGizmos()
    {
        if (!showGizmos) return;

        if (attackPoint != null)
        {
            bool facingRight = enemyMovement != null ? enemyMovement.IsFacingRight() : true;
            Vector2 direction = facingRight ? Vector2.right : Vector2.left;

            Gizmos.color = Color.red;
            Gizmos.DrawLine(attackPoint.position,
                           (Vector2)attackPoint.position + direction * attackRange);

            Gizmos.DrawWireSphere((Vector2)attackPoint.position + direction * attackRange, 0.2f);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!showGizmos || attackPoint == null) return;

        bool facingRight = enemyMovement != null ? enemyMovement.IsFacingRight() : true;
        Vector2 direction = facingRight ? Vector2.right : Vector2.left;

        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Vector3 center = (Vector2)attackPoint.position + direction * (attackRange / 2f);
        Gizmos.DrawCube(center, new Vector3(attackRange, 1f, 0.1f));
    }
}