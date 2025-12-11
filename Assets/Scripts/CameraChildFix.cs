using UnityEngine;

public class CameraChildFix : MonoBehaviour
{
    private Vector3 originalLocalPosition;

    void Awake()
    {
        originalLocalPosition = transform.localPosition;
    }

    void LateUpdate()
    {
        // Garantir que a posição local está correta
        // (às vezes a Cinemachine tenta mover para posição world)
        if (transform.localPosition != originalLocalPosition)
        {
            // Só reseta se estiver muito diferente (bug de teleporte)
            if (Vector3.Distance(transform.localPosition, originalLocalPosition) > 10f)
            {
                transform.localPosition = originalLocalPosition;
            }
        }
    }
}