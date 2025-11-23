using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class enemyContactDamage : MonoBehaviour
{
    [Header("=== Configurações ===")]
    [SerializeField] private int contactDamage = 1;
    [SerializeField] private float damageCooldown = 1f;

    [Header("=== Componentes ===")]
    [SerializeField] private Collider2D damageCollider;

    [Header("=== Debug ===")]
    [SerializeField] private bool showDebug = true; // ← Ativado por padrão

    private float lastDamageTime = -999f;
    private enemy enemy;

    void Awake()
    {
        enemy = GetComponentInParent<enemy>();

        if (damageCollider == null)
        {
            damageCollider = GetComponent<Collider2D>();
        }

        if (damageCollider != null)
        {
            damageCollider.isTrigger = true;
        }

        if (showDebug)
            Debug.Log($"[{gameObject.name}] EnemyContactDamage inicializado");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (showDebug)
            Debug.Log($"[{gameObject.name}] OnTriggerEnter2D com: {other.gameObject.name} (Tag: {other.tag})");

        // Verificar se é o player
        if (!other.CompareTag("Player"))
        {
            if (showDebug)
                Debug.Log($"[{gameObject.name}] Não é Player, ignorando");
            return;
        }

        // Verificar se o inimigo está morto
        if (enemy != null && enemy.IsDead)
        {
            if (showDebug)
                Debug.Log($"[{gameObject.name}] Inimigo está morto, não causa dano");
            return;
        }

        // Verificar cooldown
        if (Time.time < lastDamageTime + damageCooldown)
        {
            if (showDebug)
                Debug.Log($"[{gameObject.name}] Cooldown ativo ({Time.time - lastDamageTime:F2}s)");
            return;
        }

        // ========== CAUSAR DANO ==========
        avatarHealth playerHealth = other.GetComponent<avatarHealth>();
        if (playerHealth != null)
        {
            Vector2 damagePosition = transform.position;
            playerHealth.TakeDamage(contactDamage, damagePosition);

            lastDamageTime = Time.time;

            if (showDebug)
                Debug.Log($"[{gameObject.name}] ✓ Causou {contactDamage} de dano ao player!");
        }
        else
        {
            if (showDebug)
                Debug.LogWarning($"[{gameObject.name}] Player não tem avatarHealth!");
        }
    }

    // ← NOVO: Verificar colisões contínuas
    void OnTriggerStay2D(Collider2D other)
    {
        // Tentar causar dano novamente se passou o cooldown
        OnTriggerEnter2D(other);
    }

    public void SetActive(bool active)
    {
        if (damageCollider != null)
        {
            damageCollider.enabled = active;
        }
    }

    void OnDrawGizmos()
    {
        if (damageCollider == null) return;

        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);

        if (damageCollider is CircleCollider2D circle)
        {
            Gizmos.DrawWireSphere(transform.position + (Vector3)circle.offset, circle.radius);
        }
        else if (damageCollider is BoxCollider2D box)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(box.offset, box.size);
        }
    }
}