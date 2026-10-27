using UnityEngine;

// Faz a câmera seguir o player sem sair dos limites do mapa
public class CameraFollow : MonoBehaviour
{
    public Transform alvo;              // quem a câmera segue
    public float suavidade = 6f;        // quanto maior, mais rápido a câmera alcança o player
    public float alturaExtra = 2f;      // câmera um pouco acima do player (para ver mais o que vem em cima)
    public float alturaExtraQueda = -1.5f;  // quando o player cai rápido, a câmera olha mais para baixo

    // Limites do mapa (o LevelBuilder preenche)
    public float minX, maxX, minY, maxY;

    private Rigidbody2D corpoAlvo;

    void Start()
    {
        if (alvo != null)
        {
            corpoAlvo = alvo.GetComponent<Rigidbody2D>();
        }
    }

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
        float extra = alturaExtra;

        // Quanto mais rápido está caindo, mais a câmera olha para baixo
        if (corpoAlvo != null)
        {
            float velocidadeQueda = -corpoAlvo.linearVelocity.y;
            extra = Mathf.Lerp(alturaExtra, alturaExtraQueda, Mathf.InverseLerp(4f, 10f, velocidadeQueda));
        }

        float x = Mathf.Clamp(alvo.position.x, minX, maxX);
        float y = Mathf.Clamp(alvo.position.y + extra, minY, maxY);

        return new Vector3(x, y, transform.position.z);
    }
}
