using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }

    [Header("=== Referencias do Canvas ===")]
    [SerializeField] private Canvas menusCanvas;

    [Header("=== Pause Menu ===")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button pauseContinueButton;
    [SerializeField] private Button pauseMainMenuButton;

    [Header("=== Game Over Menu ===")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button gameOverRetryButton;
    [SerializeField] private Button gameOverMainMenuButton;
    [SerializeField] private float gameOverDelay = 1.5f;

    [Header("=== Referencias do Player ===")]
    [SerializeField] private avatarHealth playerHealth;
    [SerializeField] private avatarMovement playerMovement;

    [Header("=== Configurações ===")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;

    [Header("=== Sons ===")]
    [SerializeField] private AudioSource pauseOpenSound;
    [SerializeField] private AudioSource pauseCloseSound;
    [SerializeField] private AudioSource gameOverSound;

    [Header("=== Eventos ===")]
    public UnityEvent OnPauseOpened;
    public UnityEvent OnPauseClosed;
    public UnityEvent OnGameOver;

    [Header("=== Debug ===")]
    [SerializeField] private bool debugMode = false;

    // Estado
    private bool isPaused = false;
    private bool isGameOver = false;
    private AudioSource audioSource;

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        SetupUI();
        ValidateReferences();
    }

    void Start()
    {
        // Conectar ao evento de morte do player
        if (playerHealth != null)
        {
            playerHealth.OnDeath.RemoveListener(OnPlayerDeath);
            playerHealth.OnDeath.AddListener(OnPlayerDeath);
        }

        // Garantir que os menus começam fechados
        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    void Update()
    {
        // Controle do Pause
        if (Input.GetKeyDown(pauseKey) && !isGameOver)
        {
            if (isPaused)
                ClosePauseMenu();
            else
                OpenPauseMenu();
        }
    }

    void SetupUI()
    {
        // Pause Menu
        if (pauseContinueButton != null)
        {
            pauseContinueButton.onClick.RemoveAllListeners();
            pauseContinueButton.onClick.AddListener(OnPauseContinue);
        }

        if (pauseMainMenuButton != null)
        {
            pauseMainMenuButton.onClick.RemoveAllListeners();
            pauseMainMenuButton.onClick.AddListener(() => GoToMainMenu("pause"));
        }

        // Game Over Menu
        if (gameOverRetryButton != null)
        {
            gameOverRetryButton.onClick.RemoveAllListeners();
            gameOverRetryButton.onClick.AddListener(OnGameOverRetry);
        }

        if (gameOverMainMenuButton != null)
        {
            gameOverMainMenuButton.onClick.RemoveAllListeners();
            gameOverMainMenuButton.onClick.AddListener(() => GoToMainMenu("gameover"));
        }
    }

    void ValidateReferences()
    {
        if (menusCanvas == null)
            Debug.LogError("[MenuManager] Canvas não atribuído!");

        if (pausePanel == null)
            Debug.LogWarning("[MenuManager] PausePanel não atribuído!");

        if (gameOverPanel == null)
            Debug.LogWarning("[MenuManager] GameOverPanel não atribuído!");

        if (playerHealth == null)
            Debug.LogWarning("[MenuManager] PlayerHealth não atribuído!");
    }

    // ========== PAUSE MENU ==========

    public void OpenPauseMenu()
    {
        if (isPaused || isGameOver) return;

        isPaused = true;

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        Time.timeScale = 0f;

        if (audioSource != null && pauseOpenSound != null)
        {
            pauseOpenSound.Play();
        }

        OnPauseOpened?.Invoke();

        if (debugMode)
            Debug.Log("[MenuManager] Pause aberto");
    }

    public void ClosePauseMenu()
    {
        if (!isPaused) return;

        isPaused = false;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        Time.timeScale = 1f;

        if (audioSource != null && pauseCloseSound != null)
        {
            pauseCloseSound.Play();
        }

        OnPauseClosed?.Invoke();

        if (debugMode)
            Debug.Log("[MenuManager] Pause fechado");
    }

    void OnPauseContinue()
    {
        ClosePauseMenu();
    }

    // ========== GAME OVER ==========

    void OnPlayerDeath()
    {
        if (isGameOver) return;

        if (debugMode)
            Debug.Log("[MenuManager] Player morreu - iniciando Game Over");

        // Desabilitar movimento do player
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        StartCoroutine(ShowGameOverDelayed());
    }

    IEnumerator ShowGameOverDelayed()
    {
        yield return new WaitForSeconds(gameOverDelay);

        ShowGameOver();
    }

    public void ShowGameOver()
    {
        isGameOver = true;
        isPaused = false; // Garantir que pause está fechado

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        Time.timeScale = 0f;

        if (audioSource != null && gameOverSound != null)
        {
            gameOverSound.Play();
        }

        OnGameOver?.Invoke();

        if (debugMode)
            Debug.Log("[MenuManager] Game Over mostrado");
    }

    public void HideGameOver()
    {
        isGameOver = false;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
            gameOverSound.Stop();
        }

        Time.timeScale = 1f;

        // Reabilitar movimento do player
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }
    }

    void OnGameOverRetry()
    {
        if (debugMode)
            Debug.Log("[MenuManager] Retry pressionado");

        HideGameOver();

        // Chamar respawn do CheckpointSystem
        if (CheckpointSystem.Instance != null)
        {
            CheckpointSystem.Instance.RespawnPlayer();
        }
        else
        {
            Debug.LogError("[MenuManager] CheckpointSystem não encontrado!");
        }
    }

    // ========== NAVEGAÇÃO ==========

    void GoToMainMenu(string source)
    {
        if (debugMode)
            Debug.Log($"[MenuManager] Indo para menu principal de: {source}");

        Time.timeScale = 1f;
        if (source == "pause")
        {
            ClosePauseMenu();
        }
        else if (source == "gameover")
        {
            HideGameOver();
        }

        SceneManager.LoadScene(mainMenuSceneName);
    }

    // ========== MÉTODOS PÚBLICOS ==========

    public bool IsPaused() => isPaused;
    public bool IsGameOver() => isGameOver;
    public bool IsAnyMenuOpen() => isPaused || isGameOver;

    void OnDestroy()
    {
        // Garantir que o tempo volta ao normal
        Time.timeScale = 1f;
    }

    // ========== DEBUG ==========

    [ContextMenu("Test Pause")]
    void TestPause()
    {
        if (isPaused)
            ClosePauseMenu();
        else
            OpenPauseMenu();
    }

    [ContextMenu("Test Game Over")]
    void TestGameOver()
    {
        ShowGameOver();
    }
}