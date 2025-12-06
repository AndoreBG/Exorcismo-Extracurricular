using UnityEngine;
using UnityEngine.InputSystem;

public class InteractDetector : MonoBehaviour
{
    private IInteractable interactableRange = null;
    public GameObject interactionIcon;

    void Start()
    {
        interactionIcon.SetActive(false);
    }

    private void Update()
    {
        if (interactableRange != null)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                interactableRange?.Interact();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<IInteractable>(out IInteractable interactable) && interactable.CanInteract())
        {
            interactableRange = interactable;
            interactionIcon.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<IInteractable>(out IInteractable interactable) && interactable == interactableRange)
        {
            interactableRange = null;
            interactionIcon.SetActive(false);
        }
    }
}
