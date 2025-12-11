using UnityEngine;
using System.Collections.Generic;

public class BuildDebugger : MonoBehaviour
{
    private static BuildDebugger instance;
    private List<string> logs = new List<string>();
    private Vector2 scrollPosition;
    private bool showLogs = true;

    [SerializeField] private int maxLogs = 20;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        // Capturar todos os logs do Unity
        Application.logMessageReceived += HandleLog;
    }

    void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        // Filtrar apenas logs relevantes
        if (logString.Contains("[CheckpointSystem]") ||
            logString.Contains("[MenuManager]") ||
            logString.Contains("Player") ||
            type == LogType.Error ||
            type == LogType.Exception)
        {
            string prefix = type == LogType.Error || type == LogType.Exception ? "❌ " : "✅ ";
            logs.Add($"{prefix}{logString}");

            if (logs.Count > maxLogs)
                logs.RemoveAt(0);
        }
    }

    void Update()
    {
        // F12 para mostrar/esconder logs
        if (Input.GetKeyDown(KeyCode.F12))
        {
            showLogs = !showLogs;
        }
    }

    void OnGUI()
    {
        if (!showLogs) return;

        // Caixa de fundo
        GUI.Box(new Rect(10, 10, 600, 400), "BUILD DEBUG (F12 para esconder)");

        // Área de scroll para os logs
        GUILayout.BeginArea(new Rect(20, 40, 580, 360));
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        foreach (string log in logs)
        {
            GUILayout.Label(log);
        }

        GUILayout.EndScrollView();
        GUILayout.EndArea();

        // Info adicional
        GUI.Label(new Rect(10, 420, 600, 100),
            $"CheckpointSystem.Instance: {(CheckpointSystem.Instance != null ? "OK" : "NULL")}\n" +
            $"Player Tag encontrado: {(GameObject.FindGameObjectWithTag("Player")?.name ?? "NULL")}");
    }
}