using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class symbolUIDisplay : MonoBehaviour
{
    [Header("=== Imagens Base ===")]
    [SerializeField] private Image corteImage;
    [SerializeField] private Image quinaImage;
    [SerializeField] private Image luaImage;

    [Header("=== Opcional ===")]
    [SerializeField] private TextMeshProUGUI rotationText;
    [SerializeField] private Image[] rotationDots; // 4 dots

    private magicSystem magicSystem;

    void Start()
    {
        magicSystem = FindFirstObjectByType<magicSystem>();

        if (magicSystem != null)
        {
            magicSystem.OnRotationChanged.AddListener(UpdateDisplay);
        }

        UpdateDisplay(0);
    }

    public void UpdateDisplay(int rotationIndex)
    {
        // ← MUDANÇA: Converter índice em ângulo HORÁRIO (negativo)
        int angle = GetRotationAngle(rotationIndex);
        int corteAngle = GetCorteRotationAngle(rotationIndex);

        // Rotacionar imagens diretamente (sentido horário)
        if (corteImage != null)
            corteImage.transform.localRotation = Quaternion.Euler(0, 0, corteAngle);

        if (quinaImage != null)
            quinaImage.transform.localRotation = Quaternion.Euler(0, 0, angle);

        if (luaImage != null)
            luaImage.transform.localRotation = Quaternion.Euler(0, 0, angle);

        // Texto opcional
        if (rotationText != null)
        {
            if (angle == 0)
                rotationText.text = "0°";
            else
                rotationText.text = $"{angle}°"; // Mostra -90, -180, -270
        }

        // Dots de rotação
        UpdateRotationDots(rotationIndex);

        Debug.Log($"UI atualizada: índice {rotationIndex} = {angle}°");
    }

    // ← NOVO: Converter índice de rotação em ângulo horário
    int GetRotationAngle(int rotationIndex)
    {
        switch (rotationIndex)
        {
            case 0: return 0;
            case 1: return -90;
            case 2: return -180;
            case 3: return -270;
            default: return 0;
        }
    }

    // ← NOVO: Corte só tem 2 variações
    int GetCorteRotationAngle(int rotationIndex)
    {
        int index = rotationIndex % 2;
        return index == 0 ? 0 : -90;
    }

    void UpdateRotationDots(int rotationIndex)
    {
        if (rotationDots == null || rotationDots.Length < 4) return;

        for (int i = 0; i < 4; i++)
        {
            if (rotationDots[i] != null)
            {
                rotationDots[i].color = (i == rotationIndex) ?
                    Color.white : new Color(1f, 1f, 1f, 0.3f);
            }
        }
    }
}