using UnityEngine;

public class dropItem : MonoBehaviour
{
    [Space]
    [Header("=== Configuração de Drop ===")]
    [SerializeField] private GameObject[] orbPrefabs;
    [SerializeField] private int minOrb= 1;
    [SerializeField] private int maxOrb = 2;

    [Space]
    [SerializeField] private GameObject[] gemPrefabs;
    [SerializeField] private int minGems = 1;
    [SerializeField] private int maxGems = 3;


    // Chamar quando o inimigo morrer
    public void DropGems()
    {
        int gemCount = Random.Range(minGems, maxGems + 1);

        for (int i = 0; i < gemCount; i++)
        {
            SpawnGem();
        }
    }
    public void DropOrbs()
    {
        int gemCount = Random.Range(minOrb, maxOrb + 1);

        for (int i = 0; i < gemCount; i++)
        {
            SpawnOrb();
        }
    }

    private void SpawnGem()
    {
        // Escolher gem aleatória
        GameObject gemPrefab = gemPrefabs[Random.Range(0, gemPrefabs.Length)];

        // Spawnar gem na posição do inimigo
        Vector3 spawnPos = transform.position;
        GameObject gem = Instantiate(gemPrefab, spawnPos, Quaternion.identity);

    }

    private void SpawnOrb()
    {
        // Escolher orb aleatório
        GameObject gemPrefab = orbPrefabs[Random.Range(0, orbPrefabs.Length)];

        // Spawnar orb na posição do inimigo
        Vector3 spawnPos = transform.position;
        GameObject gem = Instantiate(gemPrefab, spawnPos, Quaternion.identity);

    }

    // Método para spawnar item específico
    public void SpawnSpecificItem(GameObject itemPrefab) {
        GameObject gem = Instantiate(itemPrefab, transform.position, Quaternion.identity);

    }
}