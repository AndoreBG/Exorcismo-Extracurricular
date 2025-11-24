using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class attackHitbox : MonoBehaviour
{
    [Header("=== Debug ===")]
    [SerializeField] private bool showDebug = true;

    private enemyAttack enemyAttack;
    private Collider2D hitboxCollider;
    private bool canHit = false;

    void Awake()
    {
        enemyAttack = GetComponentInParent<enemyAttack>();
        hitboxCollider = GetComponent<Collider2D>();

        if (hitboxCollider != null)
        {
            hitboxCollider.isTrigger = true;
            hitboxCollider.enabled = false;
        }
    }

    public void Activate()
    {
        if (hitboxCollider != null)
        {
            hitboxCollider.enabled = true;
            canHit = true; // Permitir hit

            if (showDebug)
                Debug.Log($"[{gameObject.name}] Hitbox ativada e pronta para causar dano");
        }
    }

    public void Deactivate()
    {
        if (hitboxCollider != null)
        {
            hitboxCollider.enabled = false;
            canHit = false;

            if (showDebug)
                Debug.Log($"[{gameObject.name}] Hitbox desativada");
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        // Usar OnTriggerStay para garantir detecção mesmo se o player já estava na área
        ProcessCollision(other);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        ProcessCollision(other);
    }

    void ProcessCollision(Collider2D other)
    {
        // Verificações básicas
        if (!canHit || !hitboxCollider.enabled) return;
        if (!other.CompareTag("Player")) return;

        // Pegar o componente avatarHealth
        avatarHealth playerHealth = other.GetComponent<avatarHealth>();

        if (playerHealth != null && enemyAttack != null)
        {
            // Causar dano usando o método correto
            int damage = enemyAttack.GetAttackDamage();
            playerHealth.TakeDamage(damage, transform.position);

            // Prevenir múltiplos hits no mesmo ataque
            canHit = false;

            if (showDebug)
                Debug.Log($"[{gameObject.name}] DANO CAUSADO: {damage} ao player!");
        }
        else if (showDebug)
        {
            if (playerHealth == null)
                Debug.LogError($"[{gameObject.name}] Player não tem avatarHealth!");
            if (enemyAttack == null)
                Debug.LogError($"[{gameObject.name}] enemyAttack não encontrado no pai!");
        }
    }

    void OnDrawGizmos()
    {
        if (hitboxCollider == null) return;

        Gizmos.color = (hitboxCollider != null && hitboxCollider.enabled) ?
            Color.red : new Color(1f, 0.5f, 0f, 0.3f);

        if (hitboxCollider is BoxCollider2D box)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(box.offset, box.size);
        }
    }
}