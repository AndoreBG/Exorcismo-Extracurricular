using UnityEngine;

public class enemyAnimator : MonoBehaviour
{
    [Header("=== Componentes ===")]
    [SerializeField] private Animator animator;

    [Header("=== Nomes dos Parâmetros ===")]
    [SerializeField] private string walkingParam = "Walking";
    [SerializeField] private string isHurtParam = "isHurt";
    [SerializeField] private string isDeadParam = "isDead";
    [SerializeField] private string attackTriggerParam = "Attack";
    [SerializeField] private string attackIndexParam = "AttackIndex"; // Para múltiplos ataques

    [Header("=== Debug ===")]
    [SerializeField] private bool showDebug = false;

    private int currentAttackIndex = 0;
    private int maxAttackVariations = 1;

    void Awake()
    {
        // Se não atribuiu, tentar encontrar no filho "Sprite"
        if (animator == null)
        {
            Transform spriteChild = transform.Find("Sprite");
            if (spriteChild != null)
                animator = spriteChild.GetComponent<Animator>();
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (animator == null)
        {
            Debug.LogWarning($"[{gameObject.name}] Animator não encontrado!");
        }
    }

    // ========== CONTROLE DE ANIMAÇÕES ==========

    public void SetWalking(bool isWalking)
    {
        if (animator == null) return;

        animator.SetBool(walkingParam, isWalking);

        if (showDebug)
            Debug.Log($"[{gameObject.name}] Walking: {isWalking}");
    }

    public void TriggerHurt()
    {
        if (animator == null) return;

        animator.SetTrigger(isHurtParam);

        if (showDebug)
            Debug.Log($"[{gameObject.name}] Trigger: Hurt");
    }

    public void TriggerDeath()
    {
        if (animator == null) return;

        animator.SetTrigger(isDeadParam);

        if (showDebug)
            Debug.Log($"[{gameObject.name}] Trigger: Death");
    }

    public void TriggerAttack(int attackIndex = -1)
    {
        if (animator == null) return;

        // Se não especificou índice, usar o atual e rotacionar
        if (attackIndex == -1)
        {
            attackIndex = currentAttackIndex;
            currentAttackIndex = (currentAttackIndex + 1) % Mathf.Max(1, maxAttackVariations);
        }

        // Set attack index (para Blend Trees ou múltiplas animações)
        animator.SetInteger(attackIndexParam, attackIndex);

        // Trigger attack
        animator.SetTrigger(attackTriggerParam);

        if (showDebug)
            Debug.Log($"[{gameObject.name}] Trigger: Attack (Index: {attackIndex})");
    }

    // ========== CONFIGURAÇÃO ==========

    public void SetMaxAttackVariations(int max)
    {
        maxAttackVariations = Mathf.Max(1, max);
    }

    // ========== ANIMATION EVENTS ==========
    // Estes métodos são chamados pelos Animation Events no Animator

    public void OnAttackHit()
    {
        // Chamado no frame em que o ataque deve causar dano
        SendMessage("ActivateAttackCollider", SendMessageOptions.DontRequireReceiver);

        if (showDebug)
            Debug.Log($"[{gameObject.name}] Animation Event: Attack Hit");
    }

    public void OnAttackEnd()
    {
        // Chamado quando a animação de ataque termina
        SendMessage("DeactivateAttackCollider", SendMessageOptions.DontRequireReceiver);
        SendMessage("OnAttackComplete", SendMessageOptions.DontRequireReceiver);

        if (showDebug)
            Debug.Log($"[{gameObject.name}] Animation Event: Attack End");
    }

    public void OnDeathComplete()
    {
        // Chamado quando a animação de morte termina
        if (showDebug)
            Debug.Log($"[{gameObject.name}] Animation Event: Death Complete");
    }

    // ========== GETTERS ==========

    public Animator GetAnimator() => animator;
    public bool IsPlaying(string stateName)
    {
        if (animator == null) return false;
        return animator.GetCurrentAnimatorStateInfo(0).IsName(stateName);
    }
}