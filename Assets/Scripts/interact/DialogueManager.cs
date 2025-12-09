// DialogueManager.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI - Personagem")]
    [SerializeField] private Image characterImage;

    [Header("UI - Caixa de Diálogo")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private Image boxImage;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("UI - Nome do Personagem")]
    [SerializeField] private GameObject namePanel;
    [SerializeField] private Image nameBoxImage;
    [SerializeField] private TextMeshProUGUI nameText;

    [Header("Configurações")]
    [SerializeField] private float typingSpeed = 0.05f;

    [Header("Áudio")]
    [SerializeField] private AudioSource audioSource;

    private DialogueEntry[] currentDialogue;
    private int currentIndex = 0;
    private bool isActive = false;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    public bool IsActive => isActive;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        dialoguePanel.SetActive(false);
        namePanel.SetActive(false);
    }

    private void Update()
    {
        if (!isActive) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isTyping)
            {
                SkipTyping();
            }
            else
            {
                NextLine();
            }
        }
    }

    public void StartDialogue(DialogueEntry[] entries)
    {
        currentDialogue = entries;
        currentIndex = 0;
        isActive = true;
        dialoguePanel.SetActive(true);
        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        DialogueEntry entry = currentDialogue[currentIndex];

        // Personagem
        characterImage.sprite = entry.characterSprite;

        // Caixa de diálogo
        boxImage.sprite = entry.dialogueBoxSprite;
        boxImage.color = entry.boxColor;

        // Nome do personagem
        if (!string.IsNullOrEmpty(entry.characterName))
        {
            namePanel.SetActive(true);
            nameText.text = entry.characterName;
            nameBoxImage.sprite = entry.nameBoxSprite;
            nameBoxImage.color = entry.nameBoxColor;
        }
        else
        {
            namePanel.SetActive(false);
        }

        // Configura o áudio
        audioSource.resource = entry.typingSound;

        // Inicia efeito de digitação
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(entry));
    }

    private IEnumerator TypeText(DialogueEntry entry)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in entry.text)
        {
            dialogueText.text += c;

            if (c != ' ' && entry.typingSound != null)
            {
                audioSource.Play();
            }

            yield return new WaitForSeconds(typingSpeed);
        }

        // Para o áudio quando termina de digitar
        StopAudio();
        isTyping = false;
    }

    private void SkipTyping()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        dialogueText.text = currentDialogue[currentIndex].text;
        StopAudio();
        isTyping = false;
    }

    private void StopAudio()
    {
        audioSource.Stop();
    }

    private void NextLine()
    {
        currentIndex++;

        if (currentIndex >= currentDialogue.Length)
        {
            EndDialogue();
        }
        else
        {
            ShowCurrentLine();
        }
    }

    private void EndDialogue()
    {
        isActive = false;
        dialoguePanel.SetActive(false);
        namePanel.SetActive(false);
        StopAudio();
    }
}