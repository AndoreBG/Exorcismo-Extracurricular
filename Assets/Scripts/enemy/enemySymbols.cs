using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class enemySymbols : MonoBehaviour
{
    [Header("Símbolos do Inimigo")]
    [SerializeField] private List<MagicSymbol> requiredSymbols = new List<MagicSymbol>();
    [SerializeField] private Transform symbolsContainer;
    [SerializeField] private GameObject symbolUIPrefab;

    [Header("Eventos")]
    public UnityEvent<MagicSymbol> OnSymbolHit;
    public UnityEvent<MagicSymbol> OnWrongSymbol;
    public UnityEvent OnAllSymbolsDestroyed;

    private List<GameObject> symbolVisuals = new List<GameObject>();

    void Start()
    {
        GenerateRandomSymbols();
        DisplaySymbols();
    }

    void GenerateRandomSymbols()
    {
        // Gerar 1-3 símbolos aleatórios
        int symbolCount = Random.Range(1, 4);

        for (int i = 0; i < symbolCount; i++)
        {
            MagicSymbol randomSymbol = (MagicSymbol)Random.Range(0,
                System.Enum.GetValues(typeof(MagicSymbol)).Length);
            requiredSymbols.Add(randomSymbol);
        }
    }

    void DisplaySymbols()
    {
        // Criar UI dos símbolos acima do inimigo
        foreach (MagicSymbol symbol in requiredSymbols)
        {
            if (symbolUIPrefab != null && symbolsContainer != null)
            {
                GameObject symbolUI = Instantiate(symbolUIPrefab, symbolsContainer);
                // Configurar visual do símbolo
                symbolVisuals.Add(symbolUI);
            }
        }
    }

    public bool TryHitSymbol(MagicSymbol projectileSymbol)
    {
        // Verificar se o símbolo existe na lista
        if (requiredSymbols.Contains(projectileSymbol))
        {
            // Remover o símbolo
            int index = requiredSymbols.IndexOf(projectileSymbol);
            requiredSymbols.RemoveAt(index);

            // Remover visual
            if (index < symbolVisuals.Count)
            {
                Destroy(symbolVisuals[index]);
                symbolVisuals.RemoveAt(index);
            }

            OnSymbolHit?.Invoke(projectileSymbol);

            // Verificar se todos os símbolos foram destruídos
            if (requiredSymbols.Count == 0)
            {
                OnAllSymbolsDestroyed?.Invoke();
                Die();
            }

            return true;
        }
        else
        {
            OnWrongSymbol?.Invoke(projectileSymbol);
            return false;
        }
    }

    void Die()
    {
        // Lógica de morte do inimigo
        GetComponent<dropItem>()?.DropGems();
        Destroy(gameObject, 0.5f);
    }
}