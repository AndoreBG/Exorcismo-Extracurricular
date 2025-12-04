using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class avatarHealth : MonoBehaviour
{
    [Space]
    [Header("== Status ==")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private int currentHealth;

    [SerializeField] private float invulnerabilityDuration = 1f;
    [SerializeField] private float blinkInterval = 0.1f;

    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private float knockbackDuration = 0.2f;
    
    [Space]
    [Header("== Componentes ==")]
    [SerializeField] private Transform spriteTransform;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    [SerializeField] private avatarMovement avatarMovement;

    private bool isInvulnerable = false;
    private bool isDead = false;
    private bool isKnockback = false;

    [Space]
    [Header("== Eventos (HUD) ==")]
    public UnityEvent<int, int> OnHealthChanged; // currentHealth, maxHealth
    public UnityEvent<int> OnDamageTaken; // damageAmount
    public UnityEvent<int> OnHealed; // healAmount
    public UnityEvent OnDeath;
    public UnityEvent OnRespawn;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => isDead;
    public bool IsInvulnerable => isInvulnerable;


    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
    void Start()
    {
        currentHealth = maxHealth; // Inicializar vida
        OnHealthChanged?.Invoke(currentHealth, maxHealth); // Disparar evento inicial de vida que iguala a vida atual com a vida máxima
    }

    public void TakeDamage(int damageAmount, Vector2 damageSourcePosition = default)
    {
        // Verificar se pode receber dano
        if (isDead || isInvulnerable || damageAmount <= 0)
            return;

        // Aplicar dano
        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Disparar eventos
        OnDamageTaken?.Invoke(damageAmount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        // Verificar morte
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Aplicar efeitos de dano
            StartCoroutine(DamageEffects());

            // Aplicar knockback se houver posição da fonte de dano
            if (damageSourcePosition != default && rb != null)
            {
                ApplyKnockback(damageSourcePosition);
            }

            // Tocar animação de dano se existir
            if (animator != null)
            {
                animator.SetTrigger("isHurt");
            }
        }
    }

    public void TakeDamage(int damageAmount, GameObject damageSource)
    {
        if (damageSource != null)
        {
            TakeDamage(damageAmount, damageSource.transform.position);
        }
        else
        {
            TakeDamage(damageAmount);
        }
    }

    public void Heal(int healAmount)
    {
        if (isDead || healAmount <= 0)
            return;

        int previousHealth = currentHealth;
        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        int actualHealed = currentHealth - previousHealth;

        if (actualHealed > 0)
        {
            OnHealed?.Invoke(actualHealed);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        // Efeito visual de cura
        StartCoroutine(HealEffect());
    }

    public void HealFull()
    {
        Heal(maxHealth);
    }

    public void SetMaxHealth(int newMaxHealth, bool healToFull = false)
    {
        maxHealth = Mathf.Max(1, newMaxHealth);

        if (healToFull)
        {
            currentHealth = maxHealth;
        }
        else
        {
            currentHealth = Mathf.Min(currentHealth, maxHealth);
        }

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    void Die()
    {
        if (isDead)
            return;

        isDead = true;
        OnDeath?.Invoke();

        if (animator != null)
        {
            animator.SetTrigger("isDead");
        }


        // Iniciar sequência de morte
       StartCoroutine(DeathSequence());
    }

    public void Respawn(Vector3 respawnPosition)
    {
        // Resetar estados
        isDead = false;
        isInvulnerable = false;

        // Restaurar vida
        currentHealth = maxHealth;

        // Mover para posição de respawn
        transform.position = respawnPosition;

        // Resetar velocidade
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        // Resetar animação
        if (animator != null)
        {
            animator.SetBool("IsDead", false);
        }

        // Disparar eventos
        OnRespawn?.Invoke();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        // Dar invulnerabilidade temporária
        StartCoroutine(InvulnerabilityCoroutine());
    }

    void ApplyKnockback(Vector2 damageSourcePosition)
    {
        if (isKnockback || rb == null)
            return;

        StartCoroutine(KnockbackCoroutine(damageSourcePosition));
    }

    IEnumerator KnockbackCoroutine(Vector2 damageSourcePosition)
    {
        isKnockback = true;

        // Calcular direção do knockback (oposta à fonte de dano)
        Vector2 knockbackDirection = ((Vector2)transform.position - damageSourcePosition).normalized;

        // Aplicar força
        rb.linearVelocity = Vector2.zero; // Resetar velocidade atual
        rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockbackDuration);

        isKnockback = false;
    }

    IEnumerator DamageEffects()
    {
        // Iniciar invulnerabilidade
        StartCoroutine(InvulnerabilityCoroutine());

        yield return new WaitForSeconds(0.1f);
    }

    IEnumerator InvulnerabilityCoroutine()
    {
        isInvulnerable = true;

        // Efeito de piscar
        if (spriteRenderer != null)
        {
            float elapsedTime = 0f;

            while (elapsedTime < invulnerabilityDuration)
            {
                spriteRenderer.enabled = !spriteRenderer.enabled;
                yield return new WaitForSeconds(blinkInterval);
                elapsedTime += blinkInterval;
            }

            spriteRenderer.enabled = true;
        }
        else
        {
            yield return new WaitForSeconds(invulnerabilityDuration);
        }

        isInvulnerable = false;
    }

    IEnumerator HealEffect()
    {
        // TODO: Adicionar efeito visual de cura (ex: brilho, partículas, etc.)
        yield return new WaitForSeconds(0.2f);
    }

    IEnumerator DeathSequence()
    {
        // Aguardar animação de morte (ajuste o tempo conforme necessário)
        yield return new WaitForSeconds(10f);

        // Adicionar aqui o sistema de Game Over:
        // - Mostrar tela de game over
        // - Recarregar a cena
        // - Respawn automático
        // GameManager.Instance.ShowGameOver();
    }
}