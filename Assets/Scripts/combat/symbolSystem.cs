using UnityEngine;

[System.Serializable]
public enum MagicSymbol
{
    // Corte (2 variações)
    Corte, 
    Corte90,

    // Quina (4 variações)
    Quina,
    Quina90,
    Quina180,
    Quina270,

    // Lua (4 variações)
    Lua,
    Lua90,
    Lua180,
    Lua270
}

public static class symbolSystem
{
    // Mapear qual ataque + variação = qual símbolo
    public static MagicSymbol GetSymbolForAttack(int attackType, int variation)
    {
        // Attack 1 - Corte (2 variações)
        if (attackType == 1)
        {
            switch (variation)
            {
                case 0: return MagicSymbol.Corte;      // Horizontal
                case 1: return MagicSymbol.Corte90;    // Vertical
                default: return MagicSymbol.Corte;
            }
        }
        // Attack 2 - Quina (4 variações)
        else if (attackType == 2)
        {
            switch (variation)
            {
                case 0: return MagicSymbol.Quina;
                case 1: return MagicSymbol.Quina90;
                case 2: return MagicSymbol.Quina180;
                case 3: return MagicSymbol.Quina270;
                default: return MagicSymbol.Quina;
            }
        }
        // Attack 3 - Lua (4 variações)
        else if (attackType == 3)
        {
            switch (variation)
            {
                case 0: return MagicSymbol.Lua;
                case 1: return MagicSymbol.Lua90;
                case 2: return MagicSymbol.Lua180;
                case 3: return MagicSymbol.Lua270;
                default: return MagicSymbol.Lua;
            }
        }

        return MagicSymbol.Corte;
    }

    // Verificar se o símbolo do projétil corresponde ao símbolo necessário
    public static bool SymbolMatches(MagicSymbol projectileSymbol, MagicSymbol targetSymbol)
    {
        return projectileSymbol == targetSymbol;
    }

    // Obter o símbolo base (sem rotação)
    public static MagicSymbol GetBaseSymbol(MagicSymbol symbol)
    {
        string symbolName = symbol.ToString();

        if (symbolName.StartsWith("Corte"))
            return MagicSymbol.Corte;
        else if (symbolName.StartsWith("Quina"))
            return MagicSymbol.Quina;
        else if (symbolName.StartsWith("Lua"))
            return MagicSymbol.Lua;

        return symbol;
    }

    // Obter tipo de símbolo (1 = Corte, 2 = Quina, 3 = Lua)
    public static int GetSymbolType(MagicSymbol symbol)
    {
        string symbolName = symbol.ToString();

        if (symbolName.StartsWith("Corte"))
            return 1;
        else if (symbolName.StartsWith("Quina"))
            return 2;
        else if (symbolName.StartsWith("Lua"))
            return 3;

        return 0;
    }

    // Obter rotação do símbolo
    public static int GetSymbolRotation(MagicSymbol symbol)
    {
        string symbolName = symbol.ToString();

        if (symbolName.EndsWith("90"))
            return 90;
        else if (symbolName.EndsWith("180"))
            return 180;
        else if (symbolName.EndsWith("270"))
            return 270;

        return 0;
    }

    // Obter próxima rotação do símbolo
    public static MagicSymbol RotateSymbol(MagicSymbol symbol)
    {
        int type = GetSymbolType(symbol);
        int currentRotation = GetSymbolRotation(symbol);

        // Corte só tem 2 variações (0° e 90°)
        if (type == 1)
        {
            if (currentRotation == 0)
                return MagicSymbol.Corte90;
            else
                return MagicSymbol.Corte;
        }
        // Quina tem 4 variações
        else if (type == 2)
        {
            switch (currentRotation)
            {
                case 0: return MagicSymbol.Quina90;
                case 90: return MagicSymbol.Quina180;
                case 180: return MagicSymbol.Quina270;
                case 270: return MagicSymbol.Quina;
            }
        }
        // Lua tem 4 variações
        else if (type == 3)
        {
            switch (currentRotation)
            {
                case 0: return MagicSymbol.Lua90;
                case 90: return MagicSymbol.Lua180;
                case 180: return MagicSymbol.Lua270;
                case 270: return MagicSymbol.Lua;
            }
        }

        return symbol;
    }

    // Obter nome amigável do símbolo
    public static string GetSymbolDisplayName(MagicSymbol symbol)
    {
        int rotation = GetSymbolRotation(symbol);
        string baseName = GetBaseSymbol(symbol).ToString();

        if (rotation == 0)
            return baseName;
        else
            return $"{baseName} ({rotation}°)";
    }

    // Obter descrição visual do símbolo
    public static string GetSymbolVisualDescription(MagicSymbol symbol)
    {
        switch (symbol)
        {
            case MagicSymbol.Corte: return "Corte Horizontal";
            case MagicSymbol.Corte90: return "Corte Vertical";

            case MagicSymbol.Quina: return "Quina";
            case MagicSymbol.Quina90: return "Quina 90°";
            case MagicSymbol.Quina180: return "Quina 180°";
            case MagicSymbol.Quina270: return "Quina 270°";

            case MagicSymbol.Lua: return "Lua";
            case MagicSymbol.Lua90: return "Lua 90°";
            case MagicSymbol.Lua180: return "Lua 180°";
            case MagicSymbol.Lua270: return "Lua 270°";

            default: return symbol.ToString();
        }
    }
}