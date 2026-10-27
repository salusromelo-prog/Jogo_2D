using UnityEngine;

// Faz a câmera seguir o player sem sair dos limites do mapa
public class CameraFollow : MonoBehaviour
{
    public Transform alvo;              // quem a câmera segue
    public float suavidade = 6f;        // quanto maior, mais rápido a câmera alcança o player
    public float alturaExtra = 2f;      // câmera um pouco acima do player (para ver mais o que vem em cima)

    // Limites do mapa (o LevelBuilder preenche)
    public float minX, maxX, minY, maxY;

    void LateUpdate()
    {
        if (alvo == null)
        {
            return;
        }

        // Anda suavemente até o destino
        transform.position = Vector3.Lerp(transform.position, Destino(), suavidade * Time.deltaTime);
    }

    // Coloca a câmera direto no destino (sem animação)
    public void IrParaOAlvo()
    {
        transform.position = Destino();
    }

    Vector3 Destino()
    {
        float x = Mathf.Clamp(alvo.position.x, minX, maxX);
        float y = Mathf.Clamp(alvo.position.y + alturaExtra, minY, maxY);

        return new Vector3(x, y, transform.position.z);
    }
}
