using UnityEngine;
using UnityEngine.Events;

public class DialogueGeneric : MonoBehaviour, IInteractable
{
    [SerializeField] private DialogueEntry[] dialogues;

    [Space]
    public UnityEvent OnInteract; 

    private bool isInDialogue = false;

    public void Interact()
    {
        if (isInDialogue) return;

        OnInteract?.Invoke();

        isInDialogue = true;
        DialogueManager.Instance.StartDialogue(dialogues);
    }

    private void Update()
    {
        // Verifica se o diálogo terminou
        if (isInDialogue && !DialogueManager.Instance.IsActive)
        {
            isInDialogue = false;
        }
    }

    public bool CanInteract()
    {
        return !isInDialogue;
    }
}