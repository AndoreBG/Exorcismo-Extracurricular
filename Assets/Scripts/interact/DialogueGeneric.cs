using UnityEngine;

public class DialogueGeneric : MonoBehaviour, IInteractable
{
    [SerializeField] private DialogueEntry[] dialogues;

    public void Interact()
    {
        if (!DialogueManager.Instance.CanStartDialogue) return;

        DialogueManager.Instance.StartDialogue(dialogues);
    }

    public bool CanInteract()
    {
        return DialogueManager.Instance.CanStartDialogue;
    }
}