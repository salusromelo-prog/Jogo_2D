
using UnityEngine;

// Classe responsável por controlar os tiros do jogador
public class PlayerTiro : MonoBehaviour
{
    // Guarda o objeto da bala que será instanciado
    public GameObject bala;

    // Marca a posição de onde a bala será disparada
    public Transform pontoDeTiro;

    // Guarda a posição do ponto de tiro quando o jogador olha para a direita
    private Vector3 posicaoDireita;

    // Guarda a posição do ponto de tiro quando o jogador olha para a esquerda
    private Vector3 posicaoEsquerda;

    // Executa uma vez quando o objeto começa
    void Start()
    {
        // Salva a posição inicial do ponto de tiro
        posicaoDireita = pontoDeTiro.localPosition;

        // Copia a posição da direita para criar a posição da esquerda
        posicaoEsquerda = posicaoDireita;

        // Inverte o valor X para colocar o ponto de tiro do lado esquerdo
        posicaoEsquerda.x *= -1;
    }

    // Executa várias vezes enquanto o jogo está rodando
    void Update()
    {
        // Verifica se o personagem está olhando para a esquerda
        if (GetComponent<SpriteRenderer>().flipX)
        {
            // Coloca o ponto de tiro na posição esquerda
            pontoDeTiro.localPosition = posicaoEsquerda;
        }
        else
        {
            // Coloca o ponto de tiro na posição direita
            pontoDeTiro.localPosition = posicaoDireita;
        }

        // Verifica se o jogador apertou a tecla F
        if (Input.GetKeyDown(KeyCode.F))
        {
            // Cria uma nova bala na posição do ponto de tiro
            GameObject novaBala = Instantiate(
                bala,
                pontoDeTiro.position,
                Quaternion.identity
            );

            // Pega o script Bala que está no objeto criado
            Bala scriptBala = novaBala.GetComponent<Bala>();

            // Verifica para qual lado o personagem está olhando
            if (GetComponent<SpriteRenderer>().flipX)
            {
                // Define a direção da bala para a esquerda
                scriptBala.direcao = -1;
            }
            else
            {
                // Define a direção da bala para a direita
                scriptBala.direcao = 1;
            }
        }
    }
}
