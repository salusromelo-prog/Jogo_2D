
using UnityEngine;

public class Bala : MonoBehaviour
{
    public float velocidade = 7f;
    public int direcao = 1; // 1 = direita e -1 = esquerda

    void Start()
    {
        // Destrói a bala depois de 3 segundos
        Destroy(gameObject, 3f);
    }

    void Update()
    {
        // Move a bala na direção definida
        transform.Translate(
            Vector2.right * direcao * velocidade * Time.deltaTime
        );
    }

    // Detecta quando a bala encosta em outro objeto
    void OnTriggerEnter2D(Collider2D collision)
    {
        // Verifica se o objeto atingido possui a tag Dano
        if (collision.CompareTag("Dano"))
        {
            Destroy(collision.gameObject); // Destrói o objeto atingido
            Destroy(gameObject); // Destrói a bala
        }
    }
}
