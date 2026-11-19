
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform alvo; // Objeto que a câmera deve seguir

    public Vector3 distancia = new Vector3(0f, 0f, -10f); // Distância entre a câmera e o jogador

    public bool movimentoSuave = true; // Ativa ou desativa o movimento suave

    public float velociade = 5f; // Velocidade do movimento da câmera

    void LateUpdate()
    {
        // Calcula a posição que a câmera deve alcançar
        Vector3 posicaoDesejada = alvo.position + distancia;

        // Verifica se o movimento suave está ativado
        if (movimentoSuave)
        {
            // Move a câmera suavemente até a posição desejada
            transform.position = Vector3.Lerp(transform.position, posicaoDesejada, velociade * Time.deltaTime);
        }
        else
        {
            // Move a câmera diretamente para a posição desejada
            transform.position = posicaoDesejada;
        }
    }
}
