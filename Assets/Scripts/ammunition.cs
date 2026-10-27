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
        // Se acertar um inimigo, ele leva dano (o inimigo cuida do resto: som, vida, etc.)
        if (collision.CompareTag("Enemy"))
        {
            Enemy inimigo = collision.GetComponent<Enemy>();

            if (inimigo != null)
            {
                inimigo.LevarDano();
            }
            else
            {
                Destroy(collision.gameObject);
            }

            Destroy(gameObject);
        }
    }
}
