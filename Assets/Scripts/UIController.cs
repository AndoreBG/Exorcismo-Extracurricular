using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    [Header("== HUD Principal ==")]
    [SerializeField] private GameObject hudPanel;

    [Header("== Barra de Vida ==")]
    [SerializeField] private Slider healthBar;

    [Header("== Barra de Energia ==")]
    [SerializeField] private Slider energyBar;
    [SerializeField] private Image energyFill;

    [Header("== Contador de Gems ==")]
    [SerializeField] private TextMeshProUGUI gemsText;
    [SerializeField] private Image gemIcon;

    [Header("== Configurações de Energia ==")]
    [SerializeField] private Color energyFullColor = Color.blue;
    [SerializeField] private Color energyLowColor = Color.red;
    [SerializeField] private float energyLowThreshold = 0.3f;

    void Start()
    {
        if (hudPanel != null)
            hudPanel.SetActive(true);
    }

    // ========== HEALTH ==========

    public void OnPlayerHealthChanged(int current, int max)
    {
        UpdateHealth(current, max);
    }

    public void UpdateHealth(int current, int max)
    {
        if (healthBar != null)
        {
            healthBar.maxValue = max;
            healthBar.value = current;
        }
    }

    public void UpdateHealthPercent(float percent)
    {
        if (healthBar != null)
        {
            healthBar.value = percent;
        }
    }

    // ========== ENERGY ==========

    public void UpdateEnergy(float current, float max)
    {
        if (energyBar != null)
        {
            energyBar.maxValue = max;
            energyBar.value = current;

            // Mudar cor do fill baseado na energia
            if (energyFill != null)
            {
                float energyPercent = current / max;
                energyFill.color = energyPercent > energyLowThreshold ?
                    energyFullColor : energyLowColor;
            }
        }
    }

    public void UpdateEnergyPercent(float percent)
    {
        if (energyBar != null)
        {
            energyBar.value = percent;

            // Mudar cor do fill
            if (energyFill != null)
            {
                energyFill.color = percent > energyLowThreshold ?
                    energyFullColor : energyLowColor;
            }
        }
    }

    // ========== GEMS ==========

    public void UpdateGems(int totalGems)
    {
        if (gemsText != null)
        {
            gemsText.text = totalGems.ToString();
        }
    }

    public void ShowGemCollected(int amount)
    {
        // Animação do ícone da gem ao coletar
        if (gemIcon != null)
        {
            Animator anim = gemIcon.GetComponent<Animator>();
            if (anim != null)
            {
                anim.SetTrigger("Collect");
            }
        }
    }
}