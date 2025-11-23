using System.Collections.Generic;
using UnityEngine;

public class obstacleDamage : MonoBehaviour
{
    [Space]
    [Header("== Configurações de Dano ==")]
    [SerializeField] private int damageAmount = 1;
    [SerializeField] private float damageCooldown = 1f;

    [Space]
    [SerializeField] private DamageType damageType = DamageType.Touch;

    [Space]
    [SerializeField] private bool continuousDamage = false;

    public enum DamageType
    {
        Touch,      // Dano ao tocar
        StayOn,     // Dano por ficar em cima
        Periodic    // Dano periódico
    }

    private Dictionary<avatarHealth, float> avatarCooldowns = new Dictionary<avatarHealth, float>();

    // == DETECÇÃO DE COLISÃO ==============================================================================================
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (damageType == DamageType.Touch)
        {
            TryApplyDamage(collision.gameObject, collision.contacts[0].point);
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (continuousDamage || damageType == DamageType.StayOn || damageType == DamageType.Periodic)
        {
            TryApplyDamage(collision.gameObject, collision.contacts[0].point);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (damageType == DamageType.Touch)
        {
            Vector2 contactPoint = other.ClosestPoint(transform.position);
            TryApplyDamage(other.gameObject, contactPoint);
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (continuousDamage || damageType == DamageType.StayOn || damageType == DamageType.Periodic)
        {
            Vector2 contactPoint = other.ClosestPoint(transform.position);
            TryApplyDamage(other.gameObject, contactPoint);
        }
    }

    // == SISTEMA DE DANO ==================================================================================================
    void TryApplyDamage(GameObject target, Vector2 contactPoint)
    {
        // Procurar avatarHealth no objeto ou no pai
        avatarHealth avatar = target.GetComponentInParent<avatarHealth>();
        if (avatar == null)
            avatar = target.GetComponent<avatarHealth>();

        if (avatar == null) return;

        // Verificar se pode causar dano
        if (avatar.IsDead || avatar.IsInvulnerable) return;

        // Verificar cooldown
        if (avatarCooldowns.ContainsKey(avatar))
        {
            if (Time.time < avatarCooldowns[avatar]) return;
        }

        // Aplicar dano
        avatar.TakeDamage(damageAmount, contactPoint);

        // Atualizar cooldown
        avatarCooldowns[avatar] = Time.time + damageCooldown;
    }

    // Limpar referências nulas
    void LateUpdate()
    {
        if (Time.frameCount % 300 == 0) // A cada ~5 segundos
        {
            avatarCooldowns.RemoveWhere(kvp => kvp.Key == null);
        }
    }

    void OnDestroy()
    {
        avatarCooldowns.Clear();
    }
}

// Extension method para simplificar a limpeza do Dictionary
public static class DictionaryExtensions
{
    public static void RemoveWhere<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, System.Func<KeyValuePair<TKey, TValue>, bool> predicate)
    {
        var keysToRemove = new List<TKey>();

        foreach (var kvp in dictionary)
        {
            if (predicate(kvp))
                keysToRemove.Add(kvp.Key);
        }

        foreach (var key in keysToRemove)
        {
            dictionary.Remove(key);
        }
    }
}