using UnityEngine;

public class ProjectileDebugger : MonoBehaviour
{
    [ContextMenu("Check Projectile Prefabs")]
    void CheckPrefabs()
    {
        Debug.Log("=== VERIFICAÇÃO DE PROJÉTEIS ===");

        // Encontrar todos os projéteis ativos
        projectile[] projectiles = FindObjectsByType<projectile>(FindObjectsSortMode.None);

        if (projectiles.Length == 0)
        {
            Debug.LogWarning("Nenhum projétil ativo encontrado. Atire um projétil primeiro!");
            return;
        }

        foreach (projectile proj in projectiles)
        {
            GameObject go = proj.gameObject;
            Rigidbody2D rb = go.GetComponent<Rigidbody2D>();
            Collider2D col = go.GetComponent<Collider2D>();

            Debug.Log($"[{go.name}]");
            Debug.Log($"  Active: {go.activeInHierarchy}");
            Debug.Log($"  Layer: {LayerMask.LayerToName(go.layer)}");
            Debug.Log($"  Tag: {go.tag}");
            Debug.Log($"  Rigidbody2D: {rb != null} (Type: {rb?.bodyType})");
            Debug.Log($"  Collider2D: {col != null} (IsTrigger: {col?.isTrigger}, Enabled: {col?.enabled})");
            Debug.Log("---");
        }
    }

    [ContextMenu("Check Tilemap Colliders")]
    void CheckTilemaps()
    {
        Debug.Log("=== VERIFICAÇÃO DE TILEMAPS ===");

        GameObject[] grounds = GameObject.FindGameObjectsWithTag("Ground");
        GameObject[] walls = GameObject.FindGameObjectsWithTag("Wall");
        GameObject[] obstacles = GameObject.FindGameObjectsWithTag("Obstacle");

        Debug.Log($"Ground tags encontradas: {grounds.Length}");
        foreach (var g in grounds)
        {
            Collider2D col = g.GetComponent<Collider2D>();
            Debug.Log($"  - {g.name}: Collider={col != null}, IsTrigger={col?.isTrigger}");
        }

        Debug.Log($"Wall tags encontradas: {walls.Length}");
        foreach (var w in walls)
        {
            Collider2D col = w.GetComponent<Collider2D>();
            Debug.Log($"  - {w.name}: Collider={col != null}, IsTrigger={col?.isTrigger}");
        }

        Debug.Log($"Obstacle tags encontradas: {obstacles.Length}");
        foreach (var o in obstacles)
        {
            Collider2D col = o.GetComponent<Collider2D>();
            Debug.Log($"  - {o.name}: Collider={col != null}, IsTrigger={col?.isTrigger}");
        }
    }
}