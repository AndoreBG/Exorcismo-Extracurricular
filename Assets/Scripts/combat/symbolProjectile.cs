using UnityEngine;

public class SymbolProjectile : MonoBehaviour
{
    [Header("Configurações")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 3f;

    [Header("Símbolo")]
    [SerializeField] private MagicSymbol symbol;
    [SerializeField] private SpriteRenderer symbolVisual;

    private float direction;
    private float spawnTime;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.freezeRotation = true;
        }

        if (symbolVisual == null)
        {
            symbolVisual = GetComponent<SpriteRenderer>();
        }
    }

    public void Initialize(float dir, MagicSymbol sym)
    {
        direction = dir;
        symbol = sym;
        spawnTime = Time.time;

        rb.linearVelocity = new Vector2(speed * direction, 0);

        // Ajustar visual baseado na direção
        if (symbolVisual != null && direction < 0)
        {
            symbolVisual.flipX = true;
        }

        // Rotacionar sprite baseado no símbolo
        int rotation = symbolSystem.GetSymbolRotation(symbol);
        transform.rotation = Quaternion.Euler(0, 0, rotation);
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
        // Verificar colisão com inimigo
        if (other.CompareTag("Enemy"))
        {
            // Tentar aplicar símbolo no inimigo
            enemySymbols enemy = other.GetComponent<enemySymbols>();
            if (enemy != null)
            {
                bool hit = enemy.TryHitSymbol(symbol);

                if (hit)
                {
                    // Efeito de acerto
                    Debug.Log($"Acertou símbolo: {symbol}!");
                }
                else
                {
                    // Efeito de erro
                    Debug.Log($"Símbolo incorreto: {symbol}");
                }
            }

            Deactivate();
        }
        else if (other.CompareTag("Wall") || other.CompareTag("Obstacle") || other.CompareTag("Ground"))
        {
            Deactivate();
        }
    }

    void Deactivate()
    {
        gameObject.SetActive(false);
        rb.linearVelocity = Vector2.zero;

        if (symbolVisual != null)
        {
            symbolVisual.flipX = false;
        }

        transform.rotation = Quaternion.identity;
    }

    public MagicSymbol GetSymbol() => symbol;
}