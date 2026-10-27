using UnityEngine;

// Faz uma luz (tocha, braseiro) tremer como fogo de verdade
public class PiscarLuz : MonoBehaviour
{
    private SpriteRenderer desenho;
    private float fase;             // cada tocha treme de um jeito diferente

    void Start()
    {
        desenho = GetComponent<SpriteRenderer>();
        fase = Random.Range(0f, 100f);
    }

    void Update()
    {
        // Ruído suave: o brilho varia entre 0,7 e 1
        float brilho = 0.7f + 0.3f * Mathf.PerlinNoise(Time.time * 6f, fase);
        desenho.color = new Color(1f, 1f, 1f, brilho);
    }
}
