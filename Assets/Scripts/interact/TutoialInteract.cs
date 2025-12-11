using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class TutorialInteract : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private GameObject skipIndicator;

    [Header("Painéis")]
    [SerializeField] private GameObject[] tutorialPanels;

    [Header("Configurações")]
    [SerializeField] private float delayToSkip = 1f;

    private UnityEvent onTutorialFinish;

    private int currentIndex = 0;
    private bool isActive = false;
    private bool canSkip = false;

    private void Start()
    {
        tutorialPanel.SetActive(false);
        skipIndicator.SetActive(false);

        // Desativa todos os painéis no início
        foreach (GameObject panel in tutorialPanels)
        {
            panel.SetActive(false);
        }
    }

    private void Update()
    {
        if (!isActive) return;

        if (canSkip && Input.GetKeyDown(KeyCode.E))
        {
            NextPanel();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            StartTutorial();
        }
    }

    private void StartTutorial()
    {
        isActive = true;
        canSkip = false;
        currentIndex = 0;

        Time.timeScale = 0f;

        tutorialPanel.SetActive(true);
        ShowCurrentPanel();
    }

    private void ShowCurrentPanel()
    {
        // Desativa o painel anterior
        if (currentIndex > 0)
        {
            tutorialPanels[currentIndex - 1].SetActive(false);
        }

        // Ativa o painel atual
        tutorialPanels[currentIndex].SetActive(true);

        canSkip = false;
        skipIndicator.SetActive(false);

        StartCoroutine(EnableSkip());
    }

    private IEnumerator EnableSkip()
    {
        yield return new WaitForSecondsRealtime(delayToSkip);

        canSkip = true;
        skipIndicator.SetActive(true);
    }

    private void NextPanel()
    {
        currentIndex++;

        if (currentIndex >= tutorialPanels.Length)
        {
            EndTutorial();
        }
        else
        {
            ShowCurrentPanel();
        }
    }

    private void EndTutorial()
    {
        isActive = false;
        canSkip = false;

        Time.timeScale = 1f;

        // Desativa o último painel
        tutorialPanels[currentIndex - 1].SetActive(false);

        tutorialPanel.SetActive(false);
        skipIndicator.SetActive(false);

        onTutorialFinish?.Invoke();

        gameObject.SetActive(false);
    }
}