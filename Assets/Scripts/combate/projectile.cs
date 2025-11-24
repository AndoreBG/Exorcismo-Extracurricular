using UnityEngine;
using UnityEngine.Events;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class projectile : MonoBehaviour
{
    [Header("Configurações")]
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private bool showDebug = false;

    [Header("=== Referências dos Sprites ===")]
    [SerializeField] private SpriteRenderer symbolSprite; // Sprite do símbolo (gira)
    [SerializeField] private GameObject magicEffectObject; // GameObject do efeito (não gira)
    [SerializeField] private Transform magicEffectTransform; // GameObject do efeito (não gira)
    [SerializeField] private Animator magicEffectAnimator; // Animator do efeito

    [Header("=== Configurações de Desativação ===")]
    [SerializeField] private float hitEffectSize = 5f; // Delay antes de desativar (para o som terminar)
    [SerializeField] private float deactivateDelay = 0f; // Delay antes de desativar (para o som terminar)
    [SerializeField] private bool hideOnDeactivate = true; // Se deve esconder visualmente antes de desativar

    [Header("=== Eventos ===")]
    public UnityEvent<MagicType, Vector3> OnCast; // Tipo e posição do lançamento
    public UnityEvent<MagicType, Vector3, bool> OnHit; // Tipo, posição e se acertou
    public UnityEvent<MagicType, Vector3> OnHitWall; // Tipo e posição da colisão com parede
    public UnityEvent<MagicType, Vector3> OnTimeout; // Tipo e posição quando expira o tempo

    private Collider2D projectileCollider;
    private Rigidbody2D rb;
    private float speed;
    private Vector2 direction;
    private float spawnTime;

    private MagicType magicType;
    private int rotation;

    // Guardar escalas originais
    private Vector3 originalEffectScale;
    private bool scaleInitialized = false;

    // Estado de desativação
    private bool isDeactivating = false;

    void Awake()
    {
        // Se não foi configurado no Inspector, tentar encontrar
        if (symbolSprite == null)
        {
            symbolSprite = GetComponent<SpriteRenderer>();
        }

        // Procurar o efeito mágico se não foi configurado
        if (magicEffectObject == null)
        {
            Transform effectTransform = transform.Find("MagicEffect");
            if (effectTransform != null)
            {
                magicEffectObject = effectTransform.gameObject;
                magicEffectAnimator = magicEffectObject.GetComponent<Animator>();
            }
        }

        // Guardar escala original do efeito
        if (magicEffectObject != null && !scaleInitialized)
        {
            originalEffectScale = magicEffectObject.transform.localScale;
            scaleInitialized = true;

            if (showDebug)
                Debug.Log($"Escala original do efeito salva: {originalEffectScale}");
        }

        projectileCollider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();

        if (projectileCollider != null)
        {
            projectileCollider.isTrigger = true;
        }

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }

    public void Initialize(float dir, float projectileSpeed, MagicType type, int rot, Sprite sprite)
    {
        transform.SetParent(null);

        direction = new Vector2(dir, 0).normalized;
        speed = projectileSpeed;

        magicType = type;
        rotation = rot;
        isDeactivating = false;

        // Configurar sprite do SÍMBOLO (que gira)
        if (symbolSprite != null && sprite != null)
        {
            symbolSprite.sprite = sprite;
            symbolSprite.transform.localRotation = Quaternion.Euler(0, 0, rotation);
            symbolSprite.flipX = dir < 0;
            symbolSprite.enabled = true; // Garantir que está visível
        }

        // Configurar efeito mágico (que NÃO gira)
        if (magicEffectObject != null)
        {
            magicEffectObject.SetActive(true); // Garantir que está ativo

            // Garantir que o efeito sempre fique com rotação zero (sem girar)
            magicEffectObject.transform.localRotation = Quaternion.identity;

            // Usar a escala original e apenas inverter X se necessário
            Vector3 effectScale = originalEffectScale;
            if (dir < 0)
            {
                effectScale.x = Mathf.Abs(originalEffectScale.x) * -1;
            }
            else
            {
                effectScale.x = Mathf.Abs(originalEffectScale.x);
            }
            magicEffectObject.transform.localScale = effectScale;

            // Ativar animação específica se tiver
            if (magicEffectAnimator != null)
            {
                string animationTrigger = GetAnimationTriggerForType(type);
                if (!string.IsNullOrEmpty(animationTrigger))
                {
                    magicEffectAnimator.SetTrigger(animationTrigger);
                }
            }

            if (showDebug)
                Debug.Log($"Efeito configurado - Escala: {effectScale}, Dir: {dir}");
        }

        if (projectileCollider != null)
        {
            projectileCollider.enabled = true;
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        spawnTime = Time.time;

        // EVENTO: Disparar OnCast
        OnCast?.Invoke(magicType, transform.position);

        if (showDebug)
            Debug.Log($"✓ Projétil inicializado: {type} {rotation}° | Evento OnCast disparado");
    }

    string GetAnimationTriggerForType(MagicType type)
    {
        switch (type)
        {
            case MagicType.Corte:
                return "PlayCorte";
            case MagicType.Quina:
                return "PlayQuina";
            case MagicType.Lua:
                return "PlayLua";
            default:
                return "";
        }
    }

    void FixedUpdate()
    {
        if (rb != null && !isDeactivating)
        {
            rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);
        }
    }

    void LateUpdate()
    {
        // Garantir que o efeito mágico NUNCA rotacione
        if (magicEffectObject != null)
        {
            magicEffectObject.transform.rotation = Quaternion.identity;
        }
    }

    void Update()
    {
        if (!isDeactivating && Time.time - spawnTime >= lifetime)
        {
            magicEffectTransform.transform.localScale = Vector3.one * hitEffectSize;
            magicEffectAnimator.SetTrigger("Hit");
            // EVENTO: Timeout
            OnTimeout?.Invoke(magicType, transform.position);

            if (showDebug)
                Debug.Log($"⏰ Projétil expirou | Evento OnTimeout disparado");

            Deactivate();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDeactivating) return; // Já está desativando, ignorar novas colisões

        if (showDebug)
            Debug.Log($"💥 Projétil colidiu com: {other.gameObject.name} (Tag: {other.tag})");

        if (other.CompareTag("Player"))
        {
            return;
        }

        // Colisão com inimigo
        if (other.CompareTag("Enemy"))
        {
            enemy enemy = other.GetComponentInParent<enemy>();
            if (enemy == null)
            {
                enemy = other.GetComponent<enemy>();
            }

            if (enemy != null)
            {
                bool hit = enemy.TryHit(magicType, rotation);

                if (showDebug)
                    Debug.Log(hit ? $"✓ Acertou! {magicType} {rotation}°" : $"✗ Errou! {magicType} {rotation}°");

                magicEffectTransform.transform.localScale = Vector3.one * hitEffectSize;
                magicEffectAnimator.SetTrigger("Hit");
                // EVENTO: Hit (com resultado)
                OnHit?.Invoke(magicType, transform.position, hit);
            }

            Deactivate();
            return;
        }

        // Colisão com obstáculos
        if (other.CompareTag("Wall") || other.CompareTag("Ground") || other.CompareTag("Obstacle"))
        {
            if (showDebug)
                Debug.Log($"→ Destruído por: {other.tag}");

            magicEffectTransform.transform.localScale = Vector3.one * hitEffectSize;
            magicEffectAnimator.SetTrigger("Hit");
            // EVENTO: Hit Wall
            OnHitWall?.Invoke(magicType, transform.position);

            Deactivate();
            return;
        }

        int otherLayer = other.gameObject.layer;
        string layerName = LayerMask.LayerToName(otherLayer);

        if (layerName == "Ground" || layerName == "Wall" || layerName == "Obstacle")
        {
            if (showDebug)
                Debug.Log($"→ Destruído por layer: {layerName}");

            magicEffectTransform.transform.localScale = Vector3.one * hitEffectSize;
            magicEffectAnimator.SetTrigger("Hit");
            // EVENTO: Hit Wall
            OnHitWall?.Invoke(magicType, transform.position);

            Deactivate();
        }
    }

    void Deactivate()
    {
        if (isDeactivating) return; // Evitar múltiplas desativações
        isDeactivating = true;

        // Parar movimento imediatamente
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        if (projectileCollider != null)
        {
            projectileCollider.enabled = false;
        }

        // Se tem delay, usar coroutine
        if (deactivateDelay > 0)
        {
            StartCoroutine(DelayedDeactivate());
        }
        else
        {
            CompleteDeactivation();
        }
    }

    IEnumerator DelayedDeactivate()
    {
        // Esconder visualmente se configurado
        if (hideOnDeactivate)
        {
            if (symbolSprite != null)
                symbolSprite.enabled = false;
        }

        // Aguardar o delay
        yield return new WaitForSeconds(deactivateDelay);

        // Completar desativação
        CompleteDeactivation();
    }

    void CompleteDeactivation()
    {
        projectilePool pool = FindFirstObjectByType<projectilePool>();
        if (pool != null)
        {
            pool.ReturnProjectile(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }

        // Resetar rotações e escalas
        if (symbolSprite != null)
        {
            symbolSprite.flipX = false;
            symbolSprite.transform.localRotation = Quaternion.identity;
            symbolSprite.enabled = true; // Reativar para próximo uso
        }

        // Restaurar para escala ORIGINAL
        if (magicEffectObject != null)
        {
            magicEffectObject.transform.localRotation = Quaternion.identity;
            magicEffectObject.transform.localScale = originalEffectScale;
            magicEffectObject.SetActive(true); // Reativar para próximo uso

            // Parar animação se tiver
            if (magicEffectAnimator != null)
            {
                magicEffectAnimator.Rebind();
            }
        }

        isDeactivating = false; // Resetar flag
    }

    [ContextMenu("Reset Original Effect Scale")]
    void ResetOriginalEffectScale()
    {
        if (magicEffectObject != null)
        {
            originalEffectScale = magicEffectObject.transform.localScale;
            scaleInitialized = true;
            Debug.Log($"Nova escala original salva: {originalEffectScale}");
        }
    }

    // Métodos para testar eventos no Editor
    [ContextMenu("Test OnCast Event")]
    void TestOnCastEvent()
    {
        OnCast?.Invoke(magicType, transform.position);
        Debug.Log("OnCast event triggered!");
    }

    [ContextMenu("Test OnHit Event (Success)")]
    void TestOnHitEventSuccess()
    {
        OnHit?.Invoke(magicType, transform.position, true);
        Debug.Log("OnHit event triggered (Success)!");
    }

    [ContextMenu("Test OnHit Event (Fail)")]
    void TestOnHitEventFail()
    {
        OnHit?.Invoke(magicType, transform.position, false);
        Debug.Log("OnHit event triggered (Fail)!");
    }

    void OnDrawGizmos()
    {
        if (projectileCollider != null && Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.2f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)(direction * 0.5f));
        }
    }

    void OnValidate()
    {
        // Se mudou o magicEffectObject no inspector, atualizar escala original
        if (magicEffectObject != null && !Application.isPlaying)
        {
            originalEffectScale = magicEffectObject.transform.localScale;
            scaleInitialized = true;
        }

        // Garantir que o delay não seja negativo
        deactivateDelay = Mathf.Max(0, deactivateDelay);
    }
}