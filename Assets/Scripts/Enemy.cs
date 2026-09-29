using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class Enemy : MonoBehaviour
{
    public float velocidade = 2f;
    public float distancia = 2f;

    private float limiteEsquerdo;
    private float limiteDireito;
    private int direcao = 1;
     Animator animator;
    private SpriteRenderer spriteRenderer;


    void Start()
    {
        limiteEsquerdo = transform.position.x - distancia;
        limiteDireito = transform.position.x + distancia;
    }


    void Update()
    {
        transform.Translate(Vector2.right * direcao * velocidade * Time.deltaTime);

        if (transform.position.x >= limiteDireito)
        {
            direcao = -1;
            Virar();
        }

        if (transform.position.x <= limiteEsquerdo)
        {
            direcao = 1;
            Virar();
        }
        
    }

    void Virar()
    {
        Vector3 escala = transform.localScale;
        escala.x *= -1;
        transform.localScale = escala;
    }


}
