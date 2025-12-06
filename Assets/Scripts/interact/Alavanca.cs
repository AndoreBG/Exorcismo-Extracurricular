using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class Alavanca : MonoBehaviour, IInteractable
{
    [Space]
    [SerializeField] GameObject closedDoor;
    [SerializeField] GameObject openedDoor;

    [Space]
    [SerializeField] GameObject notInteracted;
    [SerializeField] GameObject interacted;

    [Space]
    private bool wasInteracted = false;
    public UnityEvent IsOpen;

    public void Interact()
    {
        if (wasInteracted) return;
        Puxar();
    }

    private void Puxar()
    {
        Debug.Log("Puxou!");
        wasInteracted = true;

        notInteracted.SetActive(false);
        interacted.SetActive(true);

        IsOpen?.Invoke();
        StartCoroutine(OpenCoroutine());
    }

    IEnumerator OpenCoroutine()
    {
        closedDoor.SetActive(false);
        openedDoor.SetActive(true);

        yield return new WaitForSeconds(5f);
        Destroy(this.gameObject, 1f);
    }

    public bool CanInteract()
    {
        return !wasInteracted;
    }
}
