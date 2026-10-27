using UnityEngine;
using UnityEngine.SceneManagement;

// Controla o jogo: menu inicial, vidas, Game Over, música e sons.
// Fica em um objeto vazio chamado GameManager na cena.
public class GameManager : MonoBehaviour
{
    public int vidasIniciais = 3;       // quantas vidas o player começa
    public float tempoInvencivel = 1.5f; // tempo sem levar dano depois de perder uma vida
    public float limiteQueda = -15f;    // se o player cair abaixo dessa altura, perde uma vida

    // "static" faz o valor continuar mesmo quando a cena é recarregada
    static int vidas = -1;
    static bool jaComecou = false;      // false = ainda está no menu inicial

    static GameManager instancia;       // para outros scripts chamarem o GameManager

    bool gameOver = false;
    float invencivelTimer = 0f;

    Transform jogador;
    Rigidbody2D jogadorRb;
    SpriteRenderer jogadorSprite;
    Vector3 posicaoInicial;


    void Awake()
    {
        instancia = this;

        // Na primeira vez que o jogo abre, define as vidas
        if (vidas < 0)
        {
            vidas = vidasIniciais;
        }

        // No menu o jogo fica parado (timeScale 0). Jogando, roda normal (1).
        if (jaComecou)
        {
            Time.timeScale = 1f;
        }
        else
        {
            Time.timeScale = 0f;
        }
    }


    void Start()
    {
        // Procura o player e guarda onde ele começou (para voltar quando perder vida)
        GameObject objJogador = GameObject.FindGameObjectWithTag("Player");
        jogador = objJogador.transform;
        jogadorRb = objJogador.GetComponent<Rigidbody2D>();
        jogadorSprite = objJogador.GetComponent<SpriteRenderer>();
        posicaoInicial = jogador.position;

        // Música de fundo (arquivo em Assets/Resources/Sons/musica)
        AudioClip clipMusica = Resources.Load<AudioClip>("Sons/musica");
        if (clipMusica != null)
        {
            AudioSource musica = gameObject.AddComponent<AudioSource>();
            musica.clip = clipMusica;
            musica.loop = true;
            musica.volume = 0.4f;
            musica.Play();
        }
    }


    void Update()
    {
        // Enter também começa / reinicia o jogo
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (!jaComecou)
            {
                Comecar();
            }
            else if (gameOver)
            {
                Reiniciar();
            }
        }

        if (!jaComecou || gameOver)
        {
            return;
        }

        // Depois de perder vida o player fica piscando e sem levar dano por um tempo
        if (invencivelTimer > 0)
        {
            invencivelTimer -= Time.deltaTime;
            jogadorSprite.enabled = ((int)(Time.time * 10) % 2 == 0); // pisca

            if (invencivelTimer <= 0)
            {
                jogadorSprite.enabled = true;
            }
        }

        // Caiu para fora do mapa
        if (jogador.position.y < limiteQueda)
        {
            if (invencivelTimer > 0)
            {
                VoltarAoInicio(); // ainda invencível: só volta, sem perder outra vida
            }
            else
            {
                TirarVida();
            }
        }
    }


    // Os outros scripts chamam este método para machucar o player
    public static void PerderVida()
    {
        if (instancia == null)
        {
            // Se não tiver GameManager na cena, só reinicia a fase
            SceneManager.LoadScene(0);
            return;
        }

        instancia.TirarVida();
    }


    // Toca um som da pasta Assets/Resources/Sons
    public static void Tocar(string nome)
    {
        AudioClip clip = Resources.Load<AudioClip>("Sons/" + nome);

        if (clip != null && Camera.main != null)
        {
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);
        }
    }


    void TirarVida()
    {
        // Não leva dano no menu, no Game Over ou enquanto está invencível
        if (!jaComecou || gameOver || invencivelTimer > 0)
        {
            return;
        }

        vidas--;
        Tocar("dano");

        if (vidas <= 0)
        {
            gameOver = true;
            Time.timeScale = 0f;
            jogadorSprite.enabled = true;
            Tocar("gameover");
            return;
        }

        VoltarAoInicio();
        invencivelTimer = tempoInvencivel;
    }


    // Coloca o player de volta no ponto inicial
    void VoltarAoInicio()
    {
        jogador.position = posicaoInicial;
        jogadorRb.linearVelocity = Vector2.zero;
    }


    void Comecar()
    {
        jaComecou = true;
        Time.timeScale = 1f;
        Tocar("clique");
    }


    void Reiniciar()
    {
        vidas = vidasIniciais;
        Time.timeScale = 1f;
        Tocar("clique");
        SceneManager.LoadScene(0);
    }


    // Desenha o menu, as vidas e a tela de Game Over (OnGUI é a forma mais simples de fazer tela)
    void OnGUI()
    {
        float largura = Screen.width;
        float altura = Screen.height;

        // Estilos: tamanho das letras depende do tamanho da tela
        GUIStyle titulo = new GUIStyle(GUI.skin.label);
        titulo.fontSize = (int)(altura / 8);
        titulo.alignment = TextAnchor.MiddleCenter;
        titulo.normal.textColor = Color.white;

        GUIStyle texto = new GUIStyle(GUI.skin.label);
        texto.fontSize = (int)(altura / 28);
        texto.alignment = TextAnchor.MiddleCenter;
        texto.normal.textColor = Color.white;

        GUIStyle botao = new GUIStyle(GUI.skin.button);
        botao.fontSize = (int)(altura / 20);

        if (!jaComecou)
        {
            EscurecerTela(largura, altura);

            GUI.Label(new Rect(0, altura * 0.15f, largura, altura * 0.2f), "Jogo 2D", titulo);
            GUI.Label(new Rect(0, altura * 0.38f, largura, altura * 0.1f),
                "A / D = andar     Espaço = pular (inclusive na parede)     F = atirar", texto);

            if (GUI.Button(new Rect(largura * 0.35f, altura * 0.55f, largura * 0.3f, altura * 0.12f), "Jogar", botao))
            {
                Comecar();
            }
        }
        else if (gameOver)
        {
            EscurecerTela(largura, altura);

            GUI.Label(new Rect(0, altura * 0.2f, largura, altura * 0.2f), "Game Over", titulo);

            if (GUI.Button(new Rect(largura * 0.35f, altura * 0.5f, largura * 0.3f, altura * 0.12f), "Jogar de novo", botao))
            {
                Reiniciar();
            }
        }
        else
        {
            // Durante o jogo: mostra as vidas no canto
            GUIStyle hud = new GUIStyle(GUI.skin.label);
            hud.fontSize = (int)(altura / 20);
            hud.normal.textColor = Color.white;

            GUI.Label(new Rect(20, 10, largura * 0.4f, altura * 0.1f), "Vidas: " + vidas, hud);
        }
    }


    // Pinta um fundo preto transparente por cima do jogo
    void EscurecerTela(float largura, float altura)
    {
        GUI.color = new Color(0, 0, 0, 0.75f);
        GUI.DrawTexture(new Rect(0, 0, largura, altura), Texture2D.whiteTexture);
        GUI.color = Color.white;
    }
}
