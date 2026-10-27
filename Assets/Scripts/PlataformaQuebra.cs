using UnityEngine;

// Plataforma que quebra: o player pisa, ela treme, cai e volta depois de um tempo
public class PlataformaQuebra : MonoBehaviour
{
    public float tempoParaCair = 0.6f;      // tempo tremendo antes de sumir
    public float tempoParaVoltar = 3f;      // tempo até ela aparecer de novo

    private bool tremendo = false;
    private bool sumiu = false;
    private float contador;
    private Vector3 posicaoOriginal;

    private SpriteRenderer desenho;
    private BoxCollider2D colisor;

    void Start()
    {
        desenho = GetComponent<SpriteRenderer>();
        colisor = GetComponent<BoxCollider2D>();
        posicaoOriginal = transform.position;
    }

    void OnCollisionEnter2D(Collision2D colisao)
    {
        // Só começa a quebrar se o player pisou em cima (os pés dele estão acima do meio da plataforma)
        bool pisouEmCima = colisao.collider.bounds.min.y > transform.position.y;

        if (!tremendo && !sumiu && colisao.collider.CompareTag("Player") && pisouEmCima)
        {
            tremendo = true;
            contador = tempoParaCair;
            GameManager.Tocar("quebra");
        }
    }

    void Update()
    {
        // Jogo parado (menu ou Game Over): não faz nada
        if (Time.timeScale == 0)
        {
            return;
        }

        if (tremendo)
        {
            contador -= Time.deltaTime;

            // Treme de um lado para o outro
            transform.position = posicaoOriginal + new Vector3(Random.Range(-0.04f, 0.04f), 0f, 0f);

            if (contador <= 0)
            {
                // Some
                tremendo = false;
                sumiu = true;
                contador = tempoParaVoltar;
                transform.position = posicaoOriginal;
                desenho.enabled = false;
                colisor.enabled = false;
            }
        }
        else if (sumiu)
        {
            contador -= Time.deltaTime;

            if (contador <= 0)
            {
                // Volta
                sumiu = false;
                desenho.enabled = true;
                colisor.enabled = true;
            }
        }
    }
}
