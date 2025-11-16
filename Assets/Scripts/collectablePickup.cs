using UnityEngine;
using UnityEngine.Events;

public class CollectablePickup : MonoBehaviour
{
    public enum CollectableType
    {
        // Gems
        GemSmall,
        GemMedium,
        GemLarge,

        // Orbs
        HealthOrb,
        EnergyOrb,

        // Quest Items
        QuestItemMain,
        QuestItemGeneric,
        PurificationKey
    }

    [Header("== Configuração Principal ==")]
    [SerializeField] private CollectableType collectableType;

    [Header("== Valores ==")]
    [SerializeField] private int value = 1; // Valor para gems, orbs e quantidade de vida/energia
    [SerializeField] private bool useDefaultValue = true; // Usar valor padrão baseado no tipo

    [Header("== Quest Items ==")]
    [SerializeField] private string itemID = ""; // ID único para itens de missão
    [SerializeField] private bool isPersistent = true; // Se deve salvar que foi coletado

    [Header("== Animação ==")]
    [SerializeField] private float rotationSpeed = 45f;
    [SerializeField] private float floatSpeed = 2f;
    [SerializeField] private float floatAmplitude = 0.2f;

    [Header("== Efeitos ==")]
    [SerializeField] private GameObject pickupEffect;
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private float soundVolume = 0.7f;

    [Header("== Eventos ==")]
    public UnityEvent OnCollected;

    private Vector3 startPosition;
    private int actualValue;

    void Start()
    {
        startPosition = transform.position;

        // Definir valor baseado no tipo
        SetupValue();

        // Verificar persistência para quest items
        if (IsQuestItem() && isPersistent && !string.IsNullOrEmpty(itemID))
        {
            if (PlayerPrefs.GetInt($"Item_{itemID}", 0) == 1)
            {
                Destroy(gameObject);
            }
        }
    }

    void SetupValue()
    {
        if (useDefaultValue)
        {
            switch (collectableType)
            {
                case CollectableType.GemSmall:
                    actualValue = 1;
                    break;
                case CollectableType.GemMedium:
                    actualValue = 5;
                    break;
                case CollectableType.GemLarge:
                    actualValue = 10;
                    break;
                default:
                    actualValue = value;
                    break;
            }
        }
        else
        {
            actualValue = value;
        }
    }

    void Update()
    {
        // Rotação
        if (rotationSpeed > 0)
        {
            transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
        }

        // Flutuação
        if (floatAmplitude > 0)
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Verificar se é o player
        if (other.CompareTag("Player") ||
            other.GetComponentInParent<avatarHealth>() != null)
        {
            Collect();
        }
    }

    void Collect()
    {
        bool collected = ProcessCollection();

        if (collected)
        {
            PlayEffects();
            OnCollected?.Invoke();
            Destroy(gameObject);
        }
    }

    bool ProcessCollection()
    {
        switch (collectableType)
        {
            // Gems
            case CollectableType.GemSmall:
            case CollectableType.GemMedium:
            case CollectableType.GemLarge:
                return CollectGem();

            // Orbs
            case CollectableType.HealthOrb:
                return CollectHealthOrb();

            case CollectableType.EnergyOrb:
                return CollectEnergyOrb();

            // Quest Items
            case CollectableType.QuestItemMain:
            case CollectableType.QuestItemGeneric:
            case CollectableType.PurificationKey:
                return CollectQuestItem();

            default:
                return false;
        }
    }

    bool CollectGem()
    {
        if (gemManager.Instance != null)
        {
            gemManager.Instance.AddGems(actualValue);

            // Mostrar animação na UI
            UIController ui = FindFirstObjectByType<UIController>();
            if (ui != null)
            {
                ui.ShowGemCollected(actualValue);
            }

            return true;
        }
        return false;
    }

    bool CollectHealthOrb()
    {
        avatarHealth player = FindFirstObjectByType<avatarHealth>();
        if (player != null)
        {
            // Só coletar se não estiver com vida cheia
            if (player.CurrentHealth < player.MaxHealth)
            {
                player.Heal(actualValue);
                return true;
            }
        }
        return false;
    }

    bool CollectEnergyOrb()
    {
        // Quando implementar o sistema de energia
        // PlayerEnergy energy = FindFirstObjectByType<PlayerEnergy>();
        // if (energy != null)
        // {
        //     if (energy.CurrentEnergy < energy.MaxEnergy)
        //     {
        //         energy.RestoreEnergy(actualValue);
        //         return true;
        //     }
        // }

        Debug.Log($"Orb de energia coletado! (+{actualValue} energia)");
        return true;
    }

    bool CollectQuestItem()
    {
        if (!string.IsNullOrEmpty(itemID))
        {
            if (isPersistent)
            {
                PlayerPrefs.SetInt($"Item_{itemID}", 1);
                PlayerPrefs.Save();
            }

            Debug.Log($"Item de missão coletado: {itemID}");

            // Adicionar lógica específica para cada tipo
            if (collectableType == CollectableType.PurificationKey)
            {
                // Lógica especial para chaves de purificação
                Debug.Log("Chave de purificação obtida!");
            }

            return true;
        }

        Debug.LogWarning("Item de missão sem ID configurado!");
        return false;
    }

    void PlayEffects()
    {
        // Efeito visual
        if (pickupEffect != null)
        {
            GameObject effect = Instantiate(pickupEffect, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }

        // Som
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position, soundVolume);
        }
    }

    bool IsQuestItem()
    {
        return collectableType == CollectableType.QuestItemMain ||
               collectableType == CollectableType.QuestItemGeneric ||
               collectableType == CollectableType.PurificationKey;
    }

    bool IsGem()
    {
        return collectableType == CollectableType.GemSmall ||
               collectableType == CollectableType.GemMedium ||
               collectableType == CollectableType.GemLarge;
    }

    // Métodos Helper
    public int GetValue() => actualValue;

    public static bool IsItemCollected(string itemID)
    {
        return PlayerPrefs.GetInt($"Item_{itemID}", 0) == 1;
    }

    public static void ResetItem(string itemID)
    {
        PlayerPrefs.DeleteKey($"Item_{itemID}");
        PlayerPrefs.Save();
    }

    // Para debug no editor
    void OnValidate()
    {
        // Esconder campo itemID se não for quest item
        if (!IsQuestItem())
        {
            itemID = "";
        }
    }
}