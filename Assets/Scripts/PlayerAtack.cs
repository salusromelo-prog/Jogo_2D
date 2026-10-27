using UnityEngine;

// Tiro do player (fica no mesmo objeto do Player)
public class PlayerTiro : MonoBehaviour
{
    public GameObject bala;         // prefab da shuriken
    public Transform pontoDeTiro;   // de onde a shuriken sai

    private Vector3 posicaoDireita;
    private Vector3 posicaoEsquerda;

    void Start()
    {
        // Guarda a posição do ponto de tiro para os dois lados
        posicaoDireita = pontoDeTiro.localPosition;

        posicaoEsquerda = posicaoDireita;
        posicaoEsquerda.x *= -1;
    }

    void Update()
    {
        // flipX = true significa que o personagem olha para a esquerda
        bool olhandoEsquerda = GetComponent<SpriteRenderer>().flipX;

        // Coloca o ponto de tiro do lado certo
        if (olhandoEsquerda)
        {
            pontoDeTiro.localPosition = posicaoEsquerda;
        }
        else
        {
            pontoDeTiro.localPosition = posicaoDireita;
        }

        // Aperta F para atirar
        if (Input.GetKeyDown(KeyCode.F))
        {
            GameObject novaBala = Instantiate(bala, pontoDeTiro.position, Quaternion.identity);

            Bala scriptBala = novaBala.GetComponent<Bala>();

            // A shuriken vai para o lado que o personagem está olhando
            if (olhandoEsquerda)
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
