using UnityEngine;

// Bala (usada pela shuriken do player e pela bala do inimigo)
public class Bala : MonoBehaviour
{
    public float velocidade = 7f;   // velocidade da bala
    public int direcao = 1;         // 1 = direita, -1 = esquerda

    void Start()
    {
        // A bala some sozinha depois de 3 segundos
        Destroy(gameObject, 3f);
    }

    void Update()
    {
        // Anda sempre em linha reta na direção escolhida
        transform.Translate(Vector2.right * direcao * velocidade * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Se acertar um inimigo, destrói o inimigo e a bala
        if (collision.CompareTag("Enemy"))
        {
            Destroy(collision.gameObject);
            Destroy(gameObject);
        }
    }
}
