using UnityEngine;
using System.Collections;

public class enemyAttack : MonoBehaviour
{
    [Header("=== Configurações de Ataque ===")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private int attackDamage = 1;

    [Header("=== Detecção de Player ===")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private enemyAnimator enemyAnimator;
    [SerializeField] private enemy enemy;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private attackHitbox hitbox;

    [Header("=== Debug ===")]
    [SerializeField] private bool showDebug = true;
    [SerializeField] private bool showGizmos = true;

    // Componentes
    private enemyMovement enemyMovement;
    // Estado
    private float nextAttackTime = 0f;
    private bool isAttacking = false;

    void Awake()
    {
        enemyMovement = GetComponent<enemyMovement>();

        // Encontrar AttackPoint
        if (attackPoint == null)
        {
            attackPoint = transform.Find("AttackPoint");

            if (attackPoint == null && showDebug)
            {
                Debug.LogError($"[{gameObject.name}] AttackPoint não encontrado!");
            }
        }

        // Encontrar hitbox
        if (attackPoint != null)
        {
            if (hitbox == null && showDebug)
            {
                Debug.LogWarning($"[{gameObject.name}] attackHitbox não encontrado no AttackPoint!");
            }
        }

        if (showDebug)
        {
            Debug.Log($"[{gameObject.name}] EnemyAttack inicializado:");
            Debug.Log($"  - AttackPoint: {attackPoint != null}");
            Debug.Log($"  - Hitbox: {hitbox != null}");
            Debug.Log($"  - EnemyMovement: {enemyMovement != null}");
            Debug.Log($"  - Layer do Player: {LayerMask.LayerToName(Mathf.RoundToInt(Mathf.Log(playerLayer.value, 2)))}");
        }
    }

    void Update()
    {
        if (enemy != null && enemy.IsDead) return;
        if (isAttacking) return;

        // Verificar se pode atacar
        if (Time.time >= nextAttackTime)
        {
            GameObject player = DetectPlayer();
            if (player != null)
            {
                if (showDebug)
                    Debug.Log($"[{gameObject.name}] Player detectado! Iniciando ataque...");

                StartCoroutine(AttackSequence());
            }
        }
    }

    GameObject DetectPlayer()
    {
        if (attackPoint == null) return null;

        // Determinar direção baseado no enemyMovement ou localScale
        Vector2 direction;
        if (enemyMovement != null)
        {
            direction = enemyMovement.IsFacingRight() ? Vector2.right : Vector2.left;
        }
        else
        {
            // Fallback para localScale
            direction = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        }

        // Fazer raycast
        RaycastHit2D hit = Physics2D.Raycast(attackPoint.position, direction, attackRange, playerLayer);

        // Debug do raycast
        if (showDebug)
        {
            Debug.DrawRay(attackPoint.position, direction * attackRange, hit.collider != null ? Color.green : Color.red, 0.1f);
        }

        // Verificar se acertou algo
        if (hit.collider != null)
        {
            if (hit.collider.CompareTag("Player"))
            {
                return hit.collider.gameObject;
            }
            else if (showDebug)
            {
                Debug.Log($"[{gameObject.name}] Raycast acertou {hit.collider.name} mas não é Player (tag: {hit.collider.tag})");
            }
        }

        return null;
    }

    IEnumerator AttackSequence()
    {
        isAttacking = true;
        nextAttackTime = Time.time + attackCooldown;

        if (showDebug)
            Debug.Log($"[{gameObject.name}] Iniciando sequência de ataque");

        // Parar movimento
        if (enemyMovement != null)
        {
            enemyMovement.SetActive(false);
        }

        // Tocar animação
        if (enemyAnimator != null)
        {
            enemyAnimator.TriggerAttack();
        }

        // Aguardar um momento para ativar a hitbox (simular o swing da arma)
        yield return new WaitForSeconds(0.3f);

        // Ativar hitbox
        ActivateAttackCollider();

        // Manter hitbox ativa por um breve momento
        yield return new WaitForSeconds(0.2f);

        // Desativar hitbox
        DeactivateAttackCollider();

        // Aguardar fim da animação
        yield return new WaitForSeconds(0.3f);

        // Aguardar pausa pós-ataque se configurada
        if (enemyAnimator != null && enemyAnimator.GetPostAttackPauseDuration() > 0)
        {
            yield return new WaitForSeconds(enemyAnimator.GetPostAttackPauseDuration());
        }

        // Retomar movimento
        if (enemyMovement != null && !enemy.IsDead)
        {
            enemyMovement.SetActive(true);
        }

        isAttacking = false;

        if (showDebug)
            Debug.Log($"[{gameObject.name}] Ataque completo - Próximo ataque disponível em: {Time.time + attackCooldown}");
    }

    public void ActivateAttackCollider()
    {
        if (hitbox != null)
        {
            hitbox.Activate();
            if (showDebug)
                Debug.Log($"[{gameObject.name}] Hitbox ATIVADA");
        }
        else if (showDebug)
        {
            Debug.LogError($"[{gameObject.name}] Tentou ativar hitbox mas ela é null!");
        }
    }

    public void DeactivateAttackCollider()
    {
        if (hitbox != null)
        {
            hitbox.Deactivate();
            if (showDebug)
                Debug.Log($"[{gameObject.name}] Hitbox DESATIVADA");
        }
    }

    public void OnAttackComplete()
    {
        // Mantido para compatibilidade com animation events
    }

    public int GetAttackDamage() => attackDamage;

    public void SetActive(bool active)
    {
        if (!active)
        {
            StopAllCoroutines();
            DeactivateAttackCollider();
            isAttacking = false;
        }
    }

    void OnDrawGizmos()
    {
        if (!showGizmos) return;

        if (attackPoint != null)
        {
            // Determinar direção
            Vector2 direction;
            if (Application.isPlaying && enemyMovement != null)
            {
                direction = enemyMovement.IsFacingRight() ? Vector2.right : Vector2.left;
            }
            else
            {
                direction = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
            }

            // Desenhar linha de detecção
            Gizmos.color = isAttacking ? Color.yellow : Color.red;
            Gizmos.DrawLine(attackPoint.position,
                          (Vector2)attackPoint.position + direction * attackRange);

            // Desenhar esfera no fim do range
            Gizmos.DrawWireSphere((Vector2)attackPoint.position + direction * attackRange, 0.2f);
        }
        else
        {
            // Se não tem attack point, desenhar do centro do objeto
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, 0.3f);
        }
    }

    // Método de teste
    [ContextMenu("Test Player Detection")]
    void TestPlayerDetection()
    {
        GameObject player = DetectPlayer();
        if (player != null)
        {
            Debug.Log($"✓ Player detectado: {player.name}");
        }
        else
        {
            Debug.Log($"✗ Player NÃO detectado");
            Debug.Log($"  - AttackPoint existe? {attackPoint != null}");
            Debug.Log($"  - PlayerLayer configurada? {playerLayer.value != 0}");

            // Tentar encontrar player manualmente
            GameObject manualPlayer = GameObject.FindGameObjectWithTag("Player");
            if (manualPlayer != null)
            {
                Debug.Log($"  - Player existe na cena: {manualPlayer.name} na posição {manualPlayer.transform.position}");
                Debug.Log($"  - Layer do Player: {manualPlayer.layer} ({LayerMask.LayerToName(manualPlayer.layer)})");
            }
            else
            {
                Debug.Log($"  - Nenhum GameObject com tag 'Player' encontrado na cena!");
            }
        }
    }

    [ContextMenu("Force Attack")]
    void ForceAttack()
    {
        if (!isAttacking)
        {
            StartCoroutine(AttackSequence());
        }
    }
}