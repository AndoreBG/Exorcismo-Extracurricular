using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    [Header("=== HUD Principal ===")]
    [SerializeField] private GameObject hudPanel;

    [Header("=== Barra de Vida ===")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private Image healthFill;
    [SerializeField] private Color healthFullColor = Color.green;
    [SerializeField] private Color healthLowColor = Color.red;

    [Header("=== Barra de Energia ===")]
    [SerializeField] private Slider energyBar;
    [SerializeField] private Image energyFill;
    [SerializeField] private Color energyFullColor = new Color(0.2f, 0.5f, 1f); // Azul
    [SerializeField] private Color energyLowColor = new Color(0.5f, 0.2f, 0.8f); // Roxo
    [SerializeField] private float energyLowThreshold = 25f;

    [Header("=== Contador de Gems ===")]
    [SerializeField] private TextMeshProUGUI gemsText;

    [Header("=== Debug ===")]
    [SerializeField] private bool showDebugLogs = false;

    // Referências dos componentes do player
    private avatarHealth playerHealth;
    private avatarEnergy playerEnergy;

    void Start()
    {
        InitializeUI();
        ConnectToPlayer();
    }

    void InitializeUI()
    {
        // Configurar estado inicial
        if (hudPanel != null)
            hudPanel.SetActive(true);

        // Configurar cores iniciais
        if (healthFill != null)
            healthFill.color = healthFullColor;

        if (energyFill != null)
            energyFill.color = energyFullColor;
    }

    void ConnectToPlayer()
    {
        // Encontrar componentes do player
        playerHealth = FindFirstObjectByType<avatarHealth>();
        playerEnergy = FindFirstObjectByType<avatarEnergy>();

        // Conectar eventos de vida
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged.AddListener(UpdateHealth);

            // Atualizar valores iniciais
            UpdateHealth(playerHealth.CurrentHealth, playerHealth.MaxHealth);
        }

        // Conectar eventos de energia
        if (playerEnergy != null)
        {
            playerEnergy.OnEnergyChanged.AddListener(UpdateEnergy);

            // Atualizar valores iniciais
            UpdateEnergy(playerEnergy.CurrentEnergy, playerEnergy.MaxEnergy);
        }
    }

    // ========== HEALTH BAR ==========

    public void UpdateHealth(int current, int max)
    {
        if (healthBar != null)
        {
            healthBar.maxValue = max;
            healthBar.value = current;

            // Mudar cor baseada na vida
            if (healthFill != null)
            {
                float healthPercent = (float)current / max;
                healthFill.color = Color.Lerp(healthLowColor, healthFullColor, healthPercent);
            }
        }
    }

    // ========== ENERGY BAR ==========

    public void UpdateEnergy(float current, float max)
    {
        if (energyBar != null)
        {
            energyBar.maxValue = max;
            energyBar.value = current;

            // Mudar cor baseada na energia
            if (energyFill != null)
            {
                if (current < energyLowThreshold)
                {
                    energyFill.color = energyLowColor;
                }
                else
                {
                    energyFill.color = energyFullColor;
                }
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
    }
}