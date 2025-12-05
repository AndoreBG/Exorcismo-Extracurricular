using UnityEditor;
using UnityEngine;

public class DisableDontDestroy : MonoBehaviour
{
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
}
