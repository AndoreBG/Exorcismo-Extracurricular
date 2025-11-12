using UnityEngine;
using UnityEngine.UI;

public class enemy : MonoBehaviour
{


    [SerializeField] private float health;
    [SerializeField] symbolSetter symbolGen;


    void Start()
    {
        symbolGen = GetComponent<symbolSetter>();
        
        symbolGen.GenerateSymbol();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void recebeDano(){
        RectTransform frame = transform.Find("Canvas/frame").GetComponent<RectTransform>();

        // ERRO --
        foreach (Image imagem in frame)
        {
            Debug.Log(imagem.sprite.name);
            
        }
    }
}
