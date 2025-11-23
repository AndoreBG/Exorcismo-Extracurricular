using UnityEngine;

public class moveLimitMarker : MonoBehaviour
{
    [SerializeField] private Color gizmoColor = Color.yellow;
    [SerializeField] private float gizmoSize = 0.5f;

    void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;

        // Cruz
        Gizmos.DrawLine(transform.position + Vector3.up * gizmoSize,
                       transform.position + Vector3.down * gizmoSize);
        Gizmos.DrawLine(transform.position + Vector3.left * gizmoSize * 0.5f,
                       transform.position + Vector3.right * gizmoSize * 0.5f);

        // Círculo
        Gizmos.DrawWireSphere(transform.position, gizmoSize * 0.3f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.5f);
        Gizmos.DrawSphere(transform.position, gizmoSize * 0.4f);

#if UNITY_EDITOR
        UnityEditor.Handles.color = gizmoColor;
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.7f, gameObject.name);
#endif
    }
}