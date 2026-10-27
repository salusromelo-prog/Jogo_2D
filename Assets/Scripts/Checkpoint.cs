using UnityEngine;

// Altar de lanterna: quando o Player passa por ele, a lanterna acende
// e o player passa a voltar para cá (e não para o início da fase) quando perder uma vida.
public class Checkpoint : MonoBehaviour
{
    public Sprite spriteAceso;      // imagem do altar aceso

    private bool ativo = false;

    void OnTriggerEnter2D(Collider2D outro)
    {
        if (ativo || !outro.CompareTag("Player"))
        {
            return;
        }

        ativo = true;
        GetComponent<SpriteRenderer>().sprite = spriteAceso;

        // Guarda o lugar onde o player está agora como novo ponto de retorno
        GameManager.DefinirCheckpoint(outro.transform.position);
        GameManager.AltarAceso();
        GameManager.Tocar("checkpoint");
    }
}
