using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SymbolUIDisplay : MonoBehaviour
{
    [Header("=== Imagens dos Símbolos ===")]
    [SerializeField] private Image corteImage;
    [SerializeField] private Image quinaImage;
    [SerializeField] private Image luaImage;

    [Header("=== Sprites dos Símbolos ===")]
    [SerializeField] private SymbolSprites symbolSprites;

    [Header("=== Texto de Rotação (Opcional) ===")]
    [SerializeField] private TextMeshProUGUI rotationText;

    [Header("=== Indicador de Rotação (Opcional) ===")]
    [SerializeField] private Image[] rotationDots; // 4 dots para indicar 0°, 90°, 180°, 270°

    private avatarEnergy playerEnergy;
    private int currentRotation = 0;

    void Start()
    {
        // Encontrar o avatarEnergy
        playerEnergy = FindFirstObjectByType<avatarEnergy>();

        if (playerEnergy != null)
        {
            // Conectar ao evento de mudança de rotação
            playerEnergy.OnRotationChanged.AddListener(UpdateDisplay);
        }

        // Atualizar display inicial
        UpdateDisplay(0);
    }

    public void UpdateDisplay(int rotation)
    {
        currentRotation = rotation;

        UpdateSymbolSprites();
        UpdateRotationText();
        UpdateRotationDots();
    }

    void UpdateSymbolSprites()
    {
        if (symbolSprites == null) return;

        // Atualizar Corte (só tem 2 variações: 0° e 90°)
        if (corteImage != null)
        {
            int corteRotation = currentRotation % 2;
            corteImage.sprite = corteRotation == 0 ? symbolSprites.corte : symbolSprites.corte90;
        }

        // Atualizar Quina (4 variações)
        if (quinaImage != null)
        {
            switch (currentRotation)
            {
                case 0: quinaImage.sprite = symbolSprites.quina; break;
                case 1: quinaImage.sprite = symbolSprites.quina90; break;
                case 2: quinaImage.sprite = symbolSprites.quina180; break;
                case 3: quinaImage.sprite = symbolSprites.quina270; break;
            }
        }

        // Atualizar Lua (4 variações)
        if (luaImage != null)
        {
            switch (currentRotation)
            {
                case 0: luaImage.sprite = symbolSprites.lua; break;
                case 1: luaImage.sprite = symbolSprites.lua90; break;
                case 2: luaImage.sprite = symbolSprites.lua180; break;
                case 3: luaImage.sprite = symbolSprites.lua270; break;
            }
        }
    }

    void UpdateRotationText()
    {
        if (rotationText != null)
        {
            rotationText.text = $"Rotação: {currentRotation * 90}°";
        }
    }

    void UpdateRotationDots()
    {
        if (rotationDots == null || rotationDots.Length < 4) return;

        // Destacar apenas o dot da rotação atual
        for (int i = 0; i < rotationDots.Length; i++)
        {
            if (rotationDots[i] != null)
            {
                // Dot ativo = branco, inativo = cinza transparente
                rotationDots[i].color = (i == currentRotation) ?
                    Color.white : new Color(1f, 1f, 1f, 0.3f);
            }
        }
    }
}