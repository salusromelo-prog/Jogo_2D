using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PassarFase2 : MonoBehaviour
{
    public MenuScene menu;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Passarfase()
    {
        //vai carregar a cena de vitoria
        SceneManager.LoadScene("Win");
    }
}
