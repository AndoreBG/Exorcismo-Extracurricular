using UnityEngine;
using UnityEngine.Events;

public class CollectablePickup : MonoBehaviour
{
    public enum CollectableType
    {
        GemSmall,
        GemMedium,
        GemLarge,
        HealthOrb,
        EnergyOrb,
        QuestItem
    }

    [Header("Configuração")]
    [SerializeField] private CollectableType collectableType;
    [SerializeField] private int value = 1;

    [Header("Quest Item")]
    [SerializeField] private string itemID = "";

    [Header("Efeitos")]
    [SerializeField] private AudioClip pickupSound;

    [Header("Eventos")]
    public UnityEvent OnCollected;

    private bool isCollected = false; // Previne coleta dupla

    void Start()
    {
        // Configurar valor baseado no tipo
        if (collectableType == CollectableType.GemSmall)
            value = 1;
        else if (collectableType == CollectableType.GemMedium)
            value = 5;
        else if (collectableType == CollectableType.GemLarge)
            value = 10;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Previne múltiplas coletas
        if (isCollected) return;

        // Verificar se é o player
        if (other.CompareTag("Player") || other.GetComponentInParent<avatarHealth>() != null)
        {
            Collect();
        }
    }

    void Collect()
    {
        isCollected = true; // Marca como coletado imediatamente

        // Processar coleta baseado no tipo
        switch (collectableType)
        {
            case CollectableType.GemSmall:
            case CollectableType.GemMedium:
            case CollectableType.GemLarge:
                AddGems();
                break;

            case CollectableType.HealthOrb:
                HealPlayer();
                break;

            case CollectableType.EnergyOrb:
                RestoreEnergy();
                break;

            case CollectableType.QuestItem:
                CollectQuestItem();
                break;
        }

        // Efeitos
        PlayEffects();

        // Evento
        OnCollected?.Invoke();

        // Destruir
        Destroy(gameObject);
    }

    void AddGems()
    {
        gemManager manager = FindFirstObjectByType<gemManager>();
        if (manager != null)
        {
            manager.AddGems(value);
        }
    }

    void HealPlayer()
    {
        avatarHealth player = FindFirstObjectByType<avatarHealth>();
        if (player != null)
        {
            player.Heal(value);
        }
    }

    void RestoreEnergy()
    {
        // Implementar quando tiver sistema de energia
        Debug.Log($"Energia +{value}");
    }

    void CollectQuestItem()
    {
        if (!string.IsNullOrEmpty(itemID))
        {
            PlayerPrefs.SetInt($"Item_{itemID}", 1);
            PlayerPrefs.Save();
        }
    }

    void PlayEffects()
    {
        // Som
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position, 0.5f);
        }
    }
}