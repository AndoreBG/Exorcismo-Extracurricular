using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class DetectUI : MonoBehaviour
{
    private GameObject ui;
    private GameObject hud;
    private GameObject magicDisplay;

    private void Start()
    {
        GetUI();
    }

    private void Update()
    {
        if (ui == null)
        {
            GetUI();
        }
    }

    public void GetUI()
    {
        ui = GameObject.FindGameObjectWithTag("UI");
    }

    public void EnableMagicSystem()
    {
        if (ui == null) return;

        hud = ui.gameObject.transform.Find("HUD").gameObject;
        magicDisplay = hud.gameObject.transform.Find("Symbol_Display").gameObject;

        magicDisplay.SetActive(true);
    }
}
