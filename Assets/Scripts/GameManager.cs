using UnityEngine;
using UnityEngine.SceneManagement;

// Controla o jogo: menu, história, vidas, Game Over, fim de fase, música e sons.
// Fica em um objeto vazio chamado GameManager em cada fase.
public class GameManager : MonoBehaviour
{
    public int vidasIniciais = 3;        // quantas vidas o player começa
    public float tempoInvencivel = 1.5f; // tempo sem levar dano depois de perder uma vida
    public float limiteQueda = -15f;     // se o player cair abaixo dessa altura, perde uma vida

    // Em que "tela" o jogo está
    enum Estado { Menu, Intro, Jogando, GameOver, FimDeFase }
    Estado estado;

    // "static" faz o valor continuar mesmo quando a cena é recarregada
    static int vidas = -1;
    static bool jaComecou = false;      // false = ainda não passou do menu inicial

    static GameManager instancia;       // para outros scripts chamarem o GameManager

    // Textos da fase (o LevelBuilder preenche lendo o arquivo do mapa)
    string tituloFase = "";
    string historiaFase = "";
    string textoFim = "";

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

        // Primeira vez: menu. Depois: direto na história da fase.
        // Nas duas telas o jogo fica parado (timeScale 0).
        if (jaComecou)
        {
            estado = Estado.Intro;
        }
        else
        {
            estado = Estado.Menu;
        }

        Time.timeScale = 0f;
    }


    void Start()
    {
        // Procura o player e guarda onde ele começou (para voltar quando perder vida)
        // (o LevelBuilder já colocou o player no lugar no Awake)
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


    // O LevelBuilder chama isto para dizer os textos da fase
    public void DefinirFase(string titulo, string historia, string fim)
    {
        tituloFase = titulo;
        historiaFase = historia;
        textoFim = fim;
    }


    void Update()
    {
        // Enter avança as telas
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (estado == Estado.Menu)
            {
                IrParaHistoria();
            }
            else if (estado == Estado.Intro)
            {
                ComecarFase();
            }
            else if (estado == Estado.GameOver)
            {
                Reiniciar();
            }
            else if (estado == Estado.FimDeFase)
            {
                ProximaFase();
            }
        }

        if (estado != Estado.Jogando)
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
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            return;
        }

        instancia.TirarVida();
    }


    // O portão chama este método quando o player chega nele
    public static void FimDaFase()
    {
        if (instancia != null)
        {
            instancia.TerminarFase();
        }
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
        // Só leva dano jogando e fora da invencibilidade
        if (estado != Estado.Jogando || invencivelTimer > 0)
        {
            return;
        }

        vidas--;
        Tocar("dano");

        if (vidas <= 0)
        {
            estado = Estado.GameOver;
            Time.timeScale = 0f;
            jogadorSprite.enabled = true;
            Tocar("gameover");
            return;
        }

        VoltarAoInicio();
        invencivelTimer = tempoInvencivel;
    }


    // Coloca o player de volta no ponto inicial da fase
    void VoltarAoInicio()
    {
        jogador.position = posicaoInicial;
        jogadorRb.linearVelocity = Vector2.zero;
    }


    void IrParaHistoria()
    {
        jaComecou = true;
        estado = Estado.Intro;
        Tocar("clique");
    }


    void ComecarFase()
    {
        estado = Estado.Jogando;
        Time.timeScale = 1f;
        Tocar("clique");
    }


    void TerminarFase()
    {
        if (estado != Estado.Jogando)
        {
            return;
        }

        estado = Estado.FimDeFase;
        Time.timeScale = 0f;
        jogadorSprite.enabled = true;
        Tocar("fase_completa");
    }


    // Existe outra fase depois desta? (precisa estar na lista de cenas do Build Settings)
    bool TemProximaFase()
    {
        return SceneManager.GetActiveScene().buildIndex + 1 < SceneManager.sceneCountInBuildSettings;
    }


    void ProximaFase()
    {
        Tocar("clique");

        if (TemProximaFase())
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
        else
        {
            // Acabaram as fases: volta ao começo
            vidas = vidasIniciais;
            SceneManager.LoadScene(0);
        }
    }


    void Reiniciar()
    {
        vidas = vidasIniciais;
        Tocar("clique");
        SceneManager.LoadScene(0);
    }


    // Desenha os menus, as vidas e as telas (OnGUI é a forma mais simples de fazer tela)
    void OnGUI()
    {
        float largura = Screen.width;
        float altura = Screen.height;

        // Estilos: tamanho das letras depende do tamanho da tela
        GUIStyle titulo = new GUIStyle(GUI.skin.label);
        titulo.fontSize = (int)(altura / 10);
        titulo.alignment = TextAnchor.MiddleCenter;
        titulo.wordWrap = true;
        titulo.normal.textColor = Color.white;

        GUIStyle texto = new GUIStyle(GUI.skin.label);
        texto.fontSize = (int)(altura / 28);
        texto.alignment = TextAnchor.UpperCenter;
        texto.wordWrap = true;
        texto.normal.textColor = new Color(0.9f, 0.85f, 0.8f);

        GUIStyle botao = new GUIStyle(GUI.skin.button);
        botao.fontSize = (int)(altura / 20);

        Rect areaTitulo = new Rect(largura * 0.1f, altura * 0.12f, largura * 0.8f, altura * 0.2f);
        Rect areaTexto = new Rect(largura * 0.15f, altura * 0.36f, largura * 0.7f, altura * 0.3f);
        Rect areaBotao = new Rect(largura * 0.35f, altura * 0.72f, largura * 0.3f, altura * 0.12f);

        if (estado == Estado.Menu)
        {
            EscurecerTela(largura, altura);

            GUI.Label(areaTitulo, "Jogo 2D", titulo);
            GUI.Label(areaTexto,
                "A / D = andar\nEspaço = pular (também na parede)\nF = atirar\nEnter = continuar", texto);

            if (GUI.Button(areaBotao, "Jogar", botao))
            {
                IrParaHistoria();
            }
        }
        else if (estado == Estado.Intro)
        {
            EscurecerTela(largura, altura);

            GUI.Label(areaTitulo, tituloFase, titulo);
            GUI.Label(areaTexto, historiaFase, texto);

            if (GUI.Button(areaBotao, "Começar", botao))
            {
                ComecarFase();
            }
        }
        else if (estado == Estado.GameOver)
        {
            EscurecerTela(largura, altura);

            GUI.Label(areaTitulo, "Game Over", titulo);

            if (GUI.Button(areaBotao, "Jogar de novo", botao))
            {
                Reiniciar();
            }
        }
        else if (estado == Estado.FimDeFase)
        {
            EscurecerTela(largura, altura);

            GUI.Label(areaTitulo, "Fase completa!", titulo);
            GUI.Label(areaTexto, textoFim, texto);

            string nomeBotao = TemProximaFase() ? "Próxima fase" : "Voltar ao início";

            if (GUI.Button(areaBotao, nomeBotao, botao))
            {
                ProximaFase();
            }
        }
        else
        {
            // Durante o jogo: vidas no canto de cima e nome da fase
            GUIStyle hud = new GUIStyle(GUI.skin.label);
            hud.fontSize = (int)(altura / 22);
            hud.normal.textColor = Color.white;

            GUI.Label(new Rect(20, 10, largura * 0.4f, altura * 0.1f), "Vidas: " + vidas, hud);

            hud.alignment = TextAnchor.UpperRight;
            GUI.Label(new Rect(largura * 0.5f - 20, 10, largura * 0.5f, altura * 0.1f), tituloFase, hud);
        }
    }


    // Pinta um fundo preto transparente por cima do jogo
    void EscurecerTela(float largura, float altura)
    {
        GUI.color = new Color(0, 0, 0, 0.8f);
        GUI.DrawTexture(new Rect(0, 0, largura, altura), Texture2D.whiteTexture);
        GUI.color = Color.white;
    }
}
