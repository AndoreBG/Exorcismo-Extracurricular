using UnityEngine;
using UnityEngine.UI;

public class enemy : MonoBehaviour
{
	//comportamento pradrão dos inimigos
	
    [Header("Componentes")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private BoxCollider2D collider;
    [SerializeField] private Animator animator;
	
    [Header("Movimentação")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private bool isFacingRight = true;

	[Header("Status")]
    [SerializeField] private float maxSymbol;
	[SerializeField] private RectTransform frame;
	
    private symbolSetter symbolGen;
    private bool isJumping = false;


    void Start()
    {
        symbolGen = GetComponent<symbolSetter>();
        symbolGen.GenerateSymbol(); //Um teste
		
		//O find procura propriamente o nome, como em caminho de arquivo.
		frame = transform.Find("Canvas/frame").GetComponent<RectTransform>();
    }

    void Update()
    {
		movement();
        flip();
		IsGrounded();
    }
	
	//TODO - Movimento padrão
	public void movement(){
		
		animator.SetBool("Moving", true); //Definir a animator

	}

	// TODO - Extraido do avatar, parametrizar para o inimigo
	void flip()
    {
        if (!isFacingRight)
        {
            isFacingRight = !isFacingRight;
            Vector3 scaler = transform.localScale;
            scaler.x *= -1;
            transform.localScale = scaler;
        }
    }

	// O avatar "manda" uma string para essa função, string essa que é o nome da sprite do simbolo
    public void recebeDano(string idSymbol){
        
		animator.SetBool("Hurting", true); //Definir a animator
		
        // ERRO --
        foreach (Image imagem in frame)
        {
            Debug.Log(imagem.sprite.name);
			//if(imagem.sourceImage.name == idSymbol){
			//	Destroy(imagem);
			//}
            
        }
		animator.SetBool("Hurting", false); //Definir a animator
		//if(){
		//	enemyDie();
		//}
		recuoGolpeado();
		// TODO - Definir transições
    }
	
	public void enemyDie(){
		animator.SetBool("Die", true);
		//Destroy.GameObject(this); // TODO - Passivel de ERRO
	}
	
	// TODO - Criar um collider pro bendito aqui
    private void IsGrounded()
    {
        if (collider.IsTouchingLayers(groundLayer) || collider.IsTouchingLayers(obstacleLayer))
        {
            isJumping = false;
        }
        else
        {
            isJumping = true;
        }
    }
	
	//
	private void recuoGolpeado(){
		// da pra colocar aqui o lance de forcas para fisica, ao bater no inimigo ele recebe uma determinada forca e se move
		// Mas, se o inimigo bater em uma parede, ele inverte a força, ou seja, quica na parede e volta
	}
}
