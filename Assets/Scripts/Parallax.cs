using UnityEngine;

// Camada de fundo que anda mais devagar que a câmera (dá a sensação de profundidade).
// fator 1 = acompanha a câmera (fica parada na tela). Quanto menor, mais ela "fica para trás".
// fator maior que 1 = anda mais rápido que a câmera (usado em coisas na frente do jogo).
public class Parallax : MonoBehaviour
{
    public float fator = 0.8f;
    public float fatorY = 1f;           // o mesmo para a subida e a descida (1 = fica presa na altura da câmera)
    public float deslocamentoX = 0f;    // empurra a camada para o lado
    public float deslocamentoY = 0f;    // altura em relação à câmera

    private Transform cam;

    void LateUpdate()
    {
        if (cam == null)
        {
            if (Camera.main == null)
            {
                return;
            }

            cam = Camera.main.transform;
        }

        transform.position = new Vector3(cam.position.x * fator + deslocamentoX, cam.position.y * fatorY + deslocamentoY, 0f);
    }
}
