using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField] private string pointName;

    // Propriedade pública para o PlayerSceneHandler acessar
    public string PointName => pointName;

    // Remova o Start() - não precisa mais fazer nada aqui!

    // Visualização no Editor
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Gizmos.DrawIcon(transform.position, "sv_icon_dot3_pix16_gizmo", true);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.7f);

#if UNITY_EDITOR
        // Mostrar o nome do spawn point
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.white;
        style.fontSize = 14;
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.8f, pointName, style);
#endif
    }
}