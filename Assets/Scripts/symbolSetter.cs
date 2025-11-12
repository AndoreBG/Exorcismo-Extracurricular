using UnityEngine;
using UnityEngine.UI;


public class symbolSetter : MonoBehaviour
{
    
    [Header("Sprites a serem Utilizados")]
    [SerializeField] private Sprite[] symbols;

    [Header("Prefab da base para Instanciar")]
    [SerializeField] private GameObject imagePrefab;

    [SerializeField] private int minSymbols;
    [SerializeField] private int maxSymbols;
    [SerializeField] private RectTransform frame;
    [SerializeField] private enemy enemy;

    void Awake()
    {
        frame = transform.Find("Canvas/frame").GetComponent<RectTransform>();

        if (frame == null)
            Debug.LogError($"{name} nao encontrado");
    }

    void Start()
    {
        if (frame != null && imagePrefab != null && symbols.Length > 0)
        {
            SpawnSymbols();
        }
        else
        {
            Debug.LogError($"{name} falta referencias para o invocar os simbolos");
        }
        enemy = GetComponent<enemy>();
        enemy.recebeDano();
    }

    private void SpawnSymbols()
    {
        int amount = Random.Range(minSymbols, maxSymbols + 1);
        amount = Mathf.Min(amount, symbols.Length);
        
        for (int i = 0; i < amount; i++)
        {
            Sprite simbolo = symbols[Random.Range(0, symbols.Length)];

            GameObject game = Instantiate(imagePrefab, frame);
            Image img = game.GetComponent<Image>();
            img.sprite = simbolo;
            img.SetNativeSize();

            RectTransform rt = game.GetComponent<RectTransform>();

            float frameWidth = frame.rect.width;
            float frameHeight = frame.rect.height;

            float spriteWidth = img.sprite.rect.width;
            float spriteHeight = img.sprite.rect.height;

            float scaleFactor = Mathf.Min(
                frameWidth / spriteWidth,
                frameHeight / spriteHeight
            ) * 0.75f;

            rt.sizeDelta = new Vector2(spriteWidth * scaleFactor, spriteHeight * scaleFactor);

            Vector2 pos = new Vector2(0.0f, 0.0f);

            rt.anchoredPosition = pos;
            rt.localScale = Vector3.one;
        }
    }
    public void Update()
    {

    }

    public void GenerateSymbol()
    {
        Debug.Log("Symbol ok");

    }
}
