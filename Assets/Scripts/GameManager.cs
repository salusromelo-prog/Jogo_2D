using UnityEngine;
using UnityEngine.SceneManagement;

// Controla o jogo: menu, história, vidas, Game Over, fim de fase, música e sons.
// Fica em um objeto vazio chamado GameManager em cada fase.
public class GameManager : MonoBehaviour
{
    public int vidasIniciais = 3;        // quantas vidas o player começa
    public float tempoInvencivel = 1.5f; // tempo sem levar dano depois de perder uma vida
    public float limiteQueda = -15f;     // se o player cair abaixo dessa altura, perde uma vida

    // Volumes (de 0 a 1). Abaixe aqui se o som estiver alto.
    public float volumeEfeitos = 0.3f;   // pulo, tiro, dano...
    public float volumeMusica = 0.12f;   // música de fundo
    static float volumeDosEfeitos = 0.3f;

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
    string dicaFase = "";               // frase curta com o objetivo da fase

    float invencivelTimer = 0f;

    Transform jogador;
    Rigidbody2D jogadorRb;
    SpriteRenderer jogadorSprite;
    Vector3 posicaoInicial;

    // Informações da fase para o HUD (o LevelBuilder e os outros scripts avisam)
    int totalInimigos = 0;
    int inimigosDerrotados = 0;
    int totalAltares = 0;
    int altaresAcesos = 0;
    Vector3 posicaoPortao;
    bool temPortao = false;
    float distanciaInicial = 1f;        // distância do início da fase até o portão
    float tempoJogando = 0f;            // tempo jogado nesta fase (para a dica dos controles)

    // Ícones do HUD (Assets/Resources/Arte)
    Texture2D iconeCoracao;
    Texture2D iconeCoracaoVazio;
    Texture2D iconeAltar;
    Texture2D iconeInimigo;
    Texture2D iconeSeta;


    void Awake()
    {
        instancia = this;
        volumeDosEfeitos = volumeEfeitos;

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

        // Distância do começo até o portão (para a barra de progresso)
        if (temPortao)
        {
            distanciaInicial = Mathf.Max(1f, Vector2.Distance(posicaoInicial, posicaoPortao));
        }

        // Ícones do HUD
        iconeCoracao = Resources.Load<Texture2D>("Arte/ui_coracao");
        iconeCoracaoVazio = Resources.Load<Texture2D>("Arte/ui_coracao_vazio");
        iconeAltar = Resources.Load<Texture2D>("Arte/ui_altar");
        iconeInimigo = Resources.Load<Texture2D>("Arte/ui_inimigo");
        iconeSeta = Resources.Load<Texture2D>("Arte/seta_direita");

        // Música de fundo (arquivo em Assets/Resources/Sons/musica)
        AudioClip clipMusica = Resources.Load<AudioClip>("Sons/musica");
        if (clipMusica != null)
        {
            AudioSource musica = gameObject.AddComponent<AudioSource>();
            musica.clip = clipMusica;
            musica.loop = true;
            musica.volume = volumeMusica;
            musica.Play();
        }
    }


    // O LevelBuilder chama isto para dizer os textos da fase
    public void DefinirFase(string titulo, string historia, string fim, string dica)
    {
        tituloFase = titulo;
        historiaFase = historia;
        textoFim = fim;
        dicaFase = dica;
    }


    // O LevelBuilder chama estes métodos para o HUD saber o que existe na fase
    public void RegistrarInimigo()
    {
        totalInimigos++;
    }

    public void RegistrarAltar()
    {
        totalAltares++;
    }

    public void RegistrarPortao(Vector3 posicao)
    {
        posicaoPortao = posicao;
        temPortao = true;
    }

    // A shuriken chama quando acerta um inimigo
    public static void InimigoDerrotado()
    {
        if (instancia != null)
        {
            instancia.inimigosDerrotados++;
        }
    }

    // O altar chama quando acende
    public static void AltarAceso()
    {
        if (instancia != null)
        {
            instancia.altaresAcesos++;
        }
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

        tempoJogando += Time.deltaTime;

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


    // O altar (Checkpoint) chama este método: o player passa a voltar para este ponto
    public static void DefinirCheckpoint(Vector3 posicao)
    {
        if (instancia != null)
        {
            instancia.posicaoInicial = posicao;
        }
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
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, volumeDosEfeitos);
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

            // Objetivo da fase em destaque, logo abaixo do título
            GUIStyle destaque = new GUIStyle(texto);
            destaque.fontStyle = FontStyle.Bold;
            destaque.normal.textColor = new Color(0.65f, 1f, 0.85f);
            GUI.Label(new Rect(largura * 0.1f, altura * 0.30f, largura * 0.8f, altura * 0.07f), dicaFase, destaque);

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
            // Durante o jogo: HUD com vidas, altares, inimigos, objetivo e seta para o portão
            DesenharHud(largura, altura);
            DesenharSetaDoPortao(largura, altura);
            DesenharDicaDosControles(largura, altura);
        }
    }


    // Retângulo escuro e transparente com uma borda fina, usado de fundo no HUD
    void Painel(Rect area)
    {
        GUI.color = new Color(0.04f, 0.08f, 0.12f, 0.62f);
        GUI.DrawTexture(area, Texture2D.whiteTexture);

        GUI.color = new Color(0.55f, 0.8f, 0.85f, 0.45f);
        float borda = Mathf.Max(1f, area.height * 0.025f);
        GUI.DrawTexture(new Rect(area.x, area.y, area.width, borda), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(area.x, area.yMax - borda, area.width, borda), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(area.x, area.y, borda, area.height), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(area.xMax - borda, area.y, borda, area.height), Texture2D.whiteTexture);

        GUI.color = Color.white;
    }


    // HUD: painel da esquerda (vidas, altares, inimigos) e da direita (fase, objetivo, distância)
    void DesenharHud(float largura, float altura)
    {
        float u = altura / 400f;        // tamanho de "1 pixel de arte" na tela (escala com a tela)
        float icone = 16f * u;          // os ícones têm 16x16
        float margem = 6f * u;
        float espaco = 3f * u;

        GUIStyle texto = new GUIStyle(GUI.skin.label);
        texto.fontSize = (int)(8f * u);
        texto.fontStyle = FontStyle.Bold;
        texto.alignment = TextAnchor.MiddleLeft;
        texto.normal.textColor = Color.white;

        // ---------- Painel da esquerda ----------
        float larguraCoracoes = vidasIniciais * (icone + espaco) - espaco;
        float larguraInfo = 0f;
        if (totalAltares > 0)
        {
            larguraInfo += icone + espaco + 22f * u;
        }
        if (totalInimigos > 0)
        {
            larguraInfo += icone + espaco + 22f * u;
        }

        float larguraEsq = Mathf.Max(larguraCoracoes, larguraInfo) + margem * 2f;
        float alturaEsq = margem * 2f + icone + (larguraInfo > 0 ? espaco * 2f + icone : 0f);
        Rect painelEsq = new Rect(margem, margem, larguraEsq, alturaEsq);
        Painel(painelEsq);

        // Corações (cheios = vidas que restam, vazios = vidas perdidas)
        for (int i = 0; i < vidasIniciais; i++)
        {
            Texture2D coracao = (i < vidas) ? iconeCoracao : iconeCoracaoVazio;
            GUI.DrawTexture(new Rect(painelEsq.x + margem + i * (icone + espaco), painelEsq.y + margem, icone, icone), coracao);
        }

        // Altares acesos e inimigos derrotados
        float cursorX = painelEsq.x + margem;
        float linhaY = painelEsq.y + margem + icone + espaco * 2f;

        if (totalAltares > 0)
        {
            GUI.DrawTexture(new Rect(cursorX, linhaY, icone, icone), iconeAltar);
            GUI.Label(new Rect(cursorX + icone + espaco, linhaY, 22f * u, icone), altaresAcesos + "/" + totalAltares, texto);
            cursorX += icone + espaco + 22f * u;
        }

        if (totalInimigos > 0)
        {
            GUI.DrawTexture(new Rect(cursorX, linhaY, icone, icone), iconeInimigo);
            GUI.Label(new Rect(cursorX + icone + espaco, linhaY, 22f * u, icone), inimigosDerrotados + "/" + totalInimigos, texto);
        }

        // ---------- Painel da direita ----------
        float larguraDir = Mathf.Min(largura * 0.42f, 190f * u);
        float alturaDir = margem * 2f + 11f * u + 17f * u + 6f * u + 4f * u + 9f * u;
        Rect painelDir = new Rect(largura - larguraDir - margem, margem, larguraDir, alturaDir);
        Painel(painelDir);

        float x = painelDir.x + margem;
        float y = painelDir.y + margem;
        float larguraUtil = larguraDir - margem * 2f;

        // Nome da fase
        GUIStyle titulo = new GUIStyle(texto);
        titulo.alignment = TextAnchor.UpperLeft;
        GUI.Label(new Rect(x, y, larguraUtil, 11f * u), tituloFase, titulo);
        y += 11f * u;

        // Objetivo
        GUIStyle objetivo = new GUIStyle(GUI.skin.label);
        objetivo.fontSize = (int)(6.5f * u);
        objetivo.wordWrap = true;
        objetivo.alignment = TextAnchor.UpperLeft;
        objetivo.normal.textColor = new Color(0.65f, 1f, 0.85f);
        GUI.Label(new Rect(x, y, larguraUtil, 17f * u), dicaFase, objetivo);
        y += 17f * u;

        // Barra de progresso até o portão
        if (temPortao)
        {
            float distancia = Vector2.Distance(jogador.position, posicaoPortao);
            float progresso = Mathf.Clamp01(1f - distancia / distanciaInicial);

            GUI.color = new Color(0.1f, 0.18f, 0.22f, 0.9f);
            GUI.DrawTexture(new Rect(x, y, larguraUtil, 5f * u), Texture2D.whiteTexture);
            GUI.color = new Color(0.55f, 1f, 0.8f, 1f);
            GUI.DrawTexture(new Rect(x, y, larguraUtil * progresso, 5f * u), Texture2D.whiteTexture);
            GUI.color = Color.white;
            y += 5f * u + 1f * u;

            GUIStyle distanciaTexto = new GUIStyle(objetivo);
            distanciaTexto.fontSize = (int)(6.5f * u);
            GUI.Label(new Rect(x, y, larguraUtil, 9f * u), "Portão: " + Mathf.RoundToInt(distancia) + " m", distanciaTexto);
        }
    }


    // Seta na borda da tela apontando para o portão quando ele não está à vista
    void DesenharSetaDoPortao(float largura, float altura)
    {
        if (!temPortao || Camera.main == null || iconeSeta == null)
        {
            return;
        }

        float u = altura / 400f;

        // Onde o portão está na tela (a tela do GUI tem o y de cima para baixo)
        Vector3 naTela = Camera.main.WorldToScreenPoint(posicaoPortao);
        Vector2 alvo = new Vector2(naTela.x, altura - naTela.y);

        bool aVista = naTela.z > 0 && alvo.x > 0 && alvo.x < largura && alvo.y > 0 && alvo.y < altura;
        if (aVista)
        {
            return;
        }

        // Direção do centro da tela até o portão, e o ponto onde ela encontra a borda
        Vector2 centro = new Vector2(largura / 2f, altura / 2f);
        Vector2 direcao = (alvo - centro).normalized;

        float folga = 26f * u;
        float tx = (Mathf.Abs(direcao.x) > 0.0001f) ? (largura / 2f - folga) / Mathf.Abs(direcao.x) : float.MaxValue;
        float ty = (Mathf.Abs(direcao.y) > 0.0001f) ? (altura / 2f - folga) / Mathf.Abs(direcao.y) : float.MaxValue;
        Vector2 posicao = centro + direcao * Mathf.Min(tx, ty);

        // Seta (a imagem aponta para a direita, então gira pelo ângulo da direção)
        float tamanho = 26f * u;
        float brilho = 0.75f + 0.25f * Mathf.Sin(Time.unscaledTime * 4f);
        Matrix4x4 guardada = GUI.matrix;

        GUI.color = new Color(1f, 1f, 1f, brilho);
        GUIUtility.RotateAroundPivot(Mathf.Atan2(direcao.y, direcao.x) * Mathf.Rad2Deg, posicao);
        GUI.DrawTexture(new Rect(posicao.x - tamanho / 2f, posicao.y - tamanho / 2f, tamanho, tamanho), iconeSeta);

        GUI.matrix = guardada;
        GUI.color = Color.white;

        // Texto "Portão" e a distância, um pouco para dentro da tela
        GUIStyle texto = new GUIStyle(GUI.skin.label);
        texto.fontSize = (int)(7f * u);
        texto.fontStyle = FontStyle.Bold;
        texto.alignment = TextAnchor.MiddleCenter;
        texto.normal.textColor = new Color(0.65f, 1f, 0.85f);

        float distancia = Vector2.Distance(jogador.position, posicaoPortao);
        Vector2 posicaoTexto = posicao - direcao * (tamanho * 1.5f);
        GUI.Label(new Rect(posicaoTexto.x - 40f * u, posicaoTexto.y - 8f * u, 80f * u, 16f * u),
            "Portão " + Mathf.RoundToInt(distancia) + " m", texto);
    }


    // Nos primeiros segundos de cada fase, lembra os controles embaixo da tela
    void DesenharDicaDosControles(float largura, float altura)
    {
        float duracao = 9f;
        if (tempoJogando > duracao)
        {
            return;
        }

        float transparencia = Mathf.Clamp01((duracao - tempoJogando) / 2f);   // some devagar no fim
        float u = altura / 400f;

        Rect area = new Rect(largura / 2f - 150f * u, altura - 26f * u, 300f * u, 18f * u);
        GUI.color = new Color(1f, 1f, 1f, transparencia);
        Painel(area);

        GUIStyle texto = new GUIStyle(GUI.skin.label);
        texto.fontSize = (int)(7f * u);
        texto.alignment = TextAnchor.MiddleCenter;
        texto.normal.textColor = new Color(1f, 1f, 1f, transparencia);
        GUI.Label(area, "A / D andar     Espaço pular (e na parede)     F atirar", texto);
        GUI.color = Color.white;
    }


    // Pinta um fundo preto transparente por cima do jogo
    void EscurecerTela(float largura, float altura)
    {
        GUI.color = new Color(0, 0, 0, 0.8f);
        GUI.DrawTexture(new Rect(0, 0, largura, altura), Texture2D.whiteTexture);
        GUI.color = Color.white;
    }
}
