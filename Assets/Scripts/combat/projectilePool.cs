using System.Collections.Generic;
using UnityEngine;

public class projectilePool : MonoBehaviour
{
    private Dictionary<GameObject, Queue<GameObject>> poolDictionary = new Dictionary<GameObject, Queue<GameObject>>();
    private Dictionary<GameObject, GameObject> prefabLookup = new Dictionary<GameObject, GameObject>();

    public void AddProjectileType(GameObject prefab, int poolSize)
    {
        if (prefab == null || poolDictionary.ContainsKey(prefab))
            return;

        Queue<GameObject> objectPool = new Queue<GameObject>();

        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            objectPool.Enqueue(obj);

            // Mapear instância para prefab original
            prefabLookup[obj] = prefab;
        }

        poolDictionary.Add(prefab, objectPool);
    }

    public GameObject GetProjectile(GameObject prefab)
    {
        if (!poolDictionary.ContainsKey(prefab))
        {
            Debug.LogWarning($"Pool não contém o prefab: {prefab.name}");
            return null;
        }

        Queue<GameObject> pool = poolDictionary[prefab];

        // Procurar projétil inativo
        GameObject projectile = null;
        int attempts = pool.Count;

        while (attempts > 0)
        {
            projectile = pool.Dequeue();
            pool.Enqueue(projectile);

            if (!projectile.activeInHierarchy)
            {
                projectile.SetActive(true);
                return projectile;
            }

            attempts--;
        }

        // Se todos estão em uso, criar um novo
        GameObject newProjectile = Instantiate(prefab, transform);
        pool.Enqueue(newProjectile);
        prefabLookup[newProjectile] = prefab;

        return newProjectile;
    }

    public void ReturnProjectile(GameObject projectile)
    {
        projectile.SetActive(false);
        projectile.transform.position = transform.position;
    }
}