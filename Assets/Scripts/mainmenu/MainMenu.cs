using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject buttonsPanel;
    [SerializeField] private GameObject optionsPanel;

    public void StartGame()
    {
        SceneManager.LoadScene("Basement");
    }

    public void OpenOptions()
    {
        buttonsPanel.SetActive(false);
        optionsPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        optionsPanel.SetActive(false);
        buttonsPanel.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
