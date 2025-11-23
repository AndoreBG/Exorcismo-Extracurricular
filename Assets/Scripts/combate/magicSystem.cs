using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class magicSystem : MonoBehaviour
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

    [Header("=== Prefabs dos Projéteis ===")]
    [SerializeField] private GameObject cortePrefab;
    [SerializeField] private GameObject quinaPrefab;
    [SerializeField] private GameObject luaPrefab;

    [Header("=== Sprites Base (sem rotação) ===")]
    [SerializeField] private Sprite corteSprite;
    [SerializeField] private Sprite quinaSprite;
    [SerializeField] private Sprite luaSprite;

    [Header("=== Input ===")]
    [SerializeField] private KeyCode corteKey = KeyCode.I;
    [SerializeField] private KeyCode quinaKey = KeyCode.O;
    [SerializeField] private KeyCode luaKey = KeyCode.P;
    [SerializeField] private KeyCode rotateKey = KeyCode.U;

    [Header("=== Object Pool ===")]
    [SerializeField] private int poolSizePerType = 10;

    // Estado
    private int currentRotation = 0; // 0, 1, 2, 3
    private float lastAttackTime;
    private projectilePool pool;

    // Eventos
    public UnityEvent<float, float> OnEnergyChanged;
    public UnityEvent<int> OnRotationChanged;
    public UnityEvent OnEnergyDepleted;

    // Propriedades
    public float CurrentEnergy => currentEnergy;
    public float MaxEnergy => maxEnergy;
    public int CurrentRotation => currentRotation;

    void Start()
    {
        currentEnergy = maxEnergy;
        OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);

        InitializePool();
    }

    void InitializePool()
    {
        GameObject poolObj = new GameObject("ProjectilePool");
        // NÃO parentar ao player para evitar afetar projéteis ativos
        pool = poolObj.AddComponent<projectilePool>();

        pool.AddProjectileType(cortePrefab, poolSizePerType);
        pool.AddProjectileType(quinaPrefab, poolSizePerType);
        pool.AddProjectileType(luaPrefab, poolSizePerType);
    }

    void Update()
    {
        RegenerateEnergy();

        // Rotação
        if (Input.GetKeyDown(rotateKey))
        {
            Rotate();
        }

        // Ataques
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            if (Input.GetKeyDown(corteKey) && HasEnergy())
            {
                ShootProjectile(MagicType.Corte);
            }
            else if (Input.GetKeyDown(quinaKey) && HasEnergy())
            {
                ShootProjectile(MagicType.Quina);
            }
            else if (Input.GetKeyDown(luaKey) && HasEnergy())
            {
                ShootProjectile(MagicType.Lua);
            }
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

    void Rotate()
    {
        currentRotation = (currentRotation + 1) % 4;
        OnRotationChanged?.Invoke(currentRotation);

        int angle = GetRotationAngle(currentRotation);
        Debug.Log($"Rotação: {angle}° (sentido horário)");
    }

    bool HasEnergy()
    {
        if (currentEnergy < energyCostPerAttack)
        {
            OnEnergyDepleted?.Invoke();
            return false;
        }
        return true;
    }

    void ShootProjectile(MagicType type)
    {
        // Consumir energia
        currentEnergy -= energyCostPerAttack;
        currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);
        OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);

        // Obter prefab e sprite corretos
        GameObject prefab = GetPrefab(type);
        Sprite sprite = GetSprite(type);
        int rotation = GetRotationForType(type);

        if (prefab == null) return;

        // Pegar projétil do pool
        GameObject projectile = pool.GetProjectile(prefab);
        if (projectile == null) return;

        // Posicionar
        projectile.transform.position = firePoint.position;
        projectile.transform.rotation = Quaternion.identity;

        // Inicializar
        projectile proj = projectile.GetComponent<projectile>();
        if (proj != null)
        {
            float direction = transform.localScale.x > 0 ? 1f : -1f;
            proj.Initialize(direction, projectileSpeed, type, rotation, sprite);
        }

        lastAttackTime = Time.time;

        Debug.Log($"Disparou {type} com rotação {rotation}°");
    }

    GameObject GetPrefab(MagicType type)
    {
        switch (type)
        {
            case MagicType.Corte: return cortePrefab;
            case MagicType.Quina: return quinaPrefab;
            case MagicType.Lua: return luaPrefab;
            default: return null;
        }
    }

    Sprite GetSprite(MagicType type)
    {
        switch (type)
        {
            case MagicType.Corte: return corteSprite;
            case MagicType.Quina: return quinaSprite;
            case MagicType.Lua: return luaSprite;
            default: return null;
        }
    }

    // ← MUDANÇA: Retorna rotação NEGATIVA (sentido horário)
    int GetRotationForType(MagicType type)
    {
        // Corte só tem 2 variações (0° e -90°)
        if (type == MagicType.Corte)
        {
            int index = currentRotation % 2;
            return index == 0 ? 0 : -90;
        }
        // Quina e Lua tem 4 variações (0°, -90°, -180°, -270°)
        else
        {
            return GetRotationAngle(currentRotation);
        }
    }

    // ← NOVO: Método helper para converter índice em ângulo horário
    int GetRotationAngle(int rotationIndex)
    {
        switch (rotationIndex)
        {
            case 0: return 0;
            case 1: return -90;
            case 2: return -180;
            case 3: return -270; // ou +90, é a mesma coisa
            default: return 0;
        }
    }

    public void RestoreEnergy(float amount)
    {
        currentEnergy += amount;
        currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);
        OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
    }
}

public enum MagicType
{
    Corte,
    Quina,
    Lua
}