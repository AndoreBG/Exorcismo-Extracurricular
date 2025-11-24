using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class magicSystem : MonoBehaviour
{
    [Header("=== Animação ===")]
    [SerializeField] private Animator animator;
    [SerializeField] private string attackTrigger = "Attack"; // Nome do trigger no Animator
    [SerializeField] private string attackBlendParameter = "AttackBlend"; // Nome do parâmetro float do blend tree

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

    [Header("=== Debug ===")]
    [SerializeField] private bool showDebug = false;

    // Estado
    private int currentRotation = 0; // 0, 1, 2, 3
    private float lastAttackTime;
    private projectilePool pool;

    // Eventos
    public UnityEvent<float, float> OnEnergyChanged;
    public UnityEvent<int> OnRotationChanged;
    public UnityEvent OnEnergyDepleted;
    public UnityEvent<MagicType, int, Vector3> OnMagicCast;
    public UnityEvent OnAnyMagicCast;

    // Propriedades
    public float CurrentEnergy => currentEnergy;
    public float MaxEnergy => maxEnergy;
    public int CurrentRotation => currentRotation;

    void Awake()
    {
        // Tentar encontrar o Animator se não foi atribuído
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    void Start()
    {
        currentEnergy = maxEnergy;
        OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);

        InitializePool();
    }

    void InitializePool()
    {
        GameObject poolObj = new GameObject("ProjectilePool");
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

        if (showDebug)
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
        // NOVO: Tocar animação de ataque aleatória
        PlayRandomAttackAnimation();

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

        // Disparar eventos de uso de magia
        OnMagicCast?.Invoke(type, rotation, firePoint.position);
        OnAnyMagicCast?.Invoke();

        if (showDebug)
            Debug.Log($"Disparou {type} com rotação {rotation}°");
    }

    // NOVO: Método para tocar animação de ataque aleatória
    void PlayRandomAttackAnimation()
    {
        if (animator == null) return;

        // Gerar valor aleatório: 0 ou 1
        int randomAnimation = GetRandomBinary();

        // Configurar o valor do blend tree (0 = primeira animação, 1 = segunda animação)
        animator.SetFloat(attackBlendParameter, randomAnimation);

        // Disparar o trigger de ataque
        animator.SetTrigger(attackTrigger);

        if (showDebug)
            Debug.Log($"Tocando animação de ataque: {randomAnimation} (0=Anim1, 1=Anim2)");
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

    int GetRotationForType(MagicType type)
    {
        if (type == MagicType.Corte)
        {
            int index = currentRotation % 2;
            return index == 0 ? 0 : -90;
        }
        else
        {
            return GetRotationAngle(currentRotation);
        }
    }

    int GetRotationAngle(int rotationIndex)
    {
        switch (rotationIndex)
        {
            case 0: return 0;
            case 1: return -90;
            case 2: return -180;
            case 3: return -270;
            default: return 0;
        }
    }

    public void RestoreEnergy(float amount)
    {
        currentEnergy += amount;
        currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);
        OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
    }

    public int GetRandomBinary()
    {
        return Random.Range(0, 2);
    }

    public int GetRandomBinary(float chanceOfOne)
    {
        return Random.value < chanceOfOne ? 1 : 0;
    }

    public void ForceCastMagic(MagicType type)
    {
        if (!HasEnergy()) return;
        ShootProjectile(type);
    }

    public bool CanCastMagic()
    {
        return Time.time >= lastAttackTime + attackCooldown && currentEnergy >= energyCostPerAttack;
    }

    // ========== MÉTODOS DE TESTE ==========

    [ContextMenu("Test Attack Animation 0")]
    void TestAttackAnimation0()
    {
        if (animator != null)
        {
            animator.SetFloat(attackBlendParameter, 0);
            animator.SetTrigger(attackTrigger);
            Debug.Log("Testando animação 0");
        }
    }

    [ContextMenu("Test Attack Animation 1")]
    void TestAttackAnimation1()
    {
        if (animator != null)
        {
            animator.SetFloat(attackBlendParameter, 1);
            animator.SetTrigger(attackTrigger);
            Debug.Log("Testando animação 1");
        }
    }

    [ContextMenu("Test Random Attack Animation")]
    void TestRandomAttackAnimation()
    {
        PlayRandomAttackAnimation();
    }

    [ContextMenu("Test Random Binary")]
    void TestRandomBinary()
    {
        int result = GetRandomBinary();
        Debug.Log($"Random Binary: {result}");

        string results = "10 valores aleatórios: ";
        for (int i = 0; i < 10; i++)
        {
            results += GetRandomBinary() + " ";
        }
        Debug.Log(results);
    }

    [ContextMenu("Test Magic Cast Event")]
    void TestMagicCastEvent()
    {
        OnMagicCast?.Invoke(MagicType.Corte, 0, transform.position);
        OnAnyMagicCast?.Invoke();
        Debug.Log("Magic Cast events triggered!");
    }
}

public enum MagicType
{
    Corte,
    Quina,
    Lua
}