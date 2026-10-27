using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float velocidade = 2f;   // velocidade andando
    public float distancia = 2f;    // quanto anda para cada lado a partir do ponto inicial

    [Header("Tiro")]
    public GameObject bala;             // prefab da bala do inimigo
    public Transform pontoTiro;         // de onde a bala sai
    public float distanciaTiro = 6f;    // distância máxima para ele atirar no player
    public float tempoEntreTiros = 2f;  // tempo de espera entre um tiro e outro

    private float limiteEsquerdo;
    private float limiteDireito;
    private int direcao = 1;    // 1 = direita, -1 = esquerda

    private float contadorTiro; // conta o tempo de espera do tiro

    private Transform jogador;
    private Animator animator;


    void Start()
    {
        // Os limites da patrulha ficam ao redor de onde o inimigo começou
        limiteEsquerdo = transform.position.x - distancia;
        limiteDireito = transform.position.x + distancia;

        jogador = GameObject.FindGameObjectWithTag("Player").transform;
        animator = GetComponent<Animator>();
    }


    void Update()
    {
        // Jogo parado (menu ou Game Over): não faz nada
        if (Time.timeScale == 0)
        {
            return;
        }

        // Anda na direção atual
        transform.Translate(Vector2.right * direcao * velocidade * Time.deltaTime);

        // Chegou no limite da direita: volta para a esquerda
        if (transform.position.x >= limiteDireito)
        {
            direcao = -1;
            Virar();
        }

        // Chegou no limite da esquerda: volta para a direita
        if (transform.position.x <= limiteEsquerdo)
        {
            direcao = 1;
            Virar();
        }

        VerificarTiro();
    }


    // Espelha o inimigo para o outro lado
    void Virar()
    {
        Vector3 escala = transform.localScale;
        escala.x *= -1;
        transform.localScale = escala;
    }


    // Só atira se o player estiver perto e na frente dele
    void VerificarTiro()
    {
        float distanciaPlayer = Mathf.Abs(jogador.position.x - transform.position.x);

        if (distanciaPlayer <= distanciaTiro)
        {
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
        // Ainda esperando o tempo entre tiros
        if (contadorTiro > 0)
        {
            contadorTiro -= Time.deltaTime;
            return;
        }

        // Cria a bala e manda ela na direção do inimigo
        GameObject novaBala = Instantiate(bala, pontoTiro.position, Quaternion.identity);
        novaBala.GetComponent<Bala>().direcao = direcao;
        GameManager.Tocar("tiro_inimigo");

        // Toca a animação de ataque (só se o inimigo tiver um Animator com controller)
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            animator.SetTrigger("Atacar");
        }

        contadorTiro = tempoEntreTiros;
    }
}
