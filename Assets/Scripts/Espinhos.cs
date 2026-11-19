using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Espinho : MonoBehaviour
{
    public MenuScene menu;
    void OnCollisionEnter2D(Collision2D collision)

        
    {
        //se encostar no player mostra o game over
        if (collision.gameObject.CompareTag("Player"))
        {
            menu.MostrarGameOver();
        }
    }
}