using UnityEngine;

// Portão no fim da fase: quando o Player entra, a fase termina
public class Portao : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D outro)
    {
        if (outro.CompareTag("Player"))
        {
            GameManager.FimDaFase();
        }
    }
}
