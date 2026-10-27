using UnityEngine;

// Vaga-lumes que flutuam pela tela (só enfeite, dá clima à floresta)
public class Vagalumes : MonoBehaviour
{
    public Sprite sprite;           // imagem do brilho
    public int quantidade = 45;
    public float tamanhoMin = 0.5f;     // tamanho das luzinhas
    public float tamanhoMax = 1.1f;
    public float intensidade = 1f;      // 1 = brilho forte, menor = mais fraquinho

    // Os vaga-lumes ficam dentro de uma área ao redor da câmera. Quando saem de um lado, entram do outro.
    private const float MetadeLargura = 12f;
    private const float MetadeAltura = 7f;

    private Transform[] pontos;
    private SpriteRenderer[] desenhos;
    private Vector2[] posicoes;     // posição "no mundo" de cada vaga-lume
    private float[] fases;          // faz cada um se mexer e piscar em um ritmo diferente

    void Start()
    {
        pontos = new Transform[quantidade];
        desenhos = new SpriteRenderer[quantidade];
        posicoes = new Vector2[quantidade];
        fases = new float[quantidade];

        for (int i = 0; i < quantidade; i++)
        {
            GameObject obj = new GameObject("Vagalume");
            obj.transform.parent = transform;

            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingLayerName = "Ground";
            sr.sortingOrder = 5;

            float tamanho = Random.Range(tamanhoMin, tamanhoMax);
            obj.transform.localScale = new Vector3(tamanho, tamanho, 1f);

            pontos[i] = obj.transform;
            desenhos[i] = sr;
            posicoes[i] = new Vector2(Random.Range(-MetadeLargura, MetadeLargura), Random.Range(-MetadeAltura, MetadeAltura));
            fases[i] = Random.Range(0f, 6.28f);
        }
    }

    void Update()
    {
        if (Camera.main == null)
        {
            return;
        }

        Vector2 posCamera = Camera.main.transform.position;

        for (int i = 0; i < quantidade; i++)
        {
            // Balança devagar
            Vector2 balanco = new Vector2(
                Mathf.Sin(Time.time * 0.4f + fases[i]) * 1.2f,
                Mathf.Cos(Time.time * 0.3f + fases[i] * 1.7f) * 0.8f);

            // Posição em relação à câmera, "dando a volta" quando passa da borda da área
            Vector2 relativa = posicoes[i] + balanco - posCamera;
            relativa.x = Mathf.Repeat(relativa.x + MetadeLargura, 2f * MetadeLargura) - MetadeLargura;
            relativa.y = Mathf.Repeat(relativa.y + MetadeAltura, 2f * MetadeAltura) - MetadeAltura;

            pontos[i].position = posCamera + relativa;

            // Pisca
            float brilho = 0.35f + 0.65f * (0.5f + 0.5f * Mathf.Sin(Time.time * 1.6f + fases[i] * 3f));
            desenhos[i].color = new Color(1f, 1f, 1f, brilho * intensidade);
        }
    }
}
