using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionPoint : MonoBehaviour
{
    [Header("Configuração")]
    [SerializeField] private string targetScene;
    [SerializeField] private string spawnPointName;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Salvar onde o player deve aparecer
            PlayerPrefs.SetString("SpawnPoint", spawnPointName);

            // Carregar a cena
            SceneManager.LoadScene(targetScene);
        }
    }
}