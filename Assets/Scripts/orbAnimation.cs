using UnityEngine;
using UnityEngine.Rendering.Universal;

// ANDRÉ BOLANDIM GORZONI    
// CLARA MIRANDA ROBERTO
// JHONATAN FELIPE TORQUATO SANTOS
// MATHEUS RODRIGUES CARVALHO
// MIGUEL ARCANJO PELLIZZARI BALDO   
// LEONARDO TREVISAN

public class orbAnimation : MonoBehaviour
{
    [Space]
    [Header("=== Atração Gravitacional ===")]
    [SerializeField] private float attractionDistance = 3f;
    [SerializeField] private float gravitationalConstant = 5f;
    [SerializeField] private float orbMass = 0.1f;
    [SerializeField] private float playerMass = 1f;

    [Space]
    [Header("Movimento Orbital")]
    [SerializeField] private bool enableOrbitalMotion = true;
    [SerializeField] private float orbitRadius = 0.1f;
    [SerializeField] private float orbitSpeed = 3f;

    [Space]
    [Header("Flutuação Suave")]
    [SerializeField] private bool enableFloating = true;
    [SerializeField] private float floatSpeed = 1.5f;
    [SerializeField] private float floatAmplitude = 0.2f;

    [Space]
    [Header("Efeito de Luz")]
    [SerializeField] private Light2D orbLight;
    [SerializeField] private bool enableLightPulse = true;
    [SerializeField] private float lightPulseSpeed = 2f;
    [SerializeField] private float minIntensity = 0.5f;
    [SerializeField] private float maxIntensity = 1.5f;

    private Vector3 centerPosition;
    private float randomOffset;

    // Gravitação
    private Transform player;
    private Vector3 velocity = Vector3.zero;
    private Vector3 acceleration = Vector3.zero;

    void Start()
    {
        centerPosition = transform.position;
        randomOffset = Random.Range(0f, 2f * Mathf.PI);

        if (orbLight == null)
        {
            orbLight = GetComponentInChildren<Light2D>();
        }

        // Encontrar o player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void Update()
    {
        // Verificar distância do player
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);

            if (distance < attractionDistance && distance > 0.1f)
            {
                // Aplicar gravitação usando a mesma lógica do sistema solar
                Vector3 direction = player.position - transform.position;
                float currentDistance = direction.magnitude;

                // F = G * m1 * m2 / r²
                Vector3 gravitationalForce = gravitationalConstant * orbMass * playerMass / (currentDistance * currentDistance) * direction.normalized;

                // a = F / m
                acceleration = gravitationalForce / orbMass;
                velocity += acceleration * Time.deltaTime;
                transform.position += velocity * Time.deltaTime;
            }
            else if (distance <= 0.1f)
            {
                // Muito próximo - parar movimento
                velocity = Vector3.zero;
                acceleration = Vector3.zero;
            }
            else
            {
                // Fora do alcance - animação normal
                velocity = Vector3.zero;
                acceleration = Vector3.zero;

                Vector3 newPosition = centerPosition;

                // Movimento orbital
                if (enableOrbitalMotion)
                {
                    float angle = Time.time * orbitSpeed + randomOffset;
                    float x = Mathf.Cos(angle) * orbitRadius;
                    float z = Mathf.Sin(angle) * orbitRadius;
                    newPosition += new Vector3(x, 0, z);
                }

                // Flutuação
                if (enableFloating)
                {
                    float y = Mathf.Sin((Time.time + randomOffset) * floatSpeed) * floatAmplitude;
                    newPosition.y += y;
                }

                transform.position = newPosition;
                centerPosition = transform.position;
            }
        }
        else
        {
            // Sem player - animação normal
            Vector3 newPosition = centerPosition;

            if (enableOrbitalMotion)
            {
                float angle = Time.time * orbitSpeed + randomOffset;
                float x = Mathf.Cos(angle) * orbitRadius;
                float z = Mathf.Sin(angle) * orbitRadius;
                newPosition += new Vector3(x, 0, z);
            }

            if (enableFloating)
            {
                float y = Mathf.Sin((Time.time + randomOffset) * floatSpeed) * floatAmplitude;
                newPosition.y += y;
            }

            transform.position = newPosition;
        }

        // Pulse de luz
        if (enableLightPulse && orbLight != null)
        {
            float intensity = Mathf.Lerp(minIntensity, maxIntensity,
                (Mathf.Sin(Time.time * lightPulseSpeed) + 1f) * 0.5f);
            orbLight.intensity = intensity;
        }
    }
}