using System.Collections;
using UnityEngine;

public class spriteEffect : MonoBehaviour
{
    [Space]
    [Header("== Componente ==")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    [Space]
    [Header("== Configurações de Cor ==")]
    [SerializeField] private Color damageColor = Color.red;
    [SerializeField] private float flashDuration = 1f;

    [Space]
    [Header("== Configurações de Cura ==")]
    [SerializeField] private Color healColor = Color.green;
    [SerializeField] private float healFlashDuration = 1f;

    private Color originalColor;
    private Coroutine currentFlashCoroutine;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    // Método público para ser chamado pelo evento OnDamaged
    public void FlashDamageColor()
    {
        Flash(damageColor, flashDuration);
    }

    // Método público para ser chamado pelo evento OnHealed
    public void FlashHealColor()
    {
        Flash(healColor, healFlashDuration);
    }

    // Método genérico para flash de cor
    public void Flash(Color color, float duration)
    {
        if (spriteRenderer == null) return;

        // Cancelar flash anterior se existir
        if (currentFlashCoroutine != null)
        {
            StopCoroutine(currentFlashCoroutine);
        }

        currentFlashCoroutine = StartCoroutine(FlashCoroutine(color, duration));
    }

    IEnumerator FlashCoroutine(Color flashColor, float duration)
    {
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(duration);
        spriteRenderer.color = originalColor;
        currentFlashCoroutine = null;
    }

    // Método para resetar cor
    public void ResetColor()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }

    // Método para mudança permanente de cor
    public void SetColor(Color newColor)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = newColor;
        }
    }
}