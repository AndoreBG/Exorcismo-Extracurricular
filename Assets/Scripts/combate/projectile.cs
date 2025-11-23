using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class projectile : MonoBehaviour
{
    [Header("Configurações")]
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private bool showDebug = false;

    private SpriteRenderer spriteRenderer;
    private Collider2D projectileCollider;
    private Rigidbody2D rb;
    private float speed;
    private Vector2 direction;
    private float spawnTime;

    private MagicType magicType;
    private int rotation;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
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

        if (spriteRenderer != null && sprite != null)
        {
            spriteRenderer.sprite = sprite;
            spriteRenderer.transform.localRotation = Quaternion.Euler(0, 0, rotation);
            spriteRenderer.flipX = dir < 0;
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

    void FixedUpdate()
    {
        if (rb != null)
        {
            rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);
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

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = false;
            spriteRenderer.transform.localRotation = Quaternion.identity;
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
}