using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class CaiNoBuraco : MonoBehaviour
{
    [Space]
    [Header("=== Componentes ===")]
    [SerializeField] Grid grid;
    [SerializeField] BoxCollider2D buracoCollider;
    [Space]
    [SerializeField] avatarMovement player;
    [SerializeField] LayerMask playerLayer;
    [Space]
    [SerializeField] GameObject transitionHall;
    [SerializeField] GameObject transitionHallCursed;
    [SerializeField] GameObject doorOpened;
    [SerializeField] GameObject doorClosed;

    private LevelManager levelManager;
    private bool playerInHole = false;

    [Header("== Eventos (HUD) ==")]
    public UnityEvent OnTransitionEvent;

    void FixedUpdate()
    {
        if(player == null)
        {
            player = FindFirstObjectByType<avatarMovement>();
            levelManager = FindFirstObjectByType<LevelManager>();
        }

        TouchPlayer();

        if(playerInHole)
        {
            OnTransitionEvent?.Invoke();
            levelManager.TransitionEvent();
            StartCoroutine(PlayerTransport());
            Destroy(this.gameObject, 5f);
        }
        else
        {
            return;
        }
    }

    private void TouchPlayer()
    {
        if(buracoCollider.IsTouchingLayers(playerLayer))
        {
            playerInHole = true;

        }
        else 
        {
            playerInHole = false;
        }
    }

    IEnumerator PlayerTransport()
    {
        yield return new WaitForSeconds(1.1f);
        grid.enabled = false;
        player.PlayerTeletransport(-41f, -11.5f);
    }

}
