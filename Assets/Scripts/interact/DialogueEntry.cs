// DialogueEntry.cs
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class DialogueEntry
{
    [Header("Personagem")]
    public string characterName;
    public Sprite characterSprite;

    [Header("Caixa de Diálogo")]
    public Sprite dialogueBoxSprite;
    public Color boxColor = Color.white;

    [Header("Caixa do Nome")]
    public Sprite nameBoxSprite;
    public Color nameBoxColor = Color.white;

    [Header("Texto")]
    [TextArea(2, 4)]
    public string text;

    [Header("Áudio")]
    public AudioResource typingSound;
}