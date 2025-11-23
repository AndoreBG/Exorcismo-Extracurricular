using UnityEngine;

public class questItemAnimation : MonoBehaviour
{
    [Space]
    [Header("Rotação Lenta")]
    [SerializeField] private bool enableRotation = true;
    [SerializeField] private float rotationSpeed = 30f;

    [Space]
    [Header("Flutuação Mística")]
    [SerializeField] private bool enableFloating = true;
    [SerializeField] private float floatSpeed = 1f;
    [SerializeField] private float floatAmplitude = 0.15f;

    [Space]
    [Header("Brilho")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private bool enableGlow = true;
    [SerializeField] private float glowSpeed = 1.5f;
    [SerializeField] private Color glowColor = Color.yellow;
    [SerializeField] private float glowIntensity = 0.3f;

    private Vector3 startPosition;
    private Color originalColor;

    void Start()
    {
        startPosition = transform.position;

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    void Update()
    {
        // Rotação
        if (enableRotation)
        {
            transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
        }

        // Flutuação
        if (enableFloating)
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }

        // Brilho
        if (enableGlow && spriteRenderer != null)
        {
            float glowAmount = (Mathf.Sin(Time.time * glowSpeed) + 1f) * 0.5f * glowIntensity;
            spriteRenderer.color = Color.Lerp(originalColor, glowColor, glowAmount);
        }
    }
}