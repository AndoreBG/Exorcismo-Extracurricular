using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System;

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
    private bool canInteract = true;
    private Coroutine typingCoroutine;

    // Callback para quando o diálogo terminar
    private Action onDialogueComplete;

    public bool IsActive => isActive;
    public bool CanStartDialogue => canInteract && !isActive;

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

    // Método original (para compatibilidade)
    public void StartDialogue(DialogueEntry[] entries)
    {
        StartDialogue(entries, null);
    }

    // Novo método com callback
    public void StartDialogue(DialogueEntry[] entries, Action onComplete)
    {
        if (!canInteract || isActive) return;

        currentDialogue = entries;
        currentIndex = 0;
        isActive = true;
        onDialogueComplete = onComplete;
        dialoguePanel.SetActive(true);
        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        DialogueEntry entry = currentDialogue[currentIndex];

        characterImage.sprite = entry.characterSprite;

        boxImage.sprite = entry.dialogueBoxSprite;
        boxImage.color = entry.boxColor;

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

        audioSource.resource = entry.typingSound;

        entry.OnStartDialogue?.Invoke();

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

    public void EndDialogue()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        isActive = false;
        isTyping = false;
        canInteract = false;
        dialoguePanel.SetActive(false);
        namePanel.SetActive(false);
        StopAudio();

        // Invoca o callback antes do cooldown
        onDialogueComplete?.Invoke();
        onDialogueComplete = null;

        StartCoroutine(InteractCooldown());
    }

    private IEnumerator InteractCooldown()
    {
        yield return new WaitForSeconds(0.2f);
        canInteract = true;
    }
}