using UnityEngine;

// Seta que mostra para onde ir: balança um pouquinho na direção que aponta e pisca devagar
public class Seta : MonoBehaviour
{
    public Vector2 direcao = Vector2.right;     // para onde a seta aponta

    private Vector3 posicaoInicial;
    private SpriteRenderer desenho;

    void Start()
    {
        posicaoInicial = transform.position;
        desenho = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Vai e volta na direção da seta
        float movimento = Mathf.Sin(Time.time * 3f) * 0.12f;
        transform.position = posicaoInicial + (Vector3)(direcao * movimento);

        // Pisca suave
        float brilho = 0.75f + 0.25f * Mathf.Sin(Time.time * 3f);
        desenho.color = new Color(1f, 1f, 1f, brilho);
    }
}
