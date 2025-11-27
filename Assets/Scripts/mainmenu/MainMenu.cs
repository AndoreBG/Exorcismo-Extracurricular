using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("Basement");
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
