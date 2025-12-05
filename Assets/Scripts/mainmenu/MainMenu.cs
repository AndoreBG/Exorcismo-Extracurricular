using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject buttonsPanel;
    [SerializeField] private GameObject optionsPanel;

    private GameObject player;
    private Canvas UI;
    private Canvas Menu;

    private void Start()
    {
        player = GameObject.Find("Avatar");
        UI = GameObject.Find("UI").GetComponent<Canvas>();
        Menu = GameObject.Find("Main").GetComponent<Canvas>();
    }

    private void Update()
    {
        if (player != null)
        {
            player.SetActive(false);
        }
        
        if (UI != null)
        {
            UI.enabled = false;
        }

        if (Menu != null)
        {
            Menu.enabled = true;
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Cutscene");
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
