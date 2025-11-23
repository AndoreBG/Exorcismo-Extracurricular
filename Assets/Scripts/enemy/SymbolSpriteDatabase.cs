using UnityEngine;

[CreateAssetMenu(fileName = "SymbolSpriteDatabase", menuName = "Game/Symbol Sprite Database")]
public class SymbolSpriteDatabase : ScriptableObject
{
    [Header("=== Corte (2 variações) ===")]
    public Sprite corte0;
    public Sprite corte90;

    [Header("=== Quina (4 variações) ===")]
    public Sprite quina0;
    public Sprite quina90;
    public Sprite quina180;
    public Sprite quina270;

    [Header("=== Lua (4 variações) ===")]
    public Sprite lua0;
    public Sprite lua90;
    public Sprite lua180;
    public Sprite lua270;

    public Sprite GetSprite(MagicType type, int rotation)
    {
        switch (type)
        {
            case MagicType.Corte:
                return GetCorteSprite(rotation);

            case MagicType.Quina:
                return GetQuinaSprite(rotation);

            case MagicType.Lua:
                return GetLuaSprite(rotation);

            default:
                return null;
        }
    }

    Sprite GetCorteSprite(int rotation)
    {
        // Corte só tem 2 variações: 0° e -90°
        switch (rotation)
        {
            case 0:
                return corte0;

            case -90:
            case 270: // -90 convertido para positivo
                return corte90;

            default:
                Debug.LogWarning($"Rotação {rotation}° não esperada para Corte. Usando 0°.");
                return corte0;
        }
    }

    Sprite GetQuinaSprite(int rotation)
    {
        // Quina tem 4 variações: 0°, -90°, -180°, -270°
        switch (rotation)
        {
            case 0:
                return quina0;

            case -90:
            case 270: // -90 convertido para positivo
                return quina90;

            case -180:
            case 180:
                return quina180;

            case -270:
            case 90: // -270 convertido para positivo
                return quina270;

            default:
                Debug.LogWarning($"Rotação {rotation}° não esperada para Quina. Usando 0°.");
                return quina0;
        }
    }

    Sprite GetLuaSprite(int rotation)
    {
        // Lua tem 4 variações: 0°, -90°, -180°, -270°
        switch (rotation)
        {
            case 0:
                return lua0;

            case -90:
            case 270: // -90 convertido para positivo
                return lua90;

            case -180:
            case 180:
                return lua180;

            case -270:
            case 90: // -270 convertido para positivo
                return lua270;

            default:
                Debug.LogWarning($"Rotação {rotation}° não esperada para Lua. Usando 0°.");
                return lua0;
        }
    }

    // ========== MÉTODO HELPER PARA DEBUG ==========

    [ContextMenu("Test All Rotations")]
    void TestAllRotations()
    {
        Debug.Log("=== TESTE DE ROTAÇÕES ===");

        Debug.Log("--- CORTE ---");
        TestSymbol(MagicType.Corte, 0);
        TestSymbol(MagicType.Corte, -90);

        Debug.Log("--- QUINA ---");
        TestSymbol(MagicType.Quina, 0);
        TestSymbol(MagicType.Quina, -90);
        TestSymbol(MagicType.Quina, -180);
        TestSymbol(MagicType.Quina, -270);

        Debug.Log("--- LUA ---");
        TestSymbol(MagicType.Lua, 0);
        TestSymbol(MagicType.Lua, -90);
        TestSymbol(MagicType.Lua, -180);
        TestSymbol(MagicType.Lua, -270);
    }

    void TestSymbol(MagicType type, int rotation)
    {
        Sprite sprite = GetSprite(type, rotation);
        string result = sprite != null ? $"✓ {sprite.name}" : "✗ NULL";
        Debug.Log($"{type} {rotation}°: {result}");
    }

    // ========== VALIDAÇÃO ==========

    [ContextMenu("Validate Sprites")]
    void ValidateSprites()
    {
        Debug.Log("=== VALIDAÇÃO DE SPRITES ===");

        int missing = 0;

        // Corte
        if (corte0 == null) { Debug.LogWarning("⚠️ Corte 0° está vazio!"); missing++; }
        if (corte90 == null) { Debug.LogWarning("⚠️ Corte 90° está vazio!"); missing++; }

        // Quina
        if (quina0 == null) { Debug.LogWarning("⚠️ Quina 0° está vazio!"); missing++; }
        if (quina90 == null) { Debug.LogWarning("⚠️ Quina 90° está vazio!"); missing++; }
        if (quina180 == null) { Debug.LogWarning("⚠️ Quina 180° está vazio!"); missing++; }
        if (quina270 == null) { Debug.LogWarning("⚠️ Quina 270° está vazio!"); missing++; }

        // Lua
        if (lua0 == null) { Debug.LogWarning("⚠️ Lua 0° está vazio!"); missing++; }
        if (lua90 == null) { Debug.LogWarning("⚠️ Lua 90° está vazio!"); missing++; }
        if (lua180 == null) { Debug.LogWarning("⚠️ Lua 180° está vazio!"); missing++; }
        if (lua270 == null) { Debug.LogWarning("⚠️ Lua 270° está vazio!"); missing++; }

        if (missing == 0)
        {
            Debug.Log("✓ Todos os sprites estão atribuídos!");
        }
        else
        {
            Debug.LogWarning($"⚠️ {missing} sprite(s) faltando!");
        }
    }
}