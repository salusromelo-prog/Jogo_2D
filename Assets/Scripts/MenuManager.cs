
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    // Inicia o jogo carregando a cena Jogo
    public void StartGame()
    {
        SceneManager.LoadScene("Jogo");
    }

    // Fecha o jogo quando o botão de sair é pressionado
    public void QuitGame()
    {
        Application.Quit();
    }
}
