using UnityEngine;
using UnityEngine.Events;

public class gemManager : MonoBehaviour
{
    [Header("== Configurações ==")]
    [SerializeField] private int currentGems = 0;
    [SerializeField] private int maxGems = 9999;

    [Header("== Referências ==")]
    [SerializeField] private UIController uiController;

    [Header("== Eventos ==")]
    public UnityEvent<int> OnGemsChanged;
    public UnityEvent<int> OnGemsAdded;
    public UnityEvent<int> OnGemsSpent;

    // Singleton simples
    private static gemManager instance;
    public static gemManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindFirstObjectByType<gemManager>();
            return instance;
        }
    }

    public int CurrentGems => currentGems;

    void Awake()
    {
        // Se já existe uma instância e não é esta, destruir
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    void Start()
    {
        // Carregar gems salvas
        LoadGems();

        // Atualizar UI
        UpdateUI();
    }

    public void AddGems(int amount)
    {
        if (amount <= 0) return;

        int previousGems = currentGems;
        currentGems += amount;
        currentGems = Mathf.Clamp(currentGems, 0, maxGems);

        int actualAdded = currentGems - previousGems;

        if (actualAdded > 0)
        {
            OnGemsAdded?.Invoke(actualAdded);
            OnGemsChanged?.Invoke(currentGems);
            UpdateUI();
            SaveGems();
        }
    }

    public bool SpendGems(int amount)
    {
        if (amount <= 0 || currentGems < amount)
            return false;

        currentGems -= amount;
        OnGemsSpent?.Invoke(amount);
        OnGemsChanged?.Invoke(currentGems);
        UpdateUI();
        SaveGems();

        return true;
    }

    public bool CanAfford(int amount)
    {
        return currentGems >= amount;
    }

    void UpdateUI()
    {
        if (uiController != null)
        {
            uiController.UpdateGems(currentGems);
        }
    }

    // ========== SAVE/LOAD ==========

    void SaveGems()
    {
        PlayerPrefs.SetInt("TotalGems", currentGems);
        PlayerPrefs.Save();
    }

    void LoadGems()
    {
        currentGems = PlayerPrefs.GetInt("TotalGems", 0);
    }

    public void ResetGems()
    {
        currentGems = 0;
        UpdateUI();
        SaveGems();
    }
}