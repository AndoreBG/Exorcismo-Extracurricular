using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;

public class enemy : MonoBehaviour
{
    [Header("=== Configuração de Símbolos ===")]
    [SerializeField] private SymbolGenerationMode symbolMode = SymbolGenerationMode.AutoGenerate;
    [SerializeField] private int symbolCount = 3;
    [SerializeField] private List<SymbolRequirement> requiredSymbols = new List<SymbolRequirement>();

    [Header("=== Regras de Geração ===")]
    [SerializeField] private bool allowDuplicateTypes = true; // Permite mesmo tipo, mas ângulos diferentes
    [SerializeField] private bool includeCorte = true;
    [SerializeField] private bool includeQuina = true;
    [SerializeField] private bool includeLua = true;

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
    private int symbolsHit = 0; // MUDANÇA: contador de símbolos acertados
    private Color originalColor;

    // Componentes
    private enemyAnimator enemyAnimator;
    private enemyMovement enemyMovement;
    private enemyAttack enemyAttack;

    // Eventos - MUDANÇA: novo evento com índice do símbolo acertado
    public UnityEvent<int, int> OnSymbolHit; // (símbolos acertados, total)
    public UnityEvent<int> OnSpecificSymbolHit; // NOVO: índice específico do símbolo acertado
    public UnityEvent OnWrongSymbol;
    public UnityEvent OnDeath;

    [System.Serializable]
    public enum SymbolGenerationMode
    {
        AutoGenerate,  // Gera automaticamente na criação
        Manual         // Usa a lista configurada manualmente
    }

    [System.Serializable]
    public class SymbolRequirement
    {
        public MagicType type;
        public int rotation;
        [HideInInspector] public bool isHit = false;

        public override string ToString() => $"{type} {rotation}°";

        // Para comparação de duplicatas
        public override bool Equals(object obj)
        {
            if (obj is SymbolRequirement other)
            {
                return type == other.type && rotation == other.rotation;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return (type, rotation).GetHashCode();
        }
    }

    // Propriedades
    public bool IsDead => isDead;
    public int RemainingSymbols => requiredSymbols.Count - symbolsHit; // MUDANÇA
    public int TotalSymbols => requiredSymbols.Count;
    public int GetSymbolsHit() => symbolsHit; // NOVO

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
        // Gerar símbolos se modo automático
        if (symbolMode == SymbolGenerationMode.AutoGenerate)
        {
            GenerateRandomSymbols();
        }

        if (showDebug)
        {
            Debug.Log($"[{gameObject.name}] Inimigo criado com {requiredSymbols.Count} símbolos");
            ShowRequiredSymbols();
        }
    }

    // ========== GERAÇÃO DE SÍMBOLOS ALEATÓRIOS ==========
    public bool IsSymbolHit(int index)
    {
        if (index >= 0 && index < requiredSymbols.Count)
        {
            return requiredSymbols[index].isHit;
        }
        return false;
    }

    void GenerateRandomSymbols()
    {
        requiredSymbols.Clear();
        symbolsHit = 0; // MUDANÇA: resetar contador

        if (symbolCount <= 0)
        {
            Debug.LogWarning($"[{gameObject.name}] Symbol Count deve ser maior que 0!");
            symbolCount = 1;
        }

        // Criar pool de símbolos possíveis
        List<SymbolRequirement> availableSymbols = CreateSymbolPool();

        if (availableSymbols.Count == 0)
        {
            Debug.LogError($"[{gameObject.name}] Nenhum tipo de símbolo habilitado! Ativando todos.");
            includeCorte = true;
            includeQuina = true;
            includeLua = true;
            availableSymbols = CreateSymbolPool();
        }

        // Verificar se tem símbolos suficientes
        int maxPossibleSymbols = allowDuplicateTypes ? int.MaxValue : availableSymbols.Count;
        int actualCount = Mathf.Min(symbolCount, maxPossibleSymbols);

        if (actualCount < symbolCount)
        {
            Debug.LogWarning($"[{gameObject.name}] Não há símbolos únicos suficientes! Gerando {actualCount} ao invés de {symbolCount}");
        }

        // Gerar símbolos únicos
        HashSet<SymbolRequirement> usedSymbols = new HashSet<SymbolRequirement>();

        for (int i = 0; i < actualCount; i++)
        {
            SymbolRequirement newSymbol;
            int attempts = 0;
            int maxAttempts = 100;

            do
            {
                newSymbol = availableSymbols[Random.Range(0, availableSymbols.Count)];
                attempts++;

                if (attempts > maxAttempts)
                {
                    Debug.LogError($"[{gameObject.name}] Não foi possível gerar símbolo único após {maxAttempts} tentativas!");
                    break;
                }
            }
            while (usedSymbols.Contains(newSymbol) && attempts < maxAttempts);

            if (attempts < maxAttempts)
            {
                // Criar nova instância para evitar referências compartilhadas
                SymbolRequirement symbolCopy = new SymbolRequirement
                {
                    type = newSymbol.type,
                    rotation = newSymbol.rotation,
                    isHit = false
                };

                requiredSymbols.Add(symbolCopy);
                usedSymbols.Add(newSymbol);
            }
        }

        if (showDebug)
        {
            Debug.Log($"[{gameObject.name}] ✓ Gerados {requiredSymbols.Count} símbolos únicos");
        }
    }

    List<SymbolRequirement> CreateSymbolPool()
    {
        List<SymbolRequirement> pool = new List<SymbolRequirement>();

        // Corte (2 variações: 0° e -90°)
        if (includeCorte)
        {
            pool.Add(new SymbolRequirement { type = MagicType.Corte, rotation = 0 });
            pool.Add(new SymbolRequirement { type = MagicType.Corte, rotation = -90 });
        }

        // Quina (4 variações)
        if (includeQuina)
        {
            pool.Add(new SymbolRequirement { type = MagicType.Quina, rotation = 0 });
            pool.Add(new SymbolRequirement { type = MagicType.Quina, rotation = -90 });
            pool.Add(new SymbolRequirement { type = MagicType.Quina, rotation = -180 });
            pool.Add(new SymbolRequirement { type = MagicType.Quina, rotation = -270 });
        }

        // Lua (4 variações)
        if (includeLua)
        {
            pool.Add(new SymbolRequirement { type = MagicType.Lua, rotation = 0 });
            pool.Add(new SymbolRequirement { type = MagicType.Lua, rotation = -90 });
            pool.Add(new SymbolRequirement { type = MagicType.Lua, rotation = -180 });
            pool.Add(new SymbolRequirement { type = MagicType.Lua, rotation = -270 });
        }

        return pool;
    }

    // ========== SISTEMA DE HIT - MODIFICADO ==========

    public bool TryHit(MagicType type, int rotation)
    {
        if (isDead) return false;

        // MUDANÇA: Procurar por QUALQUER símbolo que corresponda, não apenas o próximo
        for (int i = 0; i < requiredSymbols.Count; i++)
        {
            SymbolRequirement symbol = requiredSymbols[i];

            // Se já foi acertado, pular
            if (symbol.isHit) continue;

            // Verificar se corresponde
            if (symbol.type == type && symbol.rotation == rotation)
            {
                // ✓ ACERTOU!
                symbol.isHit = true;
                symbolsHit++;

                if (showDebug)
                    Debug.Log($"[{gameObject.name}] ✓ Símbolo correto! ({symbolsHit}/{requiredSymbols.Count}) - {type} {rotation}° [Índice: {i}]");

                // Eventos
                OnSymbolHit?.Invoke(symbolsHit, requiredSymbols.Count);
                OnSpecificSymbolHit?.Invoke(i); // NOVO: passa o índice específico

                FlashHit(hitCorrectColor);

                if (enemyAnimator != null)
                {
                    enemyAnimator.TriggerHurt();
                }

                // Verificar se todos foram acertados
                if (symbolsHit >= requiredSymbols.Count)
                {
                    Die();
                }

                return true;
            }
        }

        // ✗ ERROU - Nenhum símbolo corresponde
        if (showDebug)
        {
            Debug.Log($"[{gameObject.name}] ✗ Símbolo incorreto!");
            Debug.Log($"  Recebido: {type} {rotation}°");
            Debug.Log($"  Símbolos restantes:");
            for (int i = 0; i < requiredSymbols.Count; i++)
            {
                if (!requiredSymbols[i].isHit)
                {
                    Debug.Log($"    - {requiredSymbols[i]}");
                }
            }
        }

        OnWrongSymbol?.Invoke();
        FlashHit(hitWrongColor);

        return false;
    }

    // ========== MORTE ==========

    void Die()
    {
        if (isDead) return;

        isDead = true;

        if (showDebug)
            Debug.Log($"[{gameObject.name}] 💀 Todos os símbolos acertados! Inimigo derrotado!");

        OnDeath?.Invoke();

        if (enemyMovement != null)
            enemyMovement.SetActive(false);

        if (enemyAttack != null)
            enemyAttack.SetActive(false);

        if (enemyAnimator != null)
        {
            enemyAnimator.TriggerDeath();
        }

        DisableColliders();
        DropItems();
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

    // ========== MENU DE CONTEXTO (Para Testes) ==========

    [ContextMenu("Generate New Random Symbols")]
    public void RegenerateSymbols()
    {
        symbolsHit = 0;
        GenerateRandomSymbols();
        ShowRequiredSymbols();
    }

    [ContextMenu("Show Required Symbols")]
    void ShowRequiredSymbols()
    {
        Debug.Log($"=== [{gameObject.name}] Símbolos Necessários ===");
        Debug.Log($"Modo: {symbolMode}");
        Debug.Log($"Total: {requiredSymbols.Count} | Acertados: {symbolsHit}");
        Debug.Log("---");

        for (int i = 0; i < requiredSymbols.Count; i++)
        {
            string status = requiredSymbols[i].isHit ? "✓" : "○";
            Debug.Log($"{i + 1}. {status} {requiredSymbols[i]}");
        }
    }

    [ContextMenu("Clear Symbols")]
    void ClearSymbols()
    {
        requiredSymbols.Clear();
        symbolsHit = 0;
        Debug.Log($"[{gameObject.name}] Símbolos limpos");
    }

    [ContextMenu("Die Instantly")]
    void DebugDie() => Die();

    // ========== GETTERS - MODIFICADOS ==========

    public SymbolRequirement GetSymbol(int index)
    {
        if (index >= 0 && index < requiredSymbols.Count)
            return requiredSymbols[index];
        return null;
    }

    public List<SymbolRequirement> GetAllSymbols() => requiredSymbols;

    // NOVO: Obter apenas símbolos não acertados
    public List<SymbolRequirement> GetRemainingSymbols()
    {
        return requiredSymbols.Where(s => !s.isHit).ToList();
    }
}