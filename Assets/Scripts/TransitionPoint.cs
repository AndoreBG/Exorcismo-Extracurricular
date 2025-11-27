using UnityEngine;

public class TransitionPoint : MonoBehaviour
{
    [SerializeField] private string levelToLoad;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            LevelManager.Instance.LoadLevel(levelToLoad);
        }
    }
}
