using UnityEngine;
using UnityEngine.Events;

public class avatarEnergy : MonoBehaviour
{
    [Header("=== Energia ===")]
    [SerializeField] private float maxEnergy = 100f;
    [SerializeField] private float currentEnergy;
    [SerializeField] private float energyRegenRate = 5f;
    [SerializeField] private float energyCostPerAttack = 25f;

    [Header("=== Ataque ===")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private float attackCooldown = 0.3f;
    [SerializeField] private float projectileSpeed = 10f;

    [Header("=== Projéteis (Prefabs) ===")]
    [SerializeField] private GameObject cortePrefab;
    [SerializeField] private GameObject quinaPrefab;
    [SerializeField] private GameObject luaPrefab;

    [Header("=== Sistema de Rotação ===")]
    [SerializeField] private int currentRotation = 0; // 0, 1, 2, 3

    [Header("=== Input ===")]
    [SerializeField] private KeyCode corteKey = KeyCode.I;
    [SerializeField] private KeyCode quinaKey = KeyCode.O;
    [SerializeField] private KeyCode luaKey = KeyCode.P;
    [SerializeField] private KeyCode rotateKey = KeyCode.U;

    [Header("=== Eventos ===")]
    public UnityEvent<float, float> OnEnergyChanged;
    public UnityEvent<int> OnRotationChanged;

    // Estados
    private float lastAttackTime;
    private bool isFacingRight = true;

    // Propriedades públicas
    public float CurrentEnergy => currentEnergy;
    public float MaxEnergy => maxEnergy;
    public int CurrentRotation => currentRotation;

    void Start()
    {
        currentEnergy = maxEnergy;
        OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
    }

    void Update()
    {
        // Regenerar energia
        RegenerateEnergy();

        // Detectar direção do player
        UpdateFacingDirection();

        // Input de rotação
        if (Input.GetKeyDown(rotateKey))
        {
            RotateSymbols();
        }

        // Input de ataques
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            if (Input.GetKeyDown(corteKey))
                FireProjectile(SymbolType.Corte);
            else if (Input.GetKeyDown(quinaKey))
                FireProjectile(SymbolType.Quina);
            else if (Input.GetKeyDown(luaKey))
                FireProjectile(SymbolType.Lua);
        }
    }

    void UpdateFacingDirection()
    {
        // Detecta direção baseado na escala ou velocidade
        if (transform.localScale.x != 0)
        {
            isFacingRight = transform.localScale.x > 0;
        }
    }

    void RegenerateEnergy()
    {
        if (currentEnergy < maxEnergy)
        {
            currentEnergy += energyRegenRate * Time.deltaTime;
            currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);
            OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
        }
    }

    void RotateSymbols()
    {
        currentRotation = (currentRotation + 1) % 4;
        OnRotationChanged?.Invoke(currentRotation);
        Debug.Log($"Rotação: {currentRotation * 90}°");
    }

    void FireProjectile(SymbolType type)
    {
        // Verificar energia
        if (currentEnergy < energyCostPerAttack)
        {
            Debug.Log("Sem energia!");
            return;
        }

        // Consumir energia
        currentEnergy -= energyCostPerAttack;
        currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);
        OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);

        // Escolher prefab correto
        GameObject prefab = GetProjectilePrefab(type);
        if (prefab == null) return;

        // Criar projétil
        Vector3 spawnPos = firePoint ? firePoint.position : transform.position + Vector3.right * 0.5f;
        GameObject projectile = Instantiate(prefab, spawnPos, Quaternion.identity);

        // Configurar projétil
        SimpleProjectile proj = projectile.GetComponent<SimpleProjectile>();
        if (proj == null)
            proj = projectile.AddComponent<SimpleProjectile>();

        // Calcular rotação real do símbolo
        int symbolRotation = GetSymbolRotation(type);

        // Inicializar projétil
        float direction = isFacingRight ? 1f : -1f;
        proj.Initialize(type, symbolRotation, direction, projectileSpeed);

        lastAttackTime = Time.time;

        Debug.Log($"Disparou: {type} com rotação {symbolRotation * 90}°");
    }

    GameObject GetProjectilePrefab(SymbolType type)
    {
        switch (type)
        {
            case SymbolType.Corte: return cortePrefab;
            case SymbolType.Quina: return quinaPrefab;
            case SymbolType.Lua: return luaPrefab;
            default: return null;
        }
    }

    int GetSymbolRotation(SymbolType type)
    {
        // Corte só tem 2 rotações (0 e 90 graus)
        if (type == SymbolType.Corte)
            return currentRotation % 2;

        // Quina e Lua têm 4 rotações
        return currentRotation;
    }

    public string GetCurrentSymbolName(SymbolType type)
    {
        int rotation = GetSymbolRotation(type);
        return $"{type} {rotation * 90}°";
    }
}

// Enum simplificado para tipos de símbolo
public enum SymbolType
{
    Corte,
    Quina,
    Lua
}