using UnityEngine;

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
    [SerializeField] private Animator magicEffectAnimator; // Animator do efeito

    private Collider2D projectileCollider;
    private Rigidbody2D rb;
    private float speed;
    private Vector2 direction;
    private float spawnTime;

    private MagicType magicType;
    private int rotation;

    // NOVO: Guardar escalas originais
    private Vector3 originalEffectScale;
    private bool scaleInitialized = false;

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

        // NOVO: Guardar escala original do efeito
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

        // Configurar sprite do SÍMBOLO (que gira)
        if (symbolSprite != null && sprite != null)
        {
            symbolSprite.sprite = sprite;
            symbolSprite.transform.localRotation = Quaternion.Euler(0, 0, rotation);
            symbolSprite.flipX = dir < 0;
        }

        // Configurar efeito mágico (que NÃO gira)
        if (magicEffectObject != null)
        {
            // Garantir que o efeito sempre fique com rotação zero (sem girar)
            magicEffectObject.transform.localRotation = Quaternion.identity;

            // MODIFICADO: Usar a escala original e apenas inverter X se necessário
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
                // Você pode ter diferentes animações para cada tipo
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

        if (showDebug)
            Debug.Log($"✓ Projétil inicializado: {type} {rotation}°");
    }

    string GetAnimationTriggerForType(MagicType type)
    {
        // Retorna o nome do trigger da animação baseado no tipo
        // Você pode customizar isso conforme suas animações
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
        if (rb != null)
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
        if (Time.time - spawnTime >= lifetime)
        {
            Deactivate();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
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
            }

            Deactivate();
            return;
        }

        // Colisão com obstáculos
        if (other.CompareTag("Wall") || other.CompareTag("Ground") || other.CompareTag("Obstacle"))
        {
            if (showDebug)
                Debug.Log($"→ Destruído por: {other.tag}");
            Deactivate();
            return;
        }

        int otherLayer = other.gameObject.layer;
        string layerName = LayerMask.LayerToName(otherLayer);

        if (layerName == "Ground" || layerName == "Wall" || layerName == "Obstacle")
        {
            if (showDebug)
                Debug.Log($"→ Destruído por layer: {layerName}");
            Deactivate();
        }
    }

    void Deactivate()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        if (projectileCollider != null)
        {
            projectileCollider.enabled = false;
        }

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
        }

        // MODIFICADO: Restaurar para escala ORIGINAL ao invés de Vector3.one
        if (magicEffectObject != null)
        {
            magicEffectObject.transform.localRotation = Quaternion.identity;
            magicEffectObject.transform.localScale = originalEffectScale; // <-- CORREÇÃO AQUI

            // Parar animação se tiver
            if (magicEffectAnimator != null)
            {
                magicEffectAnimator.Rebind();
            }
        }
    }

    // NOVO: Método para resetar a escala original (útil se você mudar no editor)
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

    // NOVO: Validar configuração no editor
    void OnValidate()
    {
        // Se mudou o magicEffectObject no inspector, atualizar escala original
        if (magicEffectObject != null && !Application.isPlaying)
        {
            originalEffectScale = magicEffectObject.transform.localScale;
            scaleInitialized = true;
        }
    }
}