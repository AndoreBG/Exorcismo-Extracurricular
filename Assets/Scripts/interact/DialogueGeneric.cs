using UnityEngine;
using UnityEngine.Events;   

public class DialogueGeneric : MonoBehaviour, IInteractable
{
    [System.Serializable]
    public class DialogueSequence
    {
        public string sequenceName; // Apenas para organização no Inspector
        public DialogueEntry[] dialogues;
    }

    [Header("Sequências de Diálogo")]
    [SerializeField] private DialogueSequence[] dialogueSequences;

    [Header("Configurações")]
    [Tooltip("Se true, volta para o primeiro diálogo após terminar todos")]
    [SerializeField] private bool loopDialogues = false;

    [Tooltip("Se true, repete o último diálogo infinitamente")]
    [SerializeField] private bool repeatLastDialogue = true;

    public UnityEvent OnDialogueEnd;

    private int currentSequenceIndex = 0;

    public void Interact()
    {
        if (!DialogueManager.Instance.CanStartDialogue) return;
        if (dialogueSequences.Length == 0) return;

        // Inicia o diálogo atual
        DialogueManager.Instance.StartDialogue(
            dialogueSequences[currentSequenceIndex].dialogues,
            OnDialogueComplete
        );
    }

    private void OnDialogueComplete()
    {
        // Avança para a próxima sequência
        currentSequenceIndex++;

        // Verifica se passou do limite
        if (currentSequenceIndex >= dialogueSequences.Length)
        {
            if (loopDialogues)
            {
                // Volta para o primeiro
                currentSequenceIndex = 0;
            }
            else if (repeatLastDialogue)
            {
                // Fica no último
                currentSequenceIndex = dialogueSequences.Length - 1;
            }
            // Se nenhum dos dois, mantém o índice além do limite (desabilita interação)

            OnDialogueEnd?.Invoke();
        }
    }

    public bool CanInteract()
    {
        // Se não tem loop nem repeat, desabilita após terminar todos
        if (!loopDialogues && !repeatLastDialogue)
        {
            if (currentSequenceIndex >= dialogueSequences.Length)
                return false;
        }

        return DialogueManager.Instance.CanStartDialogue;
    }

    /// <summary>
    /// Reseta para o primeiro diálogo (útil para eventos/triggers)
    /// </summary>
    public void ResetDialogue()
    {
        currentSequenceIndex = 0;
    }

    /// <summary>
    /// Pula para uma sequência específica
    /// </summary>
    public void SetDialogueSequence(int index)
    {
        currentSequenceIndex = Mathf.Clamp(index, 0, dialogueSequences.Length - 1);
    }
}