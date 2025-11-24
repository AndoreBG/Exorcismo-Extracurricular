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
    [SerializeField] private string attackIndexParam = "AttackIndex";

    [Header("=== Configurações ===")]
    [SerializeField] private float postAttackPauseDuration = 1f;

    [Header("=== Debug ===")]
    [SerializeField] private bool showDebug = false;

    void Awake()
    {
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
    }

    public void SetWalking(bool isWalking)
    {
        if (animator == null) return;
        animator.SetBool(walkingParam, isWalking);
    }

    public void TriggerHurt()
    {
        if (animator == null) return;
        animator.SetTrigger(isHurtParam);
    }

    public void TriggerDeath()
    {
        if (animator == null) return;
        animator.SetTrigger(isDeadParam);
    }

    public void TriggerAttack(int attackIndex = -1)
    {
        if (animator == null) return;

        if (attackIndex == -1)
        {
            attackIndex = Random.Range(0, 2);
        }

        animator.SetFloat(attackIndexParam, attackIndex);
        animator.SetTrigger(attackTriggerParam);

        if (showDebug)
            Debug.Log($"[{gameObject.name}] Trigger Attack: {attackIndex}");
    }

    // Animation Events - mantidos para compatibilidade mas não são mais necessários
    public void OnAttackHit()
    {
        SendMessage("ActivateAttackCollider", SendMessageOptions.DontRequireReceiver);
    }

    public void OnAttackEnd()
    {
        SendMessage("DeactivateAttackCollider", SendMessageOptions.DontRequireReceiver);
        SendMessage("OnAttackComplete", SendMessageOptions.DontRequireReceiver);
    }

    // Getters
    public float GetPostAttackPauseDuration() => postAttackPauseDuration;
    public bool IsInPostAttackPause() => false; // Removido para simplicidade
    public Animator GetAnimator() => animator;
}