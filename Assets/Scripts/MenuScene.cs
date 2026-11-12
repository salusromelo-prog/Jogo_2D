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
        Pause.SetActive(false);
        GameOver.SetActive(false);

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pausado)
            {
                Continuar();
            }
            else
            {
                Pausar();
            }
        }
    }

    public void Pausar()
    {
        if (jogoAcabou == false)
        {
            pausado = true;
            Pause.SetActive(true);
            Time.timeScale = 0f;
        }
    }
    public void Continuar()
    {
        pausado = false;
        Pause.SetActive(false);
        Time.timeScale = 1f;
    }
    public void MostrarGameOver()
    {
        jogoAcabou = true;
        GameOver.SetActive(true);
        Time.timeScale = 0f;
    }
    public void Reiniciar()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void VoltarMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }

}
