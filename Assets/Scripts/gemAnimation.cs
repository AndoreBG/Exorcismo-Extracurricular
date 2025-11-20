using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class GemPhysics : MonoBehaviour
{
    [Space]
    [Header("=== Ground Layer ===")]
    [SerializeField] private LayerMask groundLayer;

    [Space]
    [Header("=== Configuração de Spawn ===")]
    [SerializeField] private float initialJumpForce = 5f;
    [SerializeField] private float randomHorizontalForce = 2f;

    [Space]
    [Header("=== Configuração de Quique ===")]
    [SerializeField] private float bounciness = 0.6f; // 0 = não quica, 1 = quica 100%
    [SerializeField] private float minBounceVelocity = 0.5f; // Velocidade mínima para parar de quicar

    [Space]
    [Header("=== Configuração de Rotação ===")]
    [SerializeField] private float initialRotationSpeed = 360f;
    [SerializeField] private float rotationDamping = 0.9f; // Quanto diminui a cada quique

    // Componentes
    private Rigidbody2D rb;
    private Collider2D col;

    // Estado
    private float currentRotationSpeed;
    private bool hasLanded = false;
    private int bounceCount = 0;

    void Awake()
    {
        // Pegar componentes
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        // Configurar Rigidbody2D
        rb.gravityScale = 2f; // Gravidade mais forte para queda mais natural
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.freezeRotation = true; // Controlar rotação manualmente

        // Garantir que tem colisor configurado
        col.isTrigger = false; // Precisa ser false para física funcionar
    }

    void Start()
    {
        // Aplicar impulso inicial
        ApplyInitialForce();

        // Iniciar rotação
        currentRotationSpeed = initialRotationSpeed;
    }

    void ApplyInitialForce()
    {
        // For�a para cima + um pouco para o lado (aleatório)
        float horizontalForce = Random.Range(-randomHorizontalForce, randomHorizontalForce);
        Vector2 force = new Vector2(horizontalForce, initialJumpForce);

        rb.AddForce(force, ForceMode2D.Impulse);
    }

    void Update()
    {
        // Aplicar rotação
        if (currentRotationSpeed > 0.1f)
        {
            transform.Rotate(0, 0, currentRotationSpeed * Time.deltaTime);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Verificar se colidiu com o chão (você pode usar tag ou layer)
        // Por enquanto, considera qualquer colisão como chão

        if (col.IsTouchingLayers(groundLayer) && !hasLanded)
        {
            hasLanded = true;
        }

        // Verificar se deve continuar quicando
        float impactVelocity = Mathf.Abs(rb.linearVelocity.y);

        if (impactVelocity > minBounceVelocity)
        {
            // Aplicar quique
            Vector2 newVelocity = rb.linearVelocity;
            newVelocity.y = impactVelocity * bounciness;
            rb.linearVelocity = newVelocity;

            // Reduzir rotação
            currentRotationSpeed *= rotationDamping;

            bounceCount++;
        }
        else
        {
            // Parar de quicar
            StopBouncing();
        }
    }

    void StopBouncing()
    {
        // Parar velocidade vertical
        rb.linearVelocity = new Vector2(0f, 0f);

        // Parar rota��o
        currentRotationSpeed = 0;

        // Opcional: mudar para Kinematic para economizar performance
        rb.bodyType = RigidbodyType2D.Kinematic;

    }
}