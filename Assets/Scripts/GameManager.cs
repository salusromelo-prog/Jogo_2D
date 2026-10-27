using UnityEngine;
using UnityEngine.SceneManagement;

// Controla o jogo: menu, história, vidas, Game Over, fim de fase, música e sons.
// Fica em um objeto vazio chamado GameManager em cada fase.
public class GameManager : MonoBehaviour
{
    // Textos da tela inicial (mude aqui o nome do jogo)
    public string nomeDoJogo = "Sombra do Ninja";
    public string subtitulo = "A jornada para libertar um amigo";

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

    Enemy chefe;                        // o chefe da fase (se tiver)
    bool temChefe = false;
    bool chefeMorto = false;

    string avisoTexto = "";             // mensagem temporária no meio da tela
    float avisoTimer = 0f;

    // Ícones do HUD (Assets/Resources/Arte)
    Texture2D iconeCoracao;
    Texture2D iconeCoracaoVazio;
    Texture2D iconeAltar;
    Texture2D iconeInimigo;
    Texture2D iconeSeta;

    // Ilustrações das telas
    Texture2D telaInicial;
    Texture2D telaFinal;
    bool mostrandoAjuda = false;        // painel "Como jogar" aberto no menu


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

        // Ilustrações da tela inicial e da tela final
        telaInicial = Resources.Load<Texture2D>("Arte/tela_inicial");
        telaFinal = Resources.Load<Texture2D>("Arte/tela_final");

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

    // O LevelBuilder avisa quando existe um chefe na fase (ele aparece com barra de vida)
    public void RegistrarChefe(Enemy inimigo)
    {
        chefe = inimigo;
        temChefe = true;
    }

    // A cela pergunta se o chefe ainda está vivo
    public static bool ChefeVivo()
    {
        return instancia != null && instancia.temChefe && !instancia.chefeMorto;
    }

    // O chefe chama quando morre
    public static void ChefeDerrotado()
    {
        if (instancia != null)
        {
            instancia.chefeMorto = true;
            MostrarAviso("O capitão caiu! A cela está livre.");
        }
    }

    // Mostra uma mensagem no meio da tela por alguns segundos
    public static void MostrarAviso(string texto)
    {
        if (instancia != null)
        {
            instancia.avisoTexto = texto;
            instancia.avisoTimer = 3.5f;
        }
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
                if (mostrandoAjuda)
                {
                    mostrandoAjuda = false;     // Enter fecha o "Como jogar"
                }
                else
                {
                    IrParaHistoria();
                }
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

        if (avisoTimer > 0)
        {
            avisoTimer -= Time.deltaTime;
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
            // Acabaram as fases: volta ao menu inicial
            vidas = vidasIniciais;
            jaComecou = false;
            SceneManager.LoadScene(0);
        }
    }


    void Reiniciar()
    {
        vidas = vidasIniciais;
        Tocar("clique");
        SceneManager.LoadScene(0);
    }


    // Desenha os menus, as telas e o HUD (OnGUI é a forma mais simples de fazer tela)
    void OnGUI()
    {
        float largura = Screen.width;
        float altura = Screen.height;

        if (estado == Estado.Menu)
        {
            DesenharMenu(largura, altura);
        }
        else if (estado == Estado.Intro)
        {
            DesenharIntro(largura, altura);
        }
        else if (estado == Estado.GameOver)
        {
            DesenharGameOver(largura, altura);
        }
        else if (estado == Estado.FimDeFase)
        {
            DesenharFimDeFase(largura, altura);
        }
        else
        {
            // Durante o jogo: HUD com vidas, altares, inimigos, objetivo e seta para o portão
            DesenharHud(largura, altura);
            DesenharSetaDoPortao(largura, altura);
            DesenharDicaDosControles(largura, altura);
            DesenharChefeEAviso(largura, altura);
        }
    }


    // Barra de vida do chefe (quando o player está perto) e mensagens no meio da tela
    void DesenharChefeEAviso(float largura, float altura)
    {
        float u = altura / 400f;

        // Barra de vida do chefe, embaixo da tela
        if (temChefe && chefe != null && Vector2.Distance(jogador.position, chefe.transform.position) < 18f)
        {
            float larguraBarra = Mathf.Min(largura * 0.5f, 260f * u);
            Rect area = new Rect(largura / 2f - larguraBarra / 2f, altura - 34f * u, larguraBarra, 22f * u);
            Painel(area);

            GUIStyle nome = new GUIStyle(GUI.skin.label);
            nome.fontSize = (int)(6.5f * u);
            nome.fontStyle = FontStyle.Bold;
            nome.alignment = TextAnchor.UpperCenter;
            nome.normal.textColor = new Color(1f, 0.9f, 0.7f);
            GUI.Label(new Rect(area.x, area.y + 1f * u, area.width, 9f * u), "Capitão da Guarda", nome);

            float margem = 4f * u;
            float vidaRelativa = Mathf.Clamp01((float)chefe.vida / Mathf.Max(1, chefe.vidaMaxima));
            Rect barra = new Rect(area.x + margem, area.yMax - 9f * u, area.width - margem * 2f, 5f * u);

            GUI.color = new Color(0.18f, 0.05f, 0.06f, 0.95f);
            GUI.DrawTexture(barra, Texture2D.whiteTexture);
            GUI.color = new Color(0.9f, 0.25f, 0.2f, 1f);
            GUI.DrawTexture(new Rect(barra.x, barra.y, barra.width * vidaRelativa, barra.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        // Mensagem temporária (como "a cela está trancada")
        if (avisoTimer > 0)
        {
            float transparencia = Mathf.Clamp01(avisoTimer / 0.6f);
            Rect aviso = new Rect(largura * 0.2f, altura * 0.28f, largura * 0.6f, 26f * u);

            GUI.color = new Color(1f, 1f, 1f, transparencia);
            Painel(aviso);

            GUIStyle texto = new GUIStyle(GUI.skin.label);
            texto.fontSize = (int)(8f * u);
            texto.fontStyle = FontStyle.Bold;
            texto.alignment = TextAnchor.MiddleCenter;
            texto.normal.textColor = new Color(1f, 0.95f, 0.8f, transparencia);
            GUI.Label(aviso, avisoTexto, texto);
            GUI.color = Color.white;
        }
    }


    // ---------------------------------------------------------------- telas

    // Tela inicial: ilustração, nome do jogo e botões
    void DesenharMenu(float largura, float altura)
    {
        float u = altura / 400f;

        // Ilustração (preenche a tela inteira)
        if (telaInicial != null)
        {
            GUI.DrawTexture(new Rect(0, 0, largura, altura), telaInicial, ScaleMode.ScaleAndCrop);
        }
        else
        {
            EscurecerTela(largura, altura);
        }

        // Nome do jogo e subtítulo
        Rect areaTitulo = new Rect(largura * 0.05f, altura * 0.07f, largura * 0.62f, altura * 0.22f);
        TextoComContorno(areaTitulo, nomeDoJogo.ToUpper(), (int)(34f * u), TextAnchor.MiddleLeft,
            new Color(0.94f, 0.97f, 0.92f), FontStyle.Bold, 2f * u);

        Rect areaSub = new Rect(largura * 0.055f, altura * 0.29f, largura * 0.6f, altura * 0.07f);
        TextoComContorno(areaSub, subtitulo, (int)(10f * u), TextAnchor.MiddleLeft,
            new Color(0.65f, 1f, 0.85f), FontStyle.Italic, 1f * u);

        if (mostrandoAjuda)
        {
            DesenharAjuda(largura, altura);
            return;
        }

        // Botões
        float bx = largura * 0.06f;
        float bw = largura * 0.22f;
        float bh = altura * 0.085f;

        if (Botao(new Rect(bx, altura * 0.50f, bw, bh), "Jogar", u, true))
        {
            IrParaHistoria();
        }

        if (Botao(new Rect(bx, altura * 0.61f, bw, bh), "Como jogar", u, false))
        {
            mostrandoAjuda = true;
        }

        if (Botao(new Rect(bx, altura * 0.72f, bw, bh), "Sair", u, false))
        {
            SairDoJogo();
        }

        // "Pressione ENTER" piscando
        float pisca = 0.55f + 0.45f * Mathf.Sin(Time.unscaledTime * 3f);
        TextoComContorno(new Rect(bx, altura * 0.86f, bw * 1.6f, altura * 0.06f), "Pressione ENTER para jogar",
            (int)(7f * u), TextAnchor.MiddleLeft, new Color(1f, 1f, 1f, pisca), FontStyle.Normal, 1f * u);
    }


    // Painel "Como jogar", aberto pelo menu
    void DesenharAjuda(float largura, float altura)
    {
        float u = altura / 400f;

        Rect painel = new Rect(largura * 0.06f, altura * 0.42f, largura * 0.5f, altura * 0.5f);
        Painel(painel);

        GUIStyle linha = new GUIStyle(GUI.skin.label);
        linha.fontSize = (int)(8f * u);
        linha.wordWrap = true;
        linha.alignment = TextAnchor.UpperLeft;
        linha.normal.textColor = Color.white;

        string texto =
            "A / D  ou  setas      andar\n" +
            "ESPAÇO      pular (junto de uma parede: pulo de parede)\n" +
            "Segurar contra a parede      deslizar devagar\n" +
            "F      atirar shuriken\n\n" +
            "Acenda os altares de jade para guardar seu progresso.\n" +
            "Cuidado com espinhos, tábuas que quebram e guardas.";

        GUI.Label(new Rect(painel.x + 8f * u, painel.y + 6f * u, painel.width - 16f * u, painel.height - 30f * u), texto, linha);

        if (Botao(new Rect(painel.x + 8f * u, painel.yMax - 26f * u, 70f * u, 20f * u), "Voltar", u, false))
        {
            mostrandoAjuda = false;
        }
    }


    // Cartão da história, antes de cada fase
    void DesenharIntro(float largura, float altura)
    {
        float u = altura / 400f;
        EscurecerTela(largura, altura);

        Rect areaTitulo = new Rect(largura * 0.1f, altura * 0.1f, largura * 0.8f, altura * 0.18f);
        TextoComContorno(areaTitulo, tituloFase, (int)(26f * u), TextAnchor.MiddleCenter,
            new Color(0.94f, 0.97f, 0.92f), FontStyle.Bold, 2f * u);

        // Objetivo da fase, em destaque
        Rect areaDica = new Rect(largura * 0.1f, altura * 0.28f, largura * 0.8f, altura * 0.07f);
        TextoComContorno(areaDica, dicaFase, (int)(9f * u), TextAnchor.MiddleCenter,
            new Color(0.65f, 1f, 0.85f), FontStyle.Bold, 1f * u);

        // História
        Rect painelTexto = new Rect(largura * 0.15f, altura * 0.38f, largura * 0.7f, altura * 0.34f);
        Painel(painelTexto);

        GUIStyle texto = new GUIStyle(GUI.skin.label);
        texto.fontSize = (int)(8f * u);
        texto.alignment = TextAnchor.UpperCenter;
        texto.wordWrap = true;
        texto.normal.textColor = new Color(0.92f, 0.9f, 0.86f);
        GUI.Label(new Rect(painelTexto.x + 8f * u, painelTexto.y + 6f * u, painelTexto.width - 16f * u, painelTexto.height - 12f * u), historiaFase, texto);

        if (Botao(new Rect(largura * 0.38f, altura * 0.78f, largura * 0.24f, altura * 0.09f), "Começar", u, true))
        {
            ComecarFase();
        }
    }


    void DesenharGameOver(float largura, float altura)
    {
        float u = altura / 400f;
        EscurecerTela(largura, altura);

        TextoComContorno(new Rect(largura * 0.1f, altura * 0.2f, largura * 0.8f, altura * 0.22f), "Game Over", (int)(40f * u),
            TextAnchor.MiddleCenter, new Color(0.94f, 0.97f, 0.92f), FontStyle.Bold, 2f * u);

        if (Botao(new Rect(largura * 0.37f, altura * 0.55f, largura * 0.26f, altura * 0.09f), "Jogar de novo", u, true))
        {
            Reiniciar();
        }
    }


    // Fim de fase. No fim do jogo, aparece a ilustração final com o ninja e o amigo.
    void DesenharFimDeFase(float largura, float altura)
    {
        float u = altura / 400f;
        bool fimDoJogo = !TemProximaFase();

        if (fimDoJogo && telaFinal != null)
        {
            GUI.DrawTexture(new Rect(0, 0, largura, altura), telaFinal, ScaleMode.ScaleAndCrop);

            // Faixa escura embaixo para o texto ficar legível
            GUI.color = new Color(0.02f, 0.05f, 0.1f, 0.78f);
            GUI.DrawTexture(new Rect(0, altura * 0.62f, largura, altura * 0.38f), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }
        else
        {
            EscurecerTela(largura, altura);
        }

        string titulo = fimDoJogo ? "Fim da jornada" : "Fase completa!";
        TextoComContorno(new Rect(largura * 0.1f, fimDoJogo ? altura * 0.62f : altura * 0.15f, largura * 0.8f, altura * 0.14f),
            titulo, (int)(28f * u), TextAnchor.MiddleCenter, new Color(0.94f, 0.97f, 0.92f), FontStyle.Bold, 2f * u);

        GUIStyle texto = new GUIStyle(GUI.skin.label);
        texto.fontSize = (int)(8f * u);
        texto.alignment = TextAnchor.UpperCenter;
        texto.wordWrap = true;
        texto.normal.textColor = new Color(0.92f, 0.9f, 0.86f);
        float yTexto = fimDoJogo ? altura * 0.75f : altura * 0.32f;
        GUI.Label(new Rect(largura * 0.15f, yTexto, largura * 0.7f, altura * 0.14f), textoFim, texto);

        string nomeBotao = fimDoJogo ? "Voltar ao menu" : "Próxima fase";
        float yBotao = fimDoJogo ? altura * 0.9f : altura * 0.7f;
        if (Botao(new Rect(largura * 0.38f, yBotao, largura * 0.24f, altura * 0.07f), nomeBotao, u, true))
        {
            ProximaFase();
        }
    }


    // ---------------------------------------------------------------- peças de interface

    // Botão bonito: fundo escuro, borda e brilho quando o mouse passa por cima. Devolve true quando clicado.
    bool Botao(Rect area, string texto, float u, bool principal)
    {
        bool sobre = area.Contains(Event.current.mousePosition);

        Color fundo = principal ? new Color(0.1f, 0.4f, 0.36f, 0.92f) : new Color(0.05f, 0.1f, 0.15f, 0.85f);
        if (sobre)
        {
            fundo = Color.Lerp(fundo, new Color(0.45f, 1f, 0.8f, 1f), 0.3f);
        }

        GUI.color = fundo;
        GUI.DrawTexture(area, Texture2D.whiteTexture);

        // Borda
        float borda = Mathf.Max(1f, 1.5f * u);
        GUI.color = sobre ? new Color(0.8f, 1f, 0.92f, 1f) : new Color(0.55f, 0.8f, 0.85f, 0.7f);
        GUI.DrawTexture(new Rect(area.x, area.y, area.width, borda), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(area.x, area.yMax - borda, area.width, borda), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(area.x, area.y, borda, area.height), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(area.xMax - borda, area.y, borda, area.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUIStyle estilo = new GUIStyle(GUI.skin.label);
        estilo.alignment = TextAnchor.MiddleCenter;
        estilo.fontSize = (int)(9f * u);
        estilo.fontStyle = FontStyle.Bold;
        estilo.normal.textColor = Color.white;
        GUI.Label(area, texto, estilo);

        // Botão invisível por cima, só para detectar o clique
        return GUI.Button(area, GUIContent.none, GUIStyle.none);
    }


    // Texto com contorno escuro (fica legível em cima de qualquer imagem)
    void TextoComContorno(Rect area, string texto, int tamanho, TextAnchor alinhamento, Color cor, FontStyle estilo, float espessura)
    {
        GUIStyle s = new GUIStyle(GUI.skin.label);
        s.fontSize = tamanho;
        s.alignment = alinhamento;
        s.fontStyle = estilo;
        s.wordWrap = true;

        s.normal.textColor = new Color(0.01f, 0.05f, 0.07f, cor.a);

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx != 0 || dy != 0)
                {
                    GUI.Label(new Rect(area.x + dx * espessura, area.y + dy * espessura, area.width, area.height), texto, s);
                }
            }
        }

        s.normal.textColor = cor;
        GUI.Label(area, texto, s);
    }


    void SairDoJogo()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
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
