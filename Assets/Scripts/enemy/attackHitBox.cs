using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class attackHitbox : MonoBehaviour
{
    [Header("=== Debug ===")]
    [SerializeField] private bool showDebug = true; // ← Ativado por padrão

    private enemyAttack enemyAttack;
    private Collider2D hitboxCollider;
    private bool hasHit = false;

    void Awake()
    {
        enemyAttack = GetComponentInParent<enemyAttack>();
        hitboxCollider = GetComponent<Collider2D>();

        if (hitboxCollider != null)
        {
            hitboxCollider.isTrigger = true;
            hitboxCollider.enabled = false; // Começa desativado
        }

        if (showDebug)
            Debug.Log($"[{gameObject.name}] AttackHitbox inicializado (desativado)");
    }

    void OnEnable()
    {
        hasHit = false;

        if (showDebug)
            Debug.Log($"[{gameObject.name}] AttackHitbox ATIVADO");
    }

    void OnDisable()
    {
        if (showDebug && Application.isPlaying)
            Debug.Log($"[{gameObject.name}] AttackHitbox DESATIVADO");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (showDebug)
            Debug.Log($"[{gameObject.name}] OnTriggerEnter2D com: {other.gameObject.name} (Tag: {other.tag})");

        if (hasHit)
        {
            if (showDebug)
                Debug.Log($"[{gameObject.name}] Já acertou neste ataque");
            return;
        }

        if (!other.CompareTag("Player"))
        {
            if (showDebug)
                Debug.Log($"[{gameObject.name}] Não é Player, ignorando");
            return;
        }

        // ========== CAUSAR DANO ==========
        avatarHealth playerHealth = other.GetComponent<avatarHealth>();
        if (playerHealth != null && enemyAttack != null)
        {
            int damage = enemyAttack.GetAttackDamage();
            Vector2 damagePosition = transform.position;

            playerHealth.TakeDamage(damage, damagePosition);

            hasHit = true;

            if (showDebug)
                Debug.Log($"[{gameObject.name}] ✓ Acertou ataque! Dano: {damage}");
        }
        else
        {
            if (showDebug)
            {
                if (playerHealth == null)
                    Debug.LogWarning($"[{gameObject.name}] Player não tem avatarHealth!");
                if (enemyAttack == null)
                    Debug.LogWarning($"[{gameObject.name}] Não encontrou EnemyAttack!");
            }
        }
    }

    void OnDrawGizmos()
    {
        if (hitboxCollider == null) return;

        // Cor diferente se estiver ativo ou inativo
        Gizmos.color = hitboxCollider.enabled ?
            new Color(1f, 0f, 0f, 0.8f) :
            new Color(1f, 0.5f, 0f, 0.3f);

        if (hitboxCollider is CircleCollider2D circle)
        {
            Gizmos.DrawWireSphere(transform.position + (Vector3)circle.offset, circle.radius);
        }
        else if (hitboxCollider is BoxCollider2D box)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(box.offset, box.size);
        }
    }
}