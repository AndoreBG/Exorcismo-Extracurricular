using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

[System.Serializable]
public class CheckpointData
{
    public Vector3 position;
    public int playerHealth;
    public float playerEnergy;
    public int gems;
    public int checkpointIndex;

    public CheckpointData(Vector3 pos, int health, float energy, int gemCount, int index)
    {
        position = pos;
        playerHealth = health;
        playerEnergy = energy;
        gems = gemCount;
        checkpointIndex = index;
    }
}

public class CheckpointSystem : MonoBehaviour
{
    public static CheckpointSystem Instance { get; private set; }

    [Header("=== Referencias Manuais ===")]
    [SerializeField] private GameObject player;
    [SerializeField] private avatarHealth playerHealth;
    [SerializeField] private magicSystem playerMagic;
    [SerializeField] private avatarMovement playerMovement;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private gemManager gemsManager;

    [Header("=== Checkpoints ===")]
    [SerializeField] private List<Transform> checkpoints = new List<Transform>();
    [SerializeField] private float activationRange = 2f;

    [Header("=== Respawn ===")]
    [SerializeField] private float respawnDelay = 1.5f;
    [SerializeField] private GameObject respawnEffectPrefab;
    [SerializeField] private AudioClip respawnSound;

    [Header("=== Checkpoint Feedback ===")]
    [SerializeField] private GameObject checkpointEffectPrefab;
    [SerializeField] private AudioClip checkpointSound;

    [Header("=== Visual ===")]
    [SerializeField] private Color inactiveColor = Color.red;
    [SerializeField] private Color activeColor = Color.yellow;
    [SerializeField] private Color currentColor = Color.green;
    [SerializeField] private bool showGizmos = true;

    [Header("=== Eventos ===")]
    public UnityEvent<int> OnCheckpointActivated;
    public UnityEvent OnPlayerRespawned;

    [Header("=== Debug ===")]
    [SerializeField] private bool debugMode = false;

    // Estado interno
    private HashSet<int> activatedCheckpoints = new HashSet<int>();
    private CheckpointData savedData;
    private int currentCheckpointIndex = -1;
    private AudioSource audioSource;
    private bool isRespawning = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        player = GameObject.FindGameObjectWithTag("Player");
        playerHealth = player?.GetComponent<avatarHealth>();
        playerMagic = player?.GetComponent<magicSystem>();
        playerMovement = player?.GetComponent<avatarMovement>();
        playerAnimator = player?.GetComponent<Animator>();

        ValidateReferences();
    }

    void Start()
    {
        // Ativar primeiro checkpoint se existir
        if (checkpoints.Count > 0 && checkpoints[0] != null)
        {
            ActivateCheckpoint(0);
        }

        LoadSavedData();
    }

    void Update()
    {
        if (player != null && !isRespawning)
        {
            CheckNearbyCheckpoints();
        }

        // Debug
        if (debugMode && Input.GetKeyDown(KeyCode.F5))
        {
            DebugInfo();
        }
    }
        
    private void FixedUpdate()
    {
        if (playerAnimator == null)
        {
            Debug.LogWarning("[Checkpoint System] Avatar Animator NULO");
            playerAnimator = playerHealth?.avatarAnimator;
        }
        else
        {
            Debug.LogWarning("[Checkpoint System] ACHOU!!");
        }
    }

    void ValidateReferences()
    {
        if (player == null)
            Debug.LogWarning("[CheckpointSystem] Player não atribuído!");

        if (playerHealth == null)
            Debug.LogWarning("[CheckpointSystem] PlayerHealth não atribuído!");

        if (gemsManager == null)
            gemsManager = GetComponent<gemManager>();

        checkpoints.RemoveAll(item => item == null);

        if (debugMode)
            Debug.Log($"[CheckpointSystem] {checkpoints.Count} checkpoints configurados");
    }

    void CheckNearbyCheckpoints()
    {
        for (int i = 0; i < checkpoints.Count; i++)
        {
            if (checkpoints[i] == null) continue;

            float distance = Vector3.Distance(player.transform.position, checkpoints[i].position);

            if (distance < activationRange && !activatedCheckpoints.Contains(i))
            {
                ActivateCheckpoint(i);
            }
        }
    }

    public void ActivateCheckpoint(int index)
    {
        if (index < 0 || index >= checkpoints.Count || checkpoints[index] == null)
            return;

        currentCheckpointIndex = index;
        activatedCheckpoints.Add(index);

        SaveCheckpoint();

        // Efeitos
        if (checkpointEffectPrefab != null)
        {
            GameObject effect = Instantiate(checkpointEffectPrefab, checkpoints[index].position, Quaternion.identity);
            Destroy(effect, 3f);
        }

        if (audioSource != null && checkpointSound != null)
        {
            audioSource.PlayOneShot(checkpointSound);
        }

        OnCheckpointActivated?.Invoke(index);

        if (debugMode)
            Debug.Log($"[CheckpointSystem] Checkpoint {index} ativado");
    }

    void SaveCheckpoint()
    {
        if (player == null) return;

        Vector3 pos = currentCheckpointIndex >= 0 ? checkpoints[currentCheckpointIndex].position : player.transform.position;
        int health = playerHealth != null ? playerHealth.CurrentHealth : 3;
        float energy = playerMagic != null ? playerMagic.CurrentEnergy : 100f;
        int gems = gemsManager != null ? gemsManager.CurrentGems : 0;

        savedData = new CheckpointData(pos, health, energy, gems, currentCheckpointIndex);

        SaveToPrefs();
    }

    public void RespawnPlayer()
    {
        if (isRespawning) return;

        StartCoroutine(RespawnSequence());
    }

    IEnumerator RespawnSequence()
    {
        isRespawning = true;

        if (debugMode)
            Debug.Log("[CheckpointSystem] Iniciando respawn...");

        yield return new WaitForSeconds(respawnDelay);

        if (savedData == null)
        {
            Debug.LogError("[CheckpointSystem] Sem checkpoint salvo!");
            isRespawning = false;
            yield break;
        }

        // Respawnar
        if (player != null)
        {
            player.transform.position = savedData.position;
        }

        playerAnimator.Rebind();

        if (playerHealth != null)
        {
            playerHealth.Respawn(savedData.position);
        }

        if (playerMagic != null && savedData.playerEnergy > playerMagic.CurrentEnergy)
        {
            playerMagic.RestoreEnergy(savedData.playerEnergy - playerMagic.CurrentEnergy);
        }

        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        // Efeitos
        if (respawnEffectPrefab != null)
        {
            GameObject effect = Instantiate(respawnEffectPrefab, savedData.position, Quaternion.identity);
            Destroy(effect, 3f);
        }

        if (audioSource != null && respawnSound != null)
        {
            audioSource.PlayOneShot(respawnSound);
        }

        OnPlayerRespawned?.Invoke();

        isRespawning = false;

        if (debugMode)
            Debug.Log("[CheckpointSystem] Respawn completo");
    }

    public Vector3 GetLastCheckpointPosition()
    {
        return savedData != null ? savedData.position : Vector3.zero;
    }

    void SaveToPrefs()
    {
        if (savedData == null) return;

        PlayerPrefs.SetFloat("CP_X", savedData.position.x);
        PlayerPrefs.SetFloat("CP_Y", savedData.position.y);
        PlayerPrefs.SetInt("CP_Health", savedData.playerHealth);
        PlayerPrefs.SetFloat("CP_Energy", savedData.playerEnergy);
        PlayerPrefs.SetInt("CP_Index", savedData.checkpointIndex);
        PlayerPrefs.Save();
    }

    void LoadSavedData()
    {
        if (!PlayerPrefs.HasKey("CP_Index")) return;

        Vector3 pos = new Vector3(
            PlayerPrefs.GetFloat("CP_X"),
            PlayerPrefs.GetFloat("CP_Y"),
            0
        );

        savedData = new CheckpointData(
            pos,
            PlayerPrefs.GetInt("CP_Health"),
            PlayerPrefs.GetFloat("CP_Energy"),
            gemsManager != null ? gemsManager.CurrentGems : 0,
            PlayerPrefs.GetInt("CP_Index")
        );

        for (int i = 0; i <= savedData.checkpointIndex; i++)
        {
            activatedCheckpoints.Add(i);
        }
    }

    public void ClearSavedData()
    {
        PlayerPrefs.DeleteKey("CP_X");
        PlayerPrefs.DeleteKey("CP_Y");
        PlayerPrefs.DeleteKey("CP_Health");
        PlayerPrefs.DeleteKey("CP_Energy");
        PlayerPrefs.DeleteKey("CP_Index");
        PlayerPrefs.Save();

        savedData = null;
        activatedCheckpoints.Clear();
        currentCheckpointIndex = -1;
    }

    void DebugInfo()
    {
        Debug.Log("=== CHECKPOINT DEBUG ===");
        Debug.Log($"Checkpoints: {checkpoints.Count}");
        Debug.Log($"Atual: {currentCheckpointIndex}");
        Debug.Log($"Ativados: {activatedCheckpoints.Count}");
        Debug.Log($"Saved Data: {savedData != null}");
    }

    void OnDrawGizmos()
    {
        if (!showGizmos) return;

        for (int i = 0; i < checkpoints.Count; i++)
        {
            if (checkpoints[i] == null) continue;

            Vector3 pos = checkpoints[i].position;

            // Cor baseada no estado
            if (Application.isPlaying)
            {
                if (i == currentCheckpointIndex)
                    Gizmos.color = currentColor;
                else if (activatedCheckpoints.Contains(i))
                    Gizmos.color = activeColor;
                else
                    Gizmos.color = inactiveColor;
            }
            else
            {
                Gizmos.color = inactiveColor;
            }

            Gizmos.DrawWireSphere(pos, activationRange);
            Gizmos.DrawCube(pos, Vector3.one * 0.3f);
            Gizmos.DrawLine(pos, pos + Vector3.up * 1.5f);
        }
    }
}