using UnityEngine;

public class enemySymbols : MonoBehaviour
{
    [Header("=== Símbolos Necessários ===")]
    [SerializeField] private SymbolRequirement[] requiredSymbols;

    [System.Serializable]
    public class SymbolRequirement
    {
        public MagicType type;
        public int rotation; // 0, 90, 180, 270
        public bool isHit = false;
    }

    public bool TryHit(MagicType type, int rotation)
    {
        foreach (var symbol in requiredSymbols)
        {
            if (!symbol.isHit && symbol.type == type && symbol.rotation == rotation)
            {
                symbol.isHit = true;
                CheckIfDefeated();
                return true;
            }
        }
        return false;
    }

    void CheckIfDefeated()
    {
        bool allHit = true;
        foreach (var symbol in requiredSymbols)
        {
            if (!symbol.isHit)
            {
                allHit = false;
                break;
            }
        }

        if (allHit)
        {
            Debug.Log("Inimigo derrotado!");
            Destroy(gameObject);
        }
    }
}