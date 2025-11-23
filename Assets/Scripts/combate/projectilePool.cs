using System.Collections.Generic;
using UnityEngine;

public class projectilePool : MonoBehaviour
{
    private Dictionary<GameObject, Queue<GameObject>> poolDictionary = new Dictionary<GameObject, Queue<GameObject>>();

    // NOVO: Container separado para projéteis ativos
    private Transform activeProjectilesContainer;

    void Awake()
    {
        // Criar container independente para projéteis ativos
        GameObject container = new GameObject("ActiveProjectiles");
        activeProjectilesContainer = container.transform;
        // NÃO definir parent - fica na raiz da cena
    }

    public void AddProjectileType(GameObject prefab, int poolSize)
    {
        if (prefab == null || poolDictionary.ContainsKey(prefab))
            return;

        Queue<GameObject> objectPool = new Queue<GameObject>();

        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(prefab, transform); // Inativos ficam na pool
            obj.SetActive(false);
            objectPool.Enqueue(obj);
        }

        poolDictionary.Add(prefab, objectPool);
    }

    public GameObject GetProjectile(GameObject prefab)
    {
        if (!poolDictionary.ContainsKey(prefab))
        {
            Debug.LogWarning($"Pool não contém prefab: {prefab.name}");
            return null;
        }

        Queue<GameObject> pool = poolDictionary[prefab];

        // Procurar projétil inativo
        int attempts = pool.Count;
        while (attempts > 0)
        {
            GameObject obj = pool.Dequeue();
            pool.Enqueue(obj);

            if (!obj.activeInHierarchy)
            {
                obj.SetActive(true);

                // MUDANÇA: Mover para container independente
                obj.transform.SetParent(activeProjectilesContainer);

                return obj;
            }

            attempts--;
        }

        // Se todos estão ativos, criar novo
        GameObject newObj = Instantiate(prefab);

        // MUDANÇA: Já criar no container independente
        newObj.transform.SetParent(activeProjectilesContainer);

        pool.Enqueue(newObj);
        return newObj;
    }

    public void ReturnProjectile(GameObject projectile)
    {
        projectile.SetActive(false);

        // MUDANÇA: Voltar para a pool quando desativado
        projectile.transform.SetParent(transform);

        projectile.transform.localPosition = Vector3.zero;
    }
}