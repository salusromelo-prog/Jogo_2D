using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PassarFase : MonoBehaviour
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
        //vai carregar a fase2 
        SceneManager.LoadScene("Fase2");
    }
}
