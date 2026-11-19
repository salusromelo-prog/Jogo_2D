
using UnityEngine;

// Classe responsável pelo movimento e pelos tiros do inimigo
public class Enemy : MonoBehaviour
{
    // Velocidade de movimento do inimigo
    public float velocidade = 2f;

    // Distância que o inimigo percorre antes de mudar de direção
    public float distancia = 2f;

    // Cabeçalho que organiza as variáveis de tiro no Inspector
    [Header("Tiro")]

    // Objeto da bala que será disparada
    public GameObject bala;

    // Posição de onde a bala será criada
    public Transform pontoTiro;

    // Distância máxima para o inimigo conseguir atirar no jogador
    public float distanciaTiro = 6f;

    // Tempo de espera entre um tiro e outro
    public float tempoEntreTiros = 2f;

    // Limite esquerdo do movimento do inimigo
    private float limiteEsquerdo;

    // Limite direito do movimento do inimigo
    private float limiteDireito;

    // Define a direção do inimigo: 1 = direita e -1 = esquerda
    private int direcao = 1;

    // Controla o tempo de espera entre os tiros
    private float contadorTiro;

    // Guarda a posição do jogador
    private Transform jogador;


    // Executa uma vez quando o inimigo começa
    void Start()
    {
        // Define até onde o inimigo pode andar para a esquerda
        limiteEsquerdo = transform.position.x - distancia;

        // Define até onde o inimigo pode andar para a direita
        limiteDireito = transform.position.x + distancia;

        // Procura o objeto que possui a tag Player
        jogador = GameObject.FindGameObjectWithTag("Player").transform;
    }


    // Executa várias vezes enquanto o jogo está rodando
    void Update()
    {
        // Movimento do inimigo de acordo com sua direção e velocidade
        transform.Translate(Vector2.right * direcao * velocidade * Time.deltaTime);


        // Verifica se o inimigo chegou ao limite direito
        if (transform.position.x >= limiteDireito)
        {
            // Faz o inimigo andar para a esquerda
            direcao = -1;

            // Inverte a aparência do inimigo
            Virar();
        }

        // Verifica se o inimigo chegou ao limite esquerdo
        if (transform.position.x <= limiteEsquerdo)
        {
            // Faz o inimigo andar para a direita
            direcao = 1;

            // Inverte a aparência do inimigo
            Virar();
        }


        // Verifica se o inimigo pode atirar no jogador
        VerificarTiro();
    }


    // Função responsável por inverter o lado que o inimigo está olhando
    void Virar()
    {
        // Guarda a escala atual do inimigo
        Vector3 escala = transform.localScale;

        // Inverte o valor X da escala
        escala.x *= -1;

        // Aplica a nova escala ao inimigo
        transform.localScale = escala;
    }


    // Função que verifica se o jogador está perto e na frente do inimigo
    void VerificarTiro()
    {
        // Calcula a distância entre o jogador e o inimigo no eixo X
        float distanciaPlayer = Mathf.Abs(jogador.position.x - transform.position.x);

        // Verifica se o jogador está dentro do alcance do tiro
        if (distanciaPlayer <= distanciaTiro)
        {
            // Verifica se o inimigo está olhando para a direita
            // e se o jogador está à sua frente
            if (direcao == 1 && jogador.position.x > transform.position.x)
            {
                // Chama a função responsável por atirar
                Atirar();
            }

            // Verifica se o inimigo está olhando para a esquerda
            // e se o jogador está à sua frente
            if (direcao == -1 && jogador.position.x < transform.position.x)
            {
                // Chama a função responsável por atirar
                Atirar();
            }
        }
    }


    // Função responsável por criar e disparar uma bala
    void Atirar()
    {
        // Verifica se ainda existe tempo de espera para o próximo tiro
        if (contadorTiro > 0)
        {
            // Diminui o tempo de espera conforme o tempo do jogo passa
            contadorTiro -= Time.deltaTime;

            // Encerra a função até o tempo acabar
            return;
        }

        // Cria uma nova bala na posição do ponto de tiro
        GameObject novaBala = Instantiate(
            bala,
            pontoTiro.position,
            Quaternion.identity
        );

        // Acessa o script Bala do objeto que acabou de ser criado
        Bala balaScript = novaBala.GetComponent<Bala>();

        // Define a direção da bala igual à direção do inimigo
        balaScript.direcao = direcao;

        // Reinicia o contador de tempo entre os tiros
        contadorTiro = tempoEntreTiros;
    }
}
