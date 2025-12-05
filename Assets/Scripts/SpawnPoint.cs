using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField] private string pointName;

    void Start()
    {
        // Verificar se este é o spawn point correto
        string savedSpawnPoint = PlayerPrefs.GetString("SpawnPoint", "");

        if (savedSpawnPoint == pointName)
        {
            // Posicionar o player aqui
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                player.transform.position = transform.position;
            }

            // Limpar para próxima vez
            PlayerPrefs.DeleteKey("SpawnPoint");
        }
    }
}