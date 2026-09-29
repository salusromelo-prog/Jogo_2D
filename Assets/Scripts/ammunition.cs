using UnityEngine;

public class Bala : MonoBehaviour
{
    public float velocidade = 10f;
    public int direcao = 1;

    void Update()
    {
        transform.Translate(
            Vector2.right * direcao * velocidade * Time.deltaTime
        );
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Destroy(collision.gameObject);
            Destroy(gameObject);
        }
    }
}