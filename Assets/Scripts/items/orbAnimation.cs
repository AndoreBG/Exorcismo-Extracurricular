using UnityEngine;
using UnityEngine.Rendering.Universal;

public class orbAnimation : MonoBehaviour
{
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

    void Start()
    {
        centerPosition = transform.position;
        randomOffset = Random.Range(0f, 2f * Mathf.PI);

        if (orbLight == null)
        {
            orbLight = GetComponentInChildren<Light2D>();
        }
    }

    void Update()
    {
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

        // Pulse de luz
        if (enableLightPulse && orbLight != null)
        {
            float intensity = Mathf.Lerp(minIntensity, maxIntensity,
                (Mathf.Sin(Time.time * lightPulseSpeed) + 1f) * 0.5f);
            orbLight.intensity = intensity;
        }
    }
}