
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScene : MonoBehaviour
{
    public GameObject Pause;
    public GameObject GameOver;

    bool pausado = false;
    bool jogoAcabou = false;

    void Start()
    {
        // Esconde os painéis de pausa e Game Over ao iniciar
        Pause.SetActive(false);
        GameOver.SetActive(false);
    }

    void Update()
    {
        // Verifica se a tecla ESC foi pressionada
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pausado)
            {
                Continuar(); // Retoma o jogo
            }
            else
            {
                Pausar(); // Pausa o jogo
            }
        }
    }

    // Abre o menu de pausa e congela o jogo
    public void Pausar()
    {
        if (jogoAcabou == false)
        {
            pausado = true;
            Pause.SetActive(true);
            Time.timeScale = 0f; // Para o tempo do jogo
        }
    }

    // Fecha o menu de pausa e retoma o jogo
    public void Continuar()
    {
        pausado = false;
        Pause.SetActive(false);
        Time.timeScale = 1f; // Volta o tempo ao normal
    }

    // Mostra a tela de Game Over e congela o jogo
    public void MostrarGameOver()
    {
        jogoAcabou = true;
        GameOver.SetActive(true);
        Time.timeScale = 0f;
    }

    // Reinicia a fase atual
    public void Reiniciar()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Volta para a cena chamada Menu
    public void VoltarMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }
}
