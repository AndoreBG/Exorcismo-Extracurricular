using UnityEngine;
using System.Collections.Generic;

public class enemySymbolDisplay : MonoBehaviour
{
    [Header("=== Configurações ===")]
    [SerializeField] private enemy enemy;
    [SerializeField] private Transform symbolContainer;
    [SerializeField] private Vector3 containerOffset = new Vector3(0, 2f, 0);
    [SerializeField] private bool followEnemy = true;

    [Header("=== Prefab do Ícone ===")]
    [SerializeField] private GameObject symbolIconPrefab;
    [SerializeField] private float iconSize = 0.5f;
    [SerializeField] private float iconSpacing = 0.6f;

    [Header("=== Sprites dos Símbolos ===")]
    [SerializeField] private SymbolSpriteDatabase spriteDatabase;

    [Header("=== Sorting ===")]
    [SerializeField] private string sortingLayerName = "BackFX";
    [SerializeField] private int sortingOrder = 1;

    [Header("=== Animação ===")]
    [SerializeField] private bool animateRemoval = true;
    [SerializeField] private float removalDuration = 0.3f;
    [SerializeField] private AnimationType removalAnimation = AnimationType.ScaleDown;

    [Header("=== Debug ===")]
    [SerializeField] private bool showDebug = false;

    public enum AnimationType
    {
        ScaleDown,
        FadeOut,
        ScaleAndFade,
        Instant
    }

    private List<SymbolIcon> symbolIcons = new List<SymbolIcon>();

    [System.Serializable]
    private class SymbolIcon
    {
        public GameObject iconObject;
        public SpriteRenderer spriteRenderer;
        public enemy.SymbolRequirement symbol;
        public int index;
    }

    void Awake()
    {
        if (enemy == null)
            enemy = GetComponent<enemy>();

        if (enemy == null)
        {
            Debug.LogError($"[{gameObject.name}] EnemySymbolDisplay precisa de um componente Enemy!");
            enabled = false;
            return;
        }

        ValidateSymbolContainer();
    }

    void Start()
    {
        // Conectar aos eventos do Enemy - MUDANÇA: usar novo evento
        enemy.OnSpecificSymbolHit.AddListener(OnSpecificSymbolHit);
        enemy.OnDeath.AddListener(OnEnemyDeath);

        // Aguardar um frame para garantir que os símbolos foram gerados
        Invoke(nameof(CreateSymbolIcons), 0.1f);
    }

    void LateUpdate()
    {
        if (symbolContainer != null && followEnemy)
        {
            UpdateContainerPosition();
        }
    }

    void ValidateSymbolContainer()
    {
        if (symbolContainer == null)
        {
            GameObject containerObj = new GameObject("SymbolContainer");
            containerObj.transform.position = transform.position + containerOffset;
            symbolContainer = containerObj.transform;

            if (showDebug)
                Debug.Log($"[{gameObject.name}] Container criado automaticamente");
        }
        else
        {
            if (symbolContainer.IsChildOf(transform))
            {
                symbolContainer.SetParent(null);
                symbolContainer.position = transform.position + containerOffset;
            }

            if (showDebug)
                Debug.Log($"[{gameObject.name}] Usando container linkado: {symbolContainer.name}");
        }

        symbolContainer.rotation = Quaternion.identity;
    }

    void UpdateContainerPosition()
    {
        if (symbolContainer != null)
        {
            symbolContainer.position = transform.position + containerOffset;
            symbolContainer.rotation = Quaternion.identity;
        }
    }

    void CreateSymbolIcons()
    {
        if (enemy == null || symbolContainer == null)
        {
            Debug.LogError($"[{gameObject.name}] Enemy ou SymbolContainer não encontrado!");
            return;
        }

        ClearExistingIcons();

        var symbols = enemy.GetAllSymbols();

        if (symbols == null || symbols.Count == 0)
        {
            Debug.LogWarning($"[{gameObject.name}] Inimigo não tem símbolos para exibir!");
            return;
        }

        for (int i = 0; i < symbols.Count; i++)
        {
            CreateSymbolIcon(symbols[i], i);
        }

        ArrangeIconsVertically();

        if (showDebug)
            Debug.Log($"[{gameObject.name}] Criados {symbolIcons.Count} ícones de símbolos");
    }

    void ClearExistingIcons()
    {
        symbolIcons.Clear();

        foreach (Transform child in symbolContainer)
        {
            DestroyImmediate(child.gameObject);
        }
    }

    void CreateSymbolIcon(enemy.SymbolRequirement symbol, int index)
    {
        GameObject iconObj;

        if (symbolIconPrefab != null)
        {
            iconObj = Instantiate(symbolIconPrefab, symbolContainer);
        }
        else
        {
            iconObj = new GameObject($"Symbol_{index}");
            iconObj.transform.SetParent(symbolContainer);
        }

        iconObj.name = $"Symbol_{index}_{symbol.type}_{symbol.rotation}";
        iconObj.transform.localPosition = Vector3.zero;
        iconObj.transform.localRotation = Quaternion.identity;

        SpriteRenderer spriteRenderer = iconObj.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            spriteRenderer = iconObj.AddComponent<SpriteRenderer>();

        spriteRenderer.sortingLayerName = sortingLayerName;
        spriteRenderer.sortingOrder = sortingOrder;

        Sprite sprite = GetSpriteForSymbol(symbol);
        if (sprite != null)
        {
            spriteRenderer.sprite = sprite;
        }
        else
        {
            spriteRenderer.color = GetColorForSymbolType(symbol.type);
            iconObj.transform.localRotation = Quaternion.Euler(0, 0, symbol.rotation);
        }

        iconObj.transform.localScale = Vector3.one * iconSize;

        SymbolIcon symbolIcon = new SymbolIcon
        {
            iconObject = iconObj,
            spriteRenderer = spriteRenderer,
            symbol = symbol,
            index = index
        };

        symbolIcons.Add(symbolIcon);

        if (showDebug)
            Debug.Log($"[{gameObject.name}] Ícone criado: {symbol.type} {symbol.rotation}° (Index: {index})");
    }

    void ArrangeIconsVertically()
    {
        int count = symbolIcons.Count;
        float totalHeight = (count - 1) * iconSpacing;
        float startY = totalHeight / 2f;

        for (int i = 0; i < count; i++)
        {
            if (symbolIcons[i].iconObject != null)
            {
                Vector3 position = new Vector3(0, startY - (i * iconSpacing), 0);
                symbolIcons[i].iconObject.transform.localPosition = position;
            }
        }
    }

    Sprite GetSpriteForSymbol(enemy.SymbolRequirement symbol)
    {
        if (spriteDatabase != null)
        {
            return spriteDatabase.GetSprite(symbol.type, symbol.rotation);
        }
        return null;
    }

    Color GetColorForSymbolType(MagicType type)
    {
        switch (type)
        {
            case MagicType.Corte: return new Color(1f, 0.5f, 0.5f);
            case MagicType.Quina: return new Color(0.5f, 0.5f, 1f);
            case MagicType.Lua: return new Color(1f, 1f, 0.5f);
            default: return Color.white;
        }
    }

    // MUDANÇA: Novo método para remover símbolo específico
    void OnSpecificSymbolHit(int symbolIndex)
    {
        if (symbolIndex >= 0 && symbolIndex < symbolIcons.Count)
        {
            RemoveSymbolIcon(symbolIcons[symbolIndex]);

            // Reorganizar os ícones restantes
            RearrangeRemainingIcons();

            if (showDebug)
                Debug.Log($"[{gameObject.name}] Símbolo {symbolIndex} removido");
        }
    }

    void RearrangeRemainingIcons()
    {
        List<SymbolIcon> remainingIcons = new List<SymbolIcon>();

        foreach (var icon in symbolIcons)
        {
            if (icon.iconObject != null && icon.iconObject.activeSelf)
            {
                remainingIcons.Add(icon);
            }
        }

        int count = remainingIcons.Count;
        if (count > 0)
        {
            float totalHeight = (count - 1) * iconSpacing;
            float startY = totalHeight / 2f;

            for (int i = 0; i < count; i++)
            {
                if (remainingIcons[i].iconObject != null)
                {
                    Vector3 position = new Vector3(0, startY - (i * iconSpacing), 0);
                    remainingIcons[i].iconObject.transform.localPosition = position;
                }
            }
        }
    }

    void OnEnemyDeath()
    {
        foreach (var icon in symbolIcons)
        {
            if (icon.iconObject != null)
            {
                Destroy(icon.iconObject);
            }
        }

        symbolIcons.Clear();

        if (symbolContainer != null)
        {
            Destroy(symbolContainer.gameObject);
        }
    }

    void RemoveSymbolIcon(SymbolIcon icon)
    {
        if (icon == null || icon.iconObject == null)
            return;

        if (animateRemoval)
        {
            StartCoroutine(AnimateIconRemoval(icon));
        }
        else
        {
            icon.iconObject.SetActive(false);
        }
    }

    System.Collections.IEnumerator AnimateIconRemoval(SymbolIcon icon)
    {
        float elapsed = 0f;
        Vector3 originalScale = icon.iconObject.transform.localScale;
        Color originalColor = icon.spriteRenderer != null ? icon.spriteRenderer.color : Color.white;

        while (elapsed < removalDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / removalDuration;

            if (icon.iconObject == null) break;

            switch (removalAnimation)
            {
                case AnimationType.ScaleDown:
                    icon.iconObject.transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);
                    break;

                case AnimationType.FadeOut:
                    if (icon.spriteRenderer != null)
                    {
                        Color newColor = originalColor;
                        newColor.a = Mathf.Lerp(1f, 0f, t);
                        icon.spriteRenderer.color = newColor;
                    }
                    break;

                case AnimationType.ScaleAndFade:
                    icon.iconObject.transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);
                    if (icon.spriteRenderer != null)
                    {
                        Color newColor = originalColor;
                        newColor.a = Mathf.Lerp(1f, 0f, t);
                        icon.spriteRenderer.color = newColor;
                    }
                    break;
            }

            yield return null;
        }

        if (icon.iconObject != null)
            icon.iconObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (enemy != null)
        {
            enemy.OnSpecificSymbolHit.RemoveListener(OnSpecificSymbolHit);
            enemy.OnDeath.RemoveListener(OnEnemyDeath);
        }
    }

    [ContextMenu("Refresh Display")]
    void RefreshDisplay()
    {
        ValidateSymbolContainer();
        ClearExistingIcons();
        CreateSymbolIcons();
    }

    [ContextMenu("Test Container Setup")]
    void TestContainerSetup()
    {
        if (symbolContainer == null)
        {
            Debug.LogError("❌ Symbol Container não está linkado!");
        }
        else
        {
            Debug.Log($"✅ Symbol Container linkado: {symbolContainer.name}");
            Debug.Log($"  → Posição: {symbolContainer.position}");
            Debug.Log($"  → Rotação: {symbolContainer.rotation.eulerAngles}");
            Debug.Log($"  → Filhos: {symbolContainer.childCount}");
            Debug.Log($"  → Sorting Layer: {sortingLayerName}");
            Debug.Log($"  → Order in Layer: {sortingOrder}");
        }
    }

    void OnDrawGizmosSelected()
    {
        if (symbolContainer != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(symbolContainer.position, 0.1f);

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, symbolContainer.position);

            if (Application.isPlaying && symbolIcons != null)
            {
                Gizmos.color = Color.green;
                foreach (var icon in symbolIcons)
                {
                    if (icon.iconObject != null)
                    {
                        Gizmos.DrawWireCube(icon.iconObject.transform.position, Vector3.one * iconSize * 0.5f);
                    }
                }
            }
        }
    }
}