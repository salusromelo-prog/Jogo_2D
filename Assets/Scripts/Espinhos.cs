using UnityEngine;
using UnityEngine.SceneManagement;

// Perigo: espinho, inimigo e bala do inimigo usam este script.
// Se encostar no Player, a fase recomeça.
public class Espinho : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Carrega a cena de número 0 (a primeira do Build Settings)
            SceneManager.LoadScene(0);
        }
    }
}
