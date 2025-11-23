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
    private magicSystem magicSystem;

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

        // Inicializar gems em 0
        UpdateGems(0);
    }

    void ConnectToPlayer()
    {
        // Encontrar componentes do player
        playerHealth = FindFirstObjectByType<avatarHealth>();
        magicSystem = FindFirstObjectByType<magicSystem>(); // ← MUDANÇA

        // Conectar eventos de vida
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged.AddListener(UpdateHealth);
            UpdateHealth(playerHealth.CurrentHealth, playerHealth.MaxHealth);

            if (showDebugLogs)
                Debug.Log("UIController conectado ao avatarHealth");
        }
        else
        {
            Debug.LogWarning("avatarHealth não encontrado!");
        }

        // Conectar eventos de energia
        if (magicSystem != null) // ← MUDANÇA
        {
            magicSystem.OnEnergyChanged.AddListener(UpdateEnergy); // ← MUDANÇA
            UpdateEnergy(magicSystem.CurrentEnergy, magicSystem.MaxEnergy); // ← MUDANÇA

            if (showDebugLogs)
                Debug.Log("UIController conectado ao MagicSystem");
        }
        else
        {
            Debug.LogWarning("MagicSystem não encontrado!");
        }
    }

    // ========== HEALTH BAR ==========

    public void UpdateHealth(int current, int max)
    {
        if (healthBar == null) return;

        healthBar.maxValue = max;
        healthBar.value = current;

        // Mudar cor baseada na vida
        if (healthFill != null)
        {
            float healthPercent = (float)current / max;
            healthFill.color = Color.Lerp(healthLowColor, healthFullColor, healthPercent);
        }

        if (showDebugLogs)
            Debug.Log($"Vida atualizada: {current}/{max}");
    }

    // ========== ENERGY BAR ==========

    public void UpdateEnergy(float current, float max)
    {
        if (energyBar == null) return;

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

        if (showDebugLogs)
            Debug.Log($"Energia atualizada: {current:F1}/{max}");
    }

    // ========== GEMS ==========

    public void UpdateGems(int totalGems)
    {
        if (gemsText != null)
        {
            gemsText.text = totalGems.ToString();
        }

        if (showDebugLogs)
            Debug.Log($"Gems atualizadas: {totalGems}");
    }

    // Método opcional para feedback visual ao coletar gem
    public void ShowGemCollected(int amount)
    {
        if (showDebugLogs)
            Debug.Log($"+ {amount} gem(s) coletada(s)");

        // Aqui você pode adicionar efeitos visuais/sonoros
        // Exemplo: animação de bounce no texto, partículas, som, etc.
    }

    // ========== MÉTODOS UTILITÁRIOS ==========

    // Caso precise acessar os valores atuais
    public float GetCurrentEnergy() => magicSystem?.CurrentEnergy ?? 0f;
    public float GetMaxEnergy() => magicSystem?.MaxEnergy ?? 100f;
    public int GetCurrentHealth() => playerHealth?.CurrentHealth ?? 0;
    public int GetMaxHealth() => playerHealth?.MaxHealth ?? 100;

    // Método para forçar atualização (útil para debug)
    [ContextMenu("Force Update UI")]
    public void ForceUpdateUI()
    {
        if (playerHealth != null)
            UpdateHealth(playerHealth.CurrentHealth, playerHealth.MaxHealth);

        if (magicSystem != null)
            UpdateEnergy(magicSystem.CurrentEnergy, magicSystem.MaxEnergy);

        Debug.Log("UI forçada a atualizar");
    }
}