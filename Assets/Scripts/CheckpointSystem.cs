using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using UnityEditor.Animations;

[System.Serializable]
public class CheckpointData
{
    public Vector3 position;
    public int playerHealth;
    public float playerEnergy;
    public int gems;
    public int checkpointIndex;
    public string sceneName;

    public CheckpointData(Vector3 pos, int health, float energy, int gemCount, int index, string scene)
    {
        position = pos;
        playerHealth = health;
        playerEnergy = energy;
        gems = gemCount;
        checkpointIndex = index;
        sceneName = scene;
    }
}

[RequireComponent(typeof(gemManager))]
public class CheckpointSystem : MonoBehaviour
{
    public static CheckpointSystem Instance { get; private set; }

    [Header("=== Checkpoint Configuration ===")]
    [SerializeField] private List<Transform> checkpointsList = new List<Transform>();
    [SerializeField] private float checkpointActivationRange = 2f;
    [SerializeField] private bool showCheckpointNumbers = true;

    // Player references
    [Header("=== Player Components ===")]
    [SerializeField] private avatarHealth playerHealth;
    [SerializeField] private magicSystem playerMagic;
    [SerializeField] private avatarMovement playerMovement;
    [SerializeField] private Animator playerAnimator;
    private GameObject player;
    private gemManager gems;

    [Header("=== Visual Settings ===")]
    [SerializeField] private Color inactiveCheckpointColor = Color.red;
    [SerializeField] private Color activeCheckpointColor = Color.yellow;
    [SerializeField] private Color currentCheckpointColor = Color.green;

    [Header("=== Respawn Settings ===")]
    [SerializeField] private float respawnDelay = 2f;
    [SerializeField] private bool freezePlayerOnDeath = true;

    [Header("=== Game Over UI ===")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private UnityEngine.UI.Button retryButton;
    [SerializeField] private UnityEngine.UI.Button mainMenuButton;
    [SerializeField] private float gameOverDelay = 1.5f;

    [Header("=== Scene Settings ===")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private bool saveCheckpointsAcrossScenes = true;

    [Header("=== Effects ===")]
    [SerializeField] private GameObject checkpointActivatedEffect;
    [SerializeField] private GameObject respawnEffect;
    [SerializeField] private AudioClip checkpointSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip respawnSound;

    [Header("=== Events ===")]
    public UnityEvent<int> OnCheckpointActivated;
    public UnityEvent OnPlayerDeath;
    public UnityEvent OnPlayerRespawn;
    public UnityEvent OnGameOver;

    [Header("=== Debug ===")]
    [SerializeField] private bool showDebug = true; // ATIVADO POR PADRÃO
    [SerializeField] private bool showGizmos = true;

    // Checkpoint state
    private HashSet<int> activatedCheckpoints = new HashSet<int>();
    private CheckpointData lastCheckpoint;
    private int currentCheckpointIndex = -1;

    // State
    private bool isRespawning = false;
    private bool isDead = false;
    private AudioSource audioSource;

    void Awake()
    {
        Debug.Log("[CheckpointSystem] Awake iniciado");

        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;

        // Get gemManager reference
        gems = GetComponent<gemManager>();
        if (gems == null)
        {
            Debug.LogError("[CheckpointSystem] gemManager não encontrado no mesmo GameObject!");
        }

        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Validate checkpoints
        ValidateCheckpoints();

        // Setup UI
        SetupGameOverUI();

        Debug.Log($"[CheckpointSystem] Configurado com sucesso. GameOverPanel: {gameOverPanel != null}");
    }

    void Start()
    {
        Debug.Log("[CheckpointSystem] Start iniciado");

        // Find player
        StartCoroutine(FindPlayerDelayed());

        // Load saved checkpoint if exists
        LoadCheckpointData();

        Debug.Log($"[CheckpointSystem] {checkpointsList.Count} checkpoints na lista");
    }

    void Update()
    {
        if (player != null && !isDead && !isRespawning)
        {
            CheckNearbyCheckpoints();
        }

        // Debug commands
        if (showDebug)
        {
            if (Input.GetKeyDown(KeyCode.F9))
            {
                Debug.Log("[CheckpointSystem] F9 pressionado - Forçando morte do player");
                ForceKillPlayer();
            }

            if (Input.GetKeyDown(KeyCode.F10))
            {
                Debug.Log("[CheckpointSystem] F10 pressionado - Forçando respawn");
                ForceRespawn();
            }

            if (Input.GetKeyDown(KeyCode.F11))
            {
                DebugCheckpoints();
            }
        }
    }

    void ValidateCheckpoints()
    {
        checkpointsList.RemoveAll(item => item == null);

        if (checkpointsList.Count == 0)
        {
            Debug.LogWarning("[CheckpointSystem] AVISO: Nenhum checkpoint configurado na lista!");
        }
    }

    IEnumerator FindPlayerDelayed()
    {
        Debug.Log("[CheckpointSystem] Procurando player...");

        yield return null;

        FindPlayer();

        if (player == null)
        {
            Debug.LogWarning("[CheckpointSystem] Player não encontrado na primeira tentativa, tentando novamente...");
            yield return new WaitForSeconds(0.5f);
            FindPlayer();
        }
    }

    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            Debug.Log($"[CheckpointSystem] Player encontrado: {playerObj.name}");
            SetupPlayer(playerObj);
        }
        else
        {
            Debug.LogError("[CheckpointSystem] ERRO: Player com tag 'Player' não encontrado na cena!");

            // Tentar encontrar por componente
            avatarHealth health = FindFirstObjectByType<avatarHealth>();
            if (health != null)
            {
                Debug.Log($"[CheckpointSystem] Player encontrado por componente avatarHealth: {health.gameObject.name}");
                SetupPlayer(health.gameObject);
            }
        }
    }

    void SetupPlayer(GameObject playerObj)
    {
        player = playerObj;

        Debug.Log($"[CheckpointSystem] Componentes do player:");
        Debug.Log($"  - avatarHealth: {playerHealth != null}");
        Debug.Log($"  - magicSystem: {playerMagic != null}");
        Debug.Log($"  - avatarMovement: {playerMovement != null}");

        // IMPORTANTE: Conectar ao evento de morte
        if (playerHealth != null)
        {
            // Remover listener anterior se existir
            playerHealth.OnDeath.RemoveListener(OnPlayerDied);
            // Adicionar novo listener
            playerHealth.OnDeath.AddListener(OnPlayerDied);

            Debug.Log("[CheckpointSystem] ✓ Conectado ao evento OnDeath do player");
        }
        else
        {
            Debug.LogError("[CheckpointSystem] ERRO: avatarHealth não encontrado no player!");
        }

        // Se não tem checkpoint salvo e tem checkpoints na lista, usar o primeiro
        if (lastCheckpoint == null && checkpointsList.Count > 0 && checkpointsList[0] != null)
        {
            Debug.Log("[CheckpointSystem] Ativando primeiro checkpoint como spawn inicial");
            ActivateCheckpoint(0);
        }
    }

    void SetupGameOverUI()
    {
        if (gameOverPanel == null)
        {
            Debug.LogWarning("[CheckpointSystem] GameOverPanel não configurado! O sistema funcionará sem UI.");
        }
        else
        {
            gameOverPanel.SetActive(false);
            Debug.Log("[CheckpointSystem] GameOverPanel configurado e desativado");

            if (retryButton != null)
            {
                retryButton.onClick.RemoveAllListeners();
                retryButton.onClick.AddListener(OnRetryClicked);
                Debug.Log("[CheckpointSystem] Botão Retry configurado");
            }
            else
            {
                Debug.LogWarning("[CheckpointSystem] Botão Retry não configurado");
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.RemoveAllListeners();
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);
                Debug.Log("[CheckpointSystem] Botão Main Menu configurado");
            }
            else
            {
                Debug.LogWarning("[CheckpointSystem] Botão Main Menu não configurado");
            }
        }
    }

    void CheckNearbyCheckpoints()
    {
        for (int i = 0; i < checkpointsList.Count; i++)
        {
            if (checkpointsList[i] == null) continue;

            float distance = Vector3.Distance(player.transform.position, checkpointsList[i].position);

            if (distance < checkpointActivationRange && !activatedCheckpoints.Contains(i))
            {
                ActivateCheckpoint(i);
            }
        }
    }

    void ActivateCheckpoint(int index)
    {
        if (index < 0 || index >= checkpointsList.Count) return;
        if (checkpointsList[index] == null) return;

        currentCheckpointIndex = index;
        activatedCheckpoints.Add(index);

        SaveCheckpoint();

        if (checkpointActivatedEffect != null)
        {
            GameObject effect = Instantiate(checkpointActivatedEffect, checkpointsList[index].position, Quaternion.identity);
            Destroy(effect, 3f);
        }

        if (audioSource != null && checkpointSound != null)
        {
            audioSource.PlayOneShot(checkpointSound);
        }

        OnCheckpointActivated?.Invoke(index);

        Debug.Log($"[CheckpointSystem] ✓ Checkpoint {index} ativado: {checkpointsList[index].name}");
    }

    void SaveCheckpoint()
    {
        if (player == null) return;

        Vector3 savePos = currentCheckpointIndex >= 0 && currentCheckpointIndex < checkpointsList.Count
            ? checkpointsList[currentCheckpointIndex].position
            : player.transform.position;

        int health = playerHealth != null ? playerHealth.CurrentHealth : 3;
        float energy = playerMagic != null ? playerMagic.CurrentEnergy : 100f;
        int gemCount = gems != null ? gems.CurrentGems : 0;

        lastCheckpoint = new CheckpointData(
            savePos,
            health,
            energy,
            gemCount,
            currentCheckpointIndex,
            SceneManager.GetActiveScene().name
        );

        if (saveCheckpointsAcrossScenes)
        {
            SaveCheckpointData();
        }

        Debug.Log($"[CheckpointSystem] Checkpoint salvo - Posição: {savePos}");
    }

    // MÉTODO CRÍTICO - CHAMADO QUANDO O PLAYER MORRE
    public void OnPlayerDied()
    {
        Debug.Log("[CheckpointSystem] !!!!! OnPlayerDied CHAMADO !!!!!");

        if (isDead)
        {
            Debug.Log("[CheckpointSystem] Já está morto, ignorando...");
            return;
        }

        if (isRespawning)
        {
            Debug.Log("[CheckpointSystem] Já está respawnando, ignorando...");
            return;
        }

        isDead = true;

        Debug.Log("[CheckpointSystem] Processando morte do player...");

        // Som de morte
        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
            Debug.Log("[CheckpointSystem] Som de morte tocado");
        }

        // Congelar player
        if (freezePlayerOnDeath && playerMovement != null)
        {
            playerMovement.enabled = false;
            Debug.Log("[CheckpointSystem] Movimento do player congelado");
        }

        // Disparar evento
        OnPlayerDeath?.Invoke();

        // Mostrar Game Over
        Debug.Log($"[CheckpointSystem] Iniciando Game Over com delay de {gameOverDelay}s");
        StartCoroutine(ShowGameOverDelayed());
    }

    IEnumerator ShowGameOverDelayed()
    {
        Debug.Log("[CheckpointSystem] Aguardando antes de mostrar Game Over...");
        yield return new WaitForSeconds(gameOverDelay);

        Debug.Log("[CheckpointSystem] Mostrando Game Over agora");
        ShowGameOver();
    }

    void ShowGameOver()
    {
        Debug.Log($"[CheckpointSystem] ShowGameOver - Panel existe? {gameOverPanel != null}");

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Time.timeScale = 0f;
            Debug.Log("[CheckpointSystem] ✓ Game Over UI ativada e jogo pausado");
        }
        else
        {
            Debug.LogWarning("[CheckpointSystem] Sem Game Over UI - respawnando direto");
            StartCoroutine(RespawnSequence());
        }

        OnGameOver?.Invoke();
    }

    IEnumerator RespawnSequence()
    {
        if (isRespawning) yield break;

        Debug.Log("[CheckpointSystem] Iniciando sequência de respawn...");
        isRespawning = true;

        Time.timeScale = 1f;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        yield return new WaitForSeconds(respawnDelay);

        playerAnimator.Rebind();
        RespawnPlayer();

        isRespawning = false;
        isDead = false;
    }

    void RespawnPlayer()
    {
        if (lastCheckpoint == null)
        {
            Debug.LogError("[CheckpointSystem] ERRO: Sem checkpoint salvo! Criando checkpoint de emergência na posição atual.");

            if (player != null)
            {
                SaveCheckpoint();
            }
            else
            {
                Debug.LogError("[CheckpointSystem] ERRO CRÍTICO: Sem player e sem checkpoint!");
                return;
            }
        }

        if (player == null)
        {
            FindPlayer();
            if (player == null)
            {
                Debug.LogError("[CheckpointSystem] ERRO: Não foi possível encontrar o player para respawn!");
                return;
            }
        }

        if (playerHealth != null)
        {
            playerHealth.Respawn(lastCheckpoint.position);
            Debug.Log($"[CheckpointSystem] Player respawnado em {lastCheckpoint.position}");
        }
        else
        {
            player.transform.position = lastCheckpoint.position;
            Debug.Log($"[CheckpointSystem] Player movido para {lastCheckpoint.position} (sem avatarHealth)");
        }

        if (playerMagic != null)
        {
            float energyDiff = lastCheckpoint.playerEnergy - playerMagic.CurrentEnergy;
            if (energyDiff > 0)
                playerMagic.RestoreEnergy(energyDiff);
        }

        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        if (respawnEffect != null)
        {
            GameObject effect = Instantiate(respawnEffect, lastCheckpoint.position, Quaternion.identity);
            Destroy(effect, 3f);
        }

        if (audioSource != null && respawnSound != null)
        {
            audioSource.PlayOneShot(respawnSound);
        }

        OnPlayerRespawn?.Invoke();

        Debug.Log("[CheckpointSystem] ✓ Respawn completo!");
    }

    void OnRetryClicked()
    {
        Debug.Log("[CheckpointSystem] Botão Retry clicado");
        StartCoroutine(RespawnSequence());
    }

    void OnMainMenuClicked()
    {
        Debug.Log("[CheckpointSystem] Botão Main Menu clicado");
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    void SaveCheckpointData()
    {
        if (lastCheckpoint == null) return;

        PlayerPrefs.SetFloat("CP_PosX", lastCheckpoint.position.x);
        PlayerPrefs.SetFloat("CP_PosY", lastCheckpoint.position.y);
        PlayerPrefs.SetFloat("CP_PosZ", lastCheckpoint.position.z);
        PlayerPrefs.SetInt("CP_Health", lastCheckpoint.playerHealth);
        PlayerPrefs.SetFloat("CP_Energy", lastCheckpoint.playerEnergy);
        PlayerPrefs.SetInt("CP_Index", lastCheckpoint.checkpointIndex);
        PlayerPrefs.SetString("CP_Scene", lastCheckpoint.sceneName);
        PlayerPrefs.Save();
    }

    void LoadCheckpointData()
    {
        if (!PlayerPrefs.HasKey("CP_Index")) return;

        Vector3 pos = new Vector3(
            PlayerPrefs.GetFloat("CP_PosX"),
            PlayerPrefs.GetFloat("CP_PosY"),
            PlayerPrefs.GetFloat("CP_PosZ")
        );

        lastCheckpoint = new CheckpointData(
            pos,
            PlayerPrefs.GetInt("CP_Health"),
            PlayerPrefs.GetFloat("CP_Energy"),
            gems != null ? gems.CurrentGems : 0,
            PlayerPrefs.GetInt("CP_Index"),
            PlayerPrefs.GetString("CP_Scene")
        );

        for (int i = 0; i <= lastCheckpoint.checkpointIndex; i++)
        {
            activatedCheckpoints.Add(i);
        }
    }

    public void ForceKillPlayer()
    {
        Debug.Log("[CheckpointSystem] Forçando morte do player...");

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(9999, Vector2.zero);
        }
        else
        {
            Debug.LogError("[CheckpointSystem] playerHealth é null!");

            // Tentar encontrar novamente
            FindPlayer();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(9999, Vector2.zero);
            }
            else
            {
                // Forçar chamada direta
                Debug.Log("[CheckpointSystem] Forçando OnPlayerDied diretamente");
                OnPlayerDied();
            }
        }
    }

    public void ForceRespawn()
    {
        StartCoroutine(RespawnSequence());
    }

    [ContextMenu("Test Death System")]
    void TestDeathSystem()
    {
        Debug.Log("=== TESTANDO SISTEMA DE MORTE ===");
        OnPlayerDied();
    }

    [ContextMenu("Debug Status")]
    void DebugStatus()
    {
        Debug.Log("=== CHECKPOINT SYSTEM STATUS ===");
        Debug.Log($"Instance: {Instance != null}");
        Debug.Log($"Player: {player}");
        Debug.Log($"PlayerHealth: {playerHealth}");
        Debug.Log($"GameOverPanel: {gameOverPanel}");
        Debug.Log($"IsDead: {isDead}");
        Debug.Log($"IsRespawning: {isRespawning}");
        Debug.Log($"LastCheckpoint: {lastCheckpoint != null}");
        Debug.Log($"Checkpoints: {checkpointsList.Count}");
    }

    void DebugCheckpoints()
    {
        Debug.Log("=== CHECKPOINT DEBUG ===");
        Debug.Log($"Total de checkpoints: {checkpointsList.Count}");
        Debug.Log($"Checkpoint atual: {currentCheckpointIndex}");
        Debug.Log($"Checkpoints ativados: {activatedCheckpoints.Count}");

        for (int i = 0; i < checkpointsList.Count; i++)
        {
            if (checkpointsList[i] != null)
            {
                string status = activatedCheckpoints.Contains(i) ? "✓" : "✗";
                string current = (i == currentCheckpointIndex) ? " [ATUAL]" : "";
                Debug.Log($"  {i}: {checkpointsList[i].name} {status}{current}");
            }
        }
    }

    void OnDrawGizmos()
    {
        if (!showGizmos) return;

        for (int i = 0; i < checkpointsList.Count; i++)
        {
            if (checkpointsList[i] == null) continue;

            Vector3 pos = checkpointsList[i].position;

            if (Application.isPlaying)
            {
                if (i == currentCheckpointIndex)
                    Gizmos.color = currentCheckpointColor;
                else if (activatedCheckpoints.Contains(i))
                    Gizmos.color = activeCheckpointColor;
                else
                    Gizmos.color = inactiveCheckpointColor;
            }
            else
            {
                Gizmos.color = inactiveCheckpointColor;
            }

            Gizmos.DrawWireSphere(pos, checkpointActivationRange);
            Gizmos.DrawCube(pos, Vector3.one * 0.3f);

            Gizmos.DrawLine(pos, pos + Vector3.up * 2f);
            Gizmos.DrawCube(pos + Vector3.up * 2f + Vector3.right * 0.3f, new Vector3(0.6f, 0.4f, 0.1f));

            if (showCheckpointNumbers)
            {
#if UNITY_EDITOR
                UnityEditor.Handles.Label(pos + Vector3.up * 2.5f, $"CP {i}");
#endif
            }
        }
    }
}