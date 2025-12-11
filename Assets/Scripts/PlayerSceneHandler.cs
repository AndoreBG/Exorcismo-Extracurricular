using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerSceneHandler : MonoBehaviour
{
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Inscrever no evento de cena carregada
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        // IMPORTANTE: Sempre remover o listener ao destruir
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[PlayerSceneHandler] Cena '{scene.name}' carregada. Buscando spawn point...");
        StartCoroutine(MoveToSpawnPoint());
    }

    IEnumerator MoveToSpawnPoint()
    {
        // Esperar 1 frame para garantir que todos os objetos da cena foram inicializados
        yield return null;

        string savedSpawnPoint = PlayerPrefs.GetString("SpawnPoint", "");

        Debug.Log($"[PlayerSceneHandler] SpawnPoint salvo: '{savedSpawnPoint}'");

        if (string.IsNullOrEmpty(savedSpawnPoint))
        {
            Debug.Log("[PlayerSceneHandler] Nenhum spawn point definido, mantendo posição atual.");
            yield break;
        }

        // Encontrar o spawn point correto
        SpawnPoint[] spawnPoints = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);

        Debug.Log($"[PlayerSceneHandler] Encontrados {spawnPoints.Length} spawn points na cena.");

        SpawnPoint targetSpawn = null;

        foreach (var sp in spawnPoints)
        {
            if (sp.PointName == savedSpawnPoint)
            {
                targetSpawn = sp;
                break;
            }
        }

        if (targetSpawn == null)
        {
            Debug.LogWarning($"[PlayerSceneHandler] Spawn point '{savedSpawnPoint}' não encontrado!");
            PlayerPrefs.DeleteKey("SpawnPoint");
            yield break;
        }

        // Teleportar o player
        yield return StartCoroutine(TeleportTo(targetSpawn.transform.position));

        // Limpar para próxima vez
        PlayerPrefs.DeleteKey("SpawnPoint");

        Debug.Log($"[PlayerSceneHandler] Player movido para {transform.position}");
    }

    IEnumerator TeleportTo(Vector3 targetPosition)
    {
        // Desabilitar física temporariamente
        RigidbodyType2D originalType = RigidbodyType2D.Dynamic;

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
        }

        // Definir posição
        transform.position = targetPosition;

        if (rb != null)
        {
            rb.position = targetPosition;
        }

        // Sincronizar física
        Physics2D.SyncTransforms();

        // Esperar um frame de física
        yield return new WaitForFixedUpdate();

        // Restaurar física
        if (rb != null)
        {
            rb.bodyType = originalType;
        }
    }

    // Método público para teleportar de qualquer lugar (checkpoints, etc)
    public void TeleportToPosition(Vector3 position)
    {
        StartCoroutine(TeleportTo(position));
    }
}