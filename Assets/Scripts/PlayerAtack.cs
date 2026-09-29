using UnityEngine;

public class PlayerTiro : MonoBehaviour
{
    public GameObject bala;
    public Transform pontoDeTiro;

    private Vector3 posicaoDireita;
    private Vector3 posicaoEsquerda;

    void Start()
    {
        posicaoDireita = pontoDeTiro.localPosition;

        posicaoEsquerda = posicaoDireita;
        posicaoEsquerda.x *= -1;
    }

    void Update()
    {
        // Se o personagem estiver olhando para a esquerda
        if (GetComponent<SpriteRenderer>().flipX)
        {
            pontoDeTiro.localPosition = posicaoEsquerda;
        }
        else
        {
            pontoDeTiro.localPosition = posicaoDireita;
        }

        // Atirar
        if (Input.GetKeyDown(KeyCode.F))
        {
            GameObject novaBala = Instantiate(
                bala,
                pontoDeTiro.position,
                Quaternion.identity
            );

            Bala scriptBala = novaBala.GetComponent<Bala>();

            if (GetComponent<SpriteRenderer>().flipX)
            {
                scriptBala.direcao = -1;
            }
            else
            {
                scriptBala.direcao = 1;
            }
        }
    }
}