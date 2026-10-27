using UnityEngine;

// Perigo: espinho, inimigo e bala do inimigo usam este script.
// Se encostar no Player, ele perde uma vida (o GameManager cuida do resto).
public class Espinho : MonoBehaviour
{
    // Quando começa a encostar
    void OnCollisionEnter2D(Collision2D collision)
    {
        Machucar(collision);
    }

    // Enquanto continua encostando (para machucar de novo depois da invencibilidade)
    void OnCollisionStay2D(Collision2D collision)
    {
        Machucar(collision);
    }

    void Machucar(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            GameManager.PerderVida();

            // Se este objeto for uma bala, ela some depois de acertar
            if (GetComponent<Bala>() != null)
            {
                Destroy(gameObject);
            }
        }
    }
}
