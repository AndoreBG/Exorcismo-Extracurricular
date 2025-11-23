using UnityEngine;

[CreateAssetMenu(fileName = "SymbolSprites", menuName = "Game/Symbol Sprites")]
public class SymbolSprites : ScriptableObject
{
    [Header("=== Corte (2 variações) ===")]
    public Sprite corte;
    public Sprite corte90;

    [Header("=== Quina (4 variações) ===")]
    public Sprite quina; 
    public Sprite quina90;
    public Sprite quina180;
    public Sprite quina270;

    [Header("=== Lua (4 variações) ===")]
    public Sprite lua;
    public Sprite lua90;
    public Sprite lua180; 
    public Sprite lua270; 

    // Método helper para obter sprite por símbolo
    public Sprite GetSpriteForSymbol(MagicSymbol symbol)
    {
        switch (symbol)
        {
            case MagicSymbol.Corte: return corte;
            case MagicSymbol.Corte90: return corte90;

            case MagicSymbol.Quina: return quina;
            case MagicSymbol.Quina90: return quina90;
            case MagicSymbol.Quina180: return quina180;
            case MagicSymbol.Quina270: return quina270;

            case MagicSymbol.Lua: return lua;
            case MagicSymbol.Lua90: return lua90;
            case MagicSymbol.Lua180: return lua180;
            case MagicSymbol.Lua270: return lua270;

            default: return null;
        }
    }
}