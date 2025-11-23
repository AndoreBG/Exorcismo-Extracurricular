using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class enemy : MonoBehaviour
{
    [Header("=== Símbolos Necessários ===")]
    [SerializeField] private List<SymbolRequirement> requiredSymbols = new List<SymbolRequirement>();

    [Header("=== Visual Feedback ===")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float hitFlashDuration = 0.1f;
    [SerializeField] private Color hitCorrectColor = Color.green;
    [SerializeField] private Color hitWrongColor = Color.red;

    [Header("=== Drop ao Morrer ===")]
    [SerializeField] private GameObject dropItemPrefab;
    [SerializeField] private int dropAmount = 1;
    [SerializeField] private float deathDelay = 1f;

    [Header("=== Debug ===")]
    [SerializeField] private bool showDebug = false;

    // Estado
    private bool isDead = false;
    private int currentSymbolIndex = 0; // Próximo símbolo a ser acertado
    private Color originalColor;

    // Componentes
    private enemyAnimator enemyAnimator;
    private enemyMovement enemyMovement;
    private enemyAttack enemyAttack;

    // Eventos
    public UnityEvent<int, int> OnSymbolHit; // (acertados, total)
    public UnityEvent OnWrongSymbol;
    public UnityEvent OnDeath;

    [System.Serializable]
    public class SymbolRequirement
    {
        public MagicType type;
        public int rotation; // 0, -90, -180, -270
        [HideInInspector] public bool isHit = false;

        public override string ToString() => $"{type} {rotation}°";
    }

    // Propriedades
    public bool IsDead => isDead;
    public int RemainingSymbols => requiredSymbols.Count - currentSymbolIndex;
    public int TotalSymbols => requiredSymbols.Count;

    void Awake()
    {
        enemyAnimator = GetComponent<enemyAnimator>();
        enemyMovement = GetComponent<enemyMovement>();
        enemyAttack = GetComponent<enemyAttack>();

        if (spriteRenderer == null)
        {
            Transform spriteChild = transform.Find("Sprite");
            if (spriteChild != null)
                spriteRenderer = spriteChild.GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    void Start()
    {
        if (showDebug)
        {
            Debug.Log($"[{gameObject.name}] Inimigo criado com {requiredSymbols.Count} símbolos");
            ShowRequiredSymbols();
        }
    }

    // ========== SISTEMA DE HIT ==========

    public bool TryHit(MagicType type, int rotation)
    {
        if (isDead) return false;

        // Verificar se ainda há símbolos para acertar
        if (currentSymbolIndex >= requiredSymbols.Count)
        {
            if (showDebug)
                Debug.Log($"[{gameObject.name}] Todos os símbolos já foram acertados!");
            return false;
        }

        // Pegar o próximo símbolo necessário (em ordem)
        SymbolRequirement nextSymbol = requiredSymbols[currentSymbolIndex];

        // Verificar se é o símbolo correto
        if (nextSymbol.type == type && nextSymbol.rotation == rotation)
        {
            // ✓ ACERTOU!
            nextSymbol.isHit = true;
            currentSymbolIndex++;

            if (showDebug)
                Debug.Log($"[{gameObject.name}] ✓ Símbolo correto! ({currentSymbolIndex}/{requiredSymbols.Count}) - {type} {rotation}°");

            OnSymbolHit?.Invoke(currentSymbolIndex, requiredSymbols.Count);

            // Flash verde
            FlashHit(hitCorrectColor);

            // Verificar se completou todos os símbolos
            if (enemyAnimator != null && !(currentSymbolIndex >= requiredSymbols.Count))
            {
                // Animação de dano
                enemyAnimator.TriggerHurt();
            }
            else
            {
                Die();
            }

            return true;
        }
        else
        {
            // ✗ ERROU!
            if (showDebug)
            {
                Debug.Log($"[{gameObject.name}] ✗ Símbolo incorreto!");
                Debug.Log($"  Esperado: {nextSymbol}");
                Debug.Log($"  Recebido: {type} {rotation}°");
            }

            OnWrongSymbol?.Invoke();

            // Flash vermelho
            FlashHit(hitWrongColor);

            return false;
        }
    }

    // ========== MORTE ==========

    void Die()
    {
        if (isDead) return;

        isDead = true;

        if (showDebug)
            Debug.Log($"[{gameObject.name}] 💀 Todos os símbolos acertados! Inimigo derrotado!");

        // Parar movimento
        if (enemyMovement != null)
            enemyMovement.SetActive(false);

        // Parar ataque
        if (enemyAttack != null)
            enemyAttack.SetActive(false);

        // Animação de morte
        if (enemyAnimator != null)
        {
            enemyAnimator.TriggerDeath();
        }

        // Desativar colliders
        //DisableColliders();

        // Drop de itens
        DropItems();

        
        OnDeath?.Invoke();

        // Destruir após delay
        Destroy(gameObject, deathDelay);
    }

    void DisableColliders()
    {
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        foreach (var col in colliders)
        {
            col.enabled = false;
        }
    }

    void DropItems()
    {
        if (dropItemPrefab != null)
        {
            for (int i = 0; i < dropAmount; i++)
            {
                Vector3 dropPos = transform.position + new Vector3(
                    Random.Range(-0.5f, 0.5f),
                    Random.Range(0f, 0.5f),
                    0
                );
                Instantiate(dropItemPrefab, dropPos, Quaternion.identity);
            }
        }
    }

    // ========== VISUAL FEEDBACK ==========

    void FlashHit(Color flashColor)
    {
        if (spriteRenderer == null) return;

        StopAllCoroutines();
        StartCoroutine(FlashCoroutine(flashColor));
    }

    System.Collections.IEnumerator FlashCoroutine(Color flashColor)
    {
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(hitFlashDuration);
        spriteRenderer.color = originalColor;
    }

    // ========== SETUP RÁPIDO ==========

    [ContextMenu("Setup - 1 Símbolo Aleatório")]
    public void Setup1RandomSymbol()
    {
        requiredSymbols.Clear();
        requiredSymbols.Add(GenerateRandomSymbol());

        if (showDebug)
            ShowRequiredSymbols();
    }

    [ContextMenu("Setup - 2 Símbolos Aleatórios")]
    public void Setup2RandomSymbols()
    {
        requiredSymbols.Clear();
        requiredSymbols.Add(GenerateRandomSymbol());
        requiredSymbols.Add(GenerateRandomSymbol());

        if (showDebug)
            ShowRequiredSymbols();
    }

    [ContextMenu("Setup - 3 Símbolos Aleatórios")]
    public void Setup3RandomSymbols()
    {
        requiredSymbols.Clear();
        requiredSymbols.Add(GenerateRandomSymbol());
        requiredSymbols.Add(GenerateRandomSymbol());
        requiredSymbols.Add(GenerateRandomSymbol());

        if (showDebug)
            ShowRequiredSymbols();
    }

    [ContextMenu("Setup - 4 Símbolos Aleatórios")]
    public void Setup4RandomSymbols()
    {
        requiredSymbols.Clear();
        for (int i = 0; i < 4; i++)
        {
            requiredSymbols.Add(GenerateRandomSymbol());
        }

        if (showDebug)
            ShowRequiredSymbols();
    }

    [ContextMenu("Setup - 5 Símbolos Aleatórios")]
    public void Setup5RandomSymbols()
    {
        requiredSymbols.Clear();
        for (int i = 0; i < 5; i++)
        {
            requiredSymbols.Add(GenerateRandomSymbol());
        }

        if (showDebug)
            ShowRequiredSymbols();
    }

    SymbolRequirement GenerateRandomSymbol()
    {
        MagicType[] types = { MagicType.Corte, MagicType.Quina, MagicType.Lua };
        int[] rotations = { 0, -90, -180, -270 };

        MagicType randomType = types[Random.Range(0, types.Length)];
        int randomRotation = rotations[Random.Range(0, rotations.Length)];

        // Corte só tem 0 e -90
        if (randomType == MagicType.Corte)
        {
            randomRotation = Random.Range(0, 2) == 0 ? 0 : -90;
        }

        return new SymbolRequirement
        {
            type = randomType,
            rotation = randomRotation
        };
    }

    [ContextMenu("Show Required Symbols")]
    void ShowRequiredSymbols()
    {
        Debug.Log($"=== [{gameObject.name}] Símbolos Necessários ===");
        for (int i = 0; i < requiredSymbols.Count; i++)
        {
            string status = requiredSymbols[i].isHit ? "✓" : "○";
            string arrow = (i == currentSymbolIndex) ? "← PRÓXIMO" : "";
            Debug.Log($"{i + 1}. {status} {requiredSymbols[i]} {arrow}");
        }
    }

    [ContextMenu("Clear Symbols")]
    void ClearSymbols()
    {
        requiredSymbols.Clear();
        currentSymbolIndex = 0;
        Debug.Log($"[{gameObject.name}] Símbolos limpos");
    }

    [ContextMenu("Die Instantly")]
    void DebugDie() => Die();

    // ========== GETTERS ==========

    public SymbolRequirement GetNextSymbol()
    {
        if (currentSymbolIndex < requiredSymbols.Count)
            return requiredSymbols[currentSymbolIndex];
        return null;
    }

    public List<SymbolRequirement> GetAllSymbols() => requiredSymbols;
}