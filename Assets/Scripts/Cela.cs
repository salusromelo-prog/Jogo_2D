using UnityEngine;

// Cela do amigo, no fim da última fase.
// Só abre depois que o chefe for derrotado; ao abrir, o jogo termina.
public class Cela : MonoBehaviour
{
    public GameObject grade;        // as grades que escondem o amigo

    private bool aberta = false;

    void OnTriggerEnter2D(Collider2D outro)
    {
        if (aberta || !outro.CompareTag("Player"))
        {
            return;
        }

        // O chefe ainda está vivo: avisa o que fazer
        if (GameManager.ChefeVivo())
        {
            GameManager.MostrarAviso("A cela está trancada. Derrote o capitão da guarda!");
            return;
        }

        aberta = true;

        if (grade != null)
        {
            Destroy(grade);     // as grades somem
        }

        // Um instante para ver o amigo livre, e então termina o jogo
        Invoke("Terminar", 1.4f);
    }

    void Terminar()
    {
        GameManager.FimDaFase();
    }
}
