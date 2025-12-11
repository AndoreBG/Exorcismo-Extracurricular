using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [SerializeField] private Animator animator;

    private void Awake()
    {
        if (Instance == null) 
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void NextLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void LoadLevel(string levelName)
    {
        StartCoroutine(LoadLevelWithTransition(levelName));
    }

    public void TransitionEvent()
    {
        StartCoroutine(EventWithTransition());
    }

    IEnumerator LoadLevelWithTransition(string levelName)
    {
        animator.SetTrigger("Start");
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(levelName);
        animator.SetTrigger("End");
    }
    IEnumerator EventWithTransition()
    {
        animator.SetTrigger("Start");
        yield return new WaitForSeconds(2f);
        animator.SetTrigger("End");
    }
}
