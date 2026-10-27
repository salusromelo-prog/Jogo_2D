using UnityEngine;

// Camada de fundo que anda mais devagar que a câmera (dá a sensação de profundidade).
// fator 1 = acompanha a câmera (fica parada na tela). Quanto menor, mais ela "fica para trás".
public class Parallax : MonoBehaviour
{
    public float fator = 0.8f;
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

        transform.position = new Vector3(cam.position.x * fator, cam.position.y + deslocamentoY, 0f);
    }
}
