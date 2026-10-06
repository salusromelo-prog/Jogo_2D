using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float velocidade = 2f;
    public float distancia = 2f;

    [Header("Tiro")]
    public GameObject bala;
    public Transform pontoTiro;
    public float distanciaTiro = 6f;
    public float tempoEntreTiros = 2f;

    private float limiteEsquerdo;
    private float limiteDireito;
    private int direcao = 1;

    private float contadorTiro;

    private Transform jogador;


    void Start()
    {
        limiteEsquerdo = transform.position.x - distancia;
        limiteDireito = transform.position.x + distancia;

        jogador = GameObject.FindGameObjectWithTag("Player").transform;
    }


    void Update()
    {
        // Movimento do inimigo
        transform.Translate(Vector2.right * direcao * velocidade * Time.deltaTime);


        // Faz o inimigo virar
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


        // Verifica se pode atirar
        VerificarTiro();
    }


    void Virar()
    {
        Vector3 escala = transform.localScale;
        escala.x *= -1;
        transform.localScale = escala;
    }


    void VerificarTiro()
    {
        float distanciaPlayer = Mathf.Abs(jogador.position.x - transform.position.x);

        if (distanciaPlayer <= distanciaTiro)
        {
            // Player está na frente do inimigo
            if (direcao == 1 && jogador.position.x > transform.position.x)
            {
                Atirar();
            }

            if (direcao == -1 && jogador.position.x < transform.position.x)
            {
                Atirar();
            }
        }
    }


    void Atirar()
    {
        if (contadorTiro > 0)
        {
            contadorTiro -= Time.deltaTime;
            return;
        }

        GameObject novaBala = Instantiate(
            bala,
            pontoTiro.position,
            Quaternion.identity
        );

        Bala balaScript = novaBala.GetComponent<Bala>();

        balaScript.direcao = direcao;

        contadorTiro = tempoEntreTiros;
    }
}