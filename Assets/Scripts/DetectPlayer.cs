using UnityEngine;

public class DetectPlayer : MonoBehaviour
{
    private GameObject player;

    private void Start()
    {
        GetPlayer();
    }

    private void LateUpdate()
    {
        if (player == null)
        {
            GetPlayer();
        }
    }

    public void GetPlayer()
    {
        player = GameObject.FindFirstObjectByType<avatarMovement>().gameObject;
    }
}
