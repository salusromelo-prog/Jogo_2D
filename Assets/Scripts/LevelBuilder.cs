using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// Monta a fase a partir de um arquivo de texto (em Assets/Resources/Fases/).
// Cada letra do mapa vira uma parte da fase:
//   #  chão (pedra ou terra)    T  plataforma (telhado ou balsa de bambu)   V  pé de bambu (para pular na parede)
//   Q  plataforma que quebra    ^  espinhos                                 C  altar (checkpoint)
//   P  onde o player começa     E  inimigo                                  G  portão (fim da fase)
//   L  lanterna (enfeite)       .  vazio
// Linhas que começam com @ não fazem parte do mapa:
//   @titulo, @historia e @fim são os textos da fase; @tema escolhe o visual (vila ou floresta).
public class LevelBuilder : MonoBehaviour
{
    public string arquivoMapa = "Fases/fase1";  // nome do arquivo de texto (sem .txt)
    public GameObject inimigoModelo;            // inimigo da cena (fica desligado, só serve de modelo)

    private string[] linhas;    // as linhas do mapa (a primeira é a de cima)
    private int largura;        // quantidade de colunas
    private int altura;         // quantidade de linhas

    private string tema = "vila";   // "vila" ou "floresta"
    private string prefixo = "";    // as imagens de um tema começam com este nome (ex.: floresta_tile_topo)

    private Transform raiz;     // objeto que guarda tudo que for criado
    private GameManager gm;     // para avisar o GameManager do que existe na fase

    // Câmera da fase (o arquivo do mapa pode mudar com a linha @camera)
    private float alturaExtraCamera = 2f;           // quanto a câmera fica acima do player (negativo = abaixo)
    private float alturaExtraQuedaCamera = -1.5f;   // para onde a câmera olha quando o player cai rápido


    void Awake()
    {
        TextAsset arquivo = Resources.Load<TextAsset>(arquivoMapa);

        if (arquivo == null)
        {
            Debug.LogError("Mapa não encontrado: Assets/Resources/" + arquivoMapa + ".txt");
            return;
        }

        raiz = new GameObject("Fase").transform;

        LerMapa(arquivo.text);
        CriarFundo();
        CriarChao();
        CriarObjetos();
        ConfigurarCamera();
        CriarEfeitos();
    }


    // Separa os textos (@) do desenho do mapa
    void LerMapa(string texto)
    {
        string titulo = "";
        string historia = "";
        string fim = "";
        string dica = "";
        List<string> mapa = new List<string>();

        foreach (string linhaBruta in texto.Split('\n'))
        {
            string linha = linhaBruta.TrimEnd('\r');

            if (linha.StartsWith("@titulo "))
            {
                titulo = linha.Substring(8);
            }
            else if (linha.StartsWith("@historia "))
            {
                historia = linha.Substring(10).Replace("\\n", "\n");
            }
            else if (linha.StartsWith("@fim "))
            {
                fim = linha.Substring(5).Replace("\\n", "\n");
            }
            else if (linha.StartsWith("@dica "))
            {
                dica = linha.Substring(6);
            }
            else if (linha.StartsWith("@camera "))
            {
                // @camera tamanho alturaExtra alturaExtraQueda   (ex.: @camera 7 -2 -4.5)
                // tamanho maior = câmera mais afastada (mostra mais da fase)
                string[] partes = linha.Substring(8).Trim().Split(' ');
                System.Globalization.CultureInfo cultura = System.Globalization.CultureInfo.InvariantCulture;

                Camera.main.orthographicSize = float.Parse(partes[0], cultura);

                if (partes.Length > 1)
                {
                    alturaExtraCamera = float.Parse(partes[1], cultura);
                }

                if (partes.Length > 2)
                {
                    alturaExtraQuedaCamera = float.Parse(partes[2], cultura);
                }
            }
            else if (linha.StartsWith("@tema "))
            {
                tema = linha.Substring(6).Trim();
            }
            else if (linha.Length > 0)
            {
                mapa.Add(linha);
            }
        }

        // As imagens de cada tema começam com o nome dele (ex.: "desfiladeiro_"); a vila usa os nomes normais
        prefixo = (tema != "vila") ? tema + "_" : "";

        linhas = mapa.ToArray();
        altura = linhas.Length;

        foreach (string linha in linhas)
        {
            largura = Mathf.Max(largura, linha.Length);
        }

        // Passa os textos para o GameManager mostrar nas telas
        gm = FindFirstObjectByType<GameManager>();
        if (gm != null)
        {
            gm.DefinirFase(titulo, historia, fim, dica);
            gm.limiteQueda = -8f;   // o mapa começa na altura 0, então cair abaixo disso é cair no buraco
        }
    }


    // Devolve a letra do mapa na posição x, y (y = 0 é a linha de baixo)
    char Letra(int x, int y)
    {
        int linha = altura - 1 - y;

        if (x < 0 || y < 0 || linha < 0 || linha >= altura)
        {
            return '.';
        }

        if (x >= linhas[linha].Length)
        {
            return '.';
        }

        return linhas[linha][x];
    }


    // Carrega uma imagem de Assets/Resources/Arte. Primeiro procura a versão do tema, depois a normal.
    Sprite Carregar(string nome)
    {
        Sprite sprite = null;

        if (prefixo != "")
        {
            sprite = Resources.Load<Sprite>("Arte/" + prefixo + nome);
        }

        if (sprite == null)
        {
            sprite = Resources.Load<Sprite>("Arte/" + nome);
        }

        if (sprite == null)
        {
            Debug.LogError("Imagem não encontrada: Assets/Resources/Arte/" + nome);
        }

        return sprite;
    }


    // Céu e camadas de fundo que se mexem mais devagar que a câmera
    void CriarFundo()
    {
        Camera cam = Camera.main;
        Sprite spriteCeu = Carregar("bg_ceu");

        // Céu: um sprite grande que acompanha a câmera
        GameObject ceu = new GameObject("Ceu");
        ceu.transform.parent = raiz;
        SpriteRenderer srCeu = ceu.AddComponent<SpriteRenderer>();
        srCeu.sprite = spriteCeu;
        srCeu.sortingLayerName = "Background";
        srCeu.sortingOrder = -100;

        // Escala para cobrir a tela (mesmo em telas bem largas)
        float alturaTela = cam.orthographicSize * 2f;
        float larguraTela = alturaTela * 2.4f;
        Vector2 tamanho = spriteCeu.bounds.size;
        float escala = Mathf.Max(alturaTela / tamanho.y, larguraTela / tamanho.x);
        ceu.transform.localScale = new Vector3(escala, escala, 1f);

        Parallax paralaxeCeu = ceu.AddComponent<Parallax>();
        paralaxeCeu.fator = 1f;
        paralaxeCeu.deslocamentoY = 0f;

        if (tema == "desfiladeiro")
        {
            // Paredões de pedra que se repetem para os lados e para cima/baixo.
            // Quanto mais longe, mais devagar andam (também na subida e na descida).
            float alturaFundo = 200f;
            CriarCamada("PilaresLonge", "bg_pilares_longe", -90, 0.92f, 0f, true, 0f, "Background", 0.8f, alturaFundo);
            CriarCamada("PilaresMeio", "bg_pilares_meio", -85, 0.8f, 0f, true, 0f, "Background", 0.65f, alturaFundo);
            CriarCamada("PilaresPerto", "bg_pilares_perto", -80, 0.65f, 0f, true, 0f, "Background", 0.5f, alturaFundo);
        }
        else if (tema == "fortaleza")
        {
            // Torres ao longe (afundam quando você sobe) e colunas de pedra com estandartes
            float alturaFundo = 200f;
            CriarCamada("Torres", "bg_torres", -90, 0.9f, -2.5f, true, 0f, "Background", 0.85f);
            CriarCamada("ColunasLonge", "bg_colunas_longe", -85, 0.8f, 0f, true, 0f, "Background", 0.65f, alturaFundo);
            CriarCamada("ColunasPerto", "bg_colunas_perto", -80, 0.65f, 0f, true, 0f, "Background", 0.5f, alturaFundo);
        }
        else if (tema == "floresta")
        {
            CriarCamada("Montanhas", "bg_montanha", -90, 0.92f, -0.5f, true, 0f, "Background");

            // A fortaleza fica bem longe e só entra na tela perto do fim da fase
            float xDoFim = Mathf.Max(0f, largura - 9f);
            CriarCamada("Fortaleza", "fortaleza_longe", -89, 0.7f, -1.2f, false, 3f + 0.3f * xDoFim, "Background");

            CriarCamada("BambuLonge", "bg_bambu_longe", -85, 0.85f, -1.2f, true, 0f, "Background");
            CriarCamada("BambuPerto", "bg_bambu_perto", -80, 0.7f, -1.8f, true, 0f, "Background");
        }
        else
        {
            CriarCamada("Montanhas", "bg_montanha", -90, 0.9f, -1.0f, true, 0f, "Background");
            CriarCamada("Bambu", "bg_bambu", -85, 0.8f, -1.5f, true, 0f, "Background");
            CriarCamada("Vila", "bg_vila", -80, 0.7f, -2.0f, true, 0f, "Background");
        }
    }


    // Uma camada de fundo (repete para os lados se "repetir" for verdadeiro).
    // fatorY e alturaTotal são opcionais: servem para fases que sobem ou descem (a camada também se mexe na vertical).
    void CriarCamada(string nome, string imagem, int ordem, float fator, float deslocamentoY, bool repetir, float deslocamentoX, string camadaDeDesenho, float fatorY = 1f, float alturaTotal = 0f)
    {
        Sprite sprite = Carregar(imagem);

        GameObject camada = new GameObject(nome);
        camada.transform.parent = raiz;

        SpriteRenderer sr = camada.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingLayerName = camadaDeDesenho;
        sr.sortingOrder = ordem;

        if (repetir)
        {
            sr.drawMode = SpriteDrawMode.Tiled;                 // repete a imagem
            float altura = (alturaTotal > 0f) ? alturaTotal : sprite.bounds.size.y;
            sr.size = new Vector2(400f, altura);                // 400 unidades de largura
        }

        Parallax paralaxe = camada.AddComponent<Parallax>();
        paralaxe.fator = fator;
        paralaxe.fatorY = fatorY;
        paralaxe.deslocamentoX = deslocamentoX;
        paralaxe.deslocamentoY = deslocamentoY;
    }


    // Cria o Tilemap com os blocos # T V
    void CriarChao()
    {
        GameObject grade = new GameObject("Grade");
        grade.transform.parent = raiz;
        grade.AddComponent<Grid>();

        GameObject objChao = new GameObject("Chao");
        objChao.transform.parent = grade.transform;
        objChao.tag = "Ground";     // o Player só pisa e escala em coisas com a tag Ground

        Tilemap tilemap = objChao.AddComponent<Tilemap>();

        TilemapRenderer render = objChao.AddComponent<TilemapRenderer>();
        render.sortingLayerName = "Ground";
        render.sortingOrder = -10;

        // Colisão: junta todos os blocos em uma forma só (evita o player "agarrar" nas emendas)
        Rigidbody2D corpo = objChao.AddComponent<Rigidbody2D>();
        corpo.bodyType = RigidbodyType2D.Static;
        CompositeCollider2D composto = objChao.AddComponent<CompositeCollider2D>();
        composto.geometryType = CompositeCollider2D.GeometryType.Polygons;  // blocos sólidos por dentro
        TilemapCollider2D colisor = objChao.AddComponent<TilemapCollider2D>();
        colisor.compositeOperation = Collider2D.CompositeOperation.Merge;

        Tile pedraTopo = CriarTile("tile_topo");
        Tile pedraCorpo = CriarTile("tile_corpo");
        Tile telhado = CriarTile("tile_telhado");
        Tile bambu = null;      // só é criado se o mapa tiver bambu (letra V)

        for (int x = 0; x < largura; x++)
        {
            for (int y = 0; y < altura; y++)
            {
                char letra = Letra(x, y);
                Vector3Int posicao = new Vector3Int(x, y, 0);

                if (letra == '#')
                {
                    // Se não tem chão em cima, usa o bloco com a borda de cima
                    if (Letra(x, y + 1) == '#')
                    {
                        tilemap.SetTile(posicao, pedraCorpo);
                    }
                    else
                    {
                        tilemap.SetTile(posicao, pedraTopo);
                    }
                }
                else if (letra == 'T')
                {
                    tilemap.SetTile(posicao, telhado);
                }
                else if (letra == 'V')
                {
                    if (bambu == null)
                    {
                        bambu = CriarTile("tile_bambu");
                    }

                    tilemap.SetTile(posicao, bambu);
                }
            }
        }
    }


    Tile CriarTile(string imagem)
    {
        Tile tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = Carregar(imagem);
        tile.colliderType = Tile.ColliderType.Grid;     // colisão quadrada certinha
        return tile;
    }


    // Cria espinhos, lanternas, altares, portão, plataformas que quebram, inimigos e coloca o player
    void CriarObjetos()
    {
        for (int x = 0; x < largura; x++)
        {
            for (int y = 0; y < altura; y++)
            {
                char letra = Letra(x, y);

                if (letra == '^')
                {
                    CriarEspinhos(x, y);
                }
                else if (letra == '>')
                {
                    CriarEspinhosParede(x, y, true);     // presos na parede da esquerda, apontando para a direita
                }
                else if (letra == '<')
                {
                    CriarEspinhosParede(x, y, false);    // presos na parede da direita, apontando para a esquerda
                }
                else if (letra == 'r')
                {
                    CriarSeta(x, y, "seta_direita", Vector2.right);
                }
                else if (letra == 'e')
                {
                    CriarSeta(x, y, "seta_esquerda", Vector2.left);
                }
                else if (letra == 'v')
                {
                    CriarSeta(x, y, "seta_baixo", Vector2.down);
                }
                else if (letra == 'u')
                {
                    CriarSeta(x, y, "seta_cima", Vector2.up);
                }
                else if (letra == 'L')
                {
                    GameObject luz = NovoSprite("Lanterna", "lanterna", x + 0.5f, y + 1f, -8);

                    // Na fortaleza as luzes são braseiros: o fogo treme
                    if (tema == "fortaleza")
                    {
                        luz.AddComponent<PiscarLuz>();
                    }
                }
                else if (letra == 'B')
                {
                    CriarChefe(x, y);
                }
                else if (letra == 'A')
                {
                    CriarCela(x, y);
                }
                else if (letra == 'C')
                {
                    CriarAltar(x, y);
                }
                else if (letra == 'Q')
                {
                    CriarPlataformaQuebra(x, y);
                }
                else if (letra == 'G')
                {
                    CriarPortao(x, y);
                }
                else if (letra == 'E')
                {
                    CriarInimigo(x, y);
                }
                else if (letra == 'P')
                {
                    ColocarPlayer(x, y);
                }
            }
        }
    }


    GameObject NovoSprite(string nome, string imagem, float x, float y, int ordem)
    {
        GameObject obj = new GameObject(nome);
        obj.transform.parent = raiz;
        obj.transform.position = new Vector3(x, y, 0f);

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = Carregar(imagem);
        sr.sortingLayerName = "Ground";
        sr.sortingOrder = ordem;

        return obj;
    }


    void CriarEspinhos(int x, int y)
    {
        // A imagem tem meio bloco de altura, então fica encostada no chão
        GameObject espinhos = NovoSprite("Espinhos", "espinhos", x + 0.5f, y + 0.25f, -5);
        espinhos.tag = "Dano";

        BoxCollider2D colisor = espinhos.AddComponent<BoxCollider2D>();
        colisor.size = new Vector2(0.9f, 0.35f);
        colisor.offset = new Vector2(0f, -0.05f);

        espinhos.AddComponent<Espinho>();   // quem encosta perde uma vida
    }


    // Espinhos na parede: é a mesma imagem dos espinhos, só girada. Ocupam metade do bloco de largura.
    void CriarEspinhosParede(int x, int y, bool apontaParaDireita)
    {
        float centroX = apontaParaDireita ? x + 0.25f : x + 0.75f;
        GameObject espinhos = NovoSprite("EspinhosParede", "espinhos", centroX, y + 0.5f, -5);
        espinhos.tag = "Dano";

        // Gira 90 graus: a ponta que apontava para cima passa a apontar para o lado
        float giro = apontaParaDireita ? -90f : 90f;
        espinhos.transform.rotation = Quaternion.Euler(0f, 0f, giro);

        BoxCollider2D colisor = espinhos.AddComponent<BoxCollider2D>();
        colisor.size = new Vector2(0.9f, 0.4f);
        colisor.offset = new Vector2(0f, 0f);

        espinhos.AddComponent<Espinho>();
    }


    // Seta brilhante que mostra para onde ir (r = direita, e = esquerda, v = para baixo, u = para cima)
    void CriarSeta(int x, int y, string imagem, Vector2 direcao)
    {
        GameObject seta = NovoSprite("Seta", imagem, x + 0.5f, y + 0.5f, -3);
        Seta script = seta.AddComponent<Seta>();
        script.direcao = direcao;
    }


    void CriarAltar(int x, int y)
    {
        // O altar tem 1,5 de largura e 2 de altura
        GameObject altar = NovoSprite("Altar", "altar_apagado", x + 0.5f, y + 1f, -4);

        BoxCollider2D colisor = altar.AddComponent<BoxCollider2D>();
        colisor.isTrigger = true;               // trigger: o player passa por dentro
        colisor.size = new Vector2(1.6f, 2f);

        Checkpoint checkpoint = altar.AddComponent<Checkpoint>();
        checkpoint.spriteAceso = Carregar("altar_aceso");

        if (gm != null)
        {
            gm.RegistrarAltar();    // conta quantos altares tem na fase
        }
    }


    void CriarPlataformaQuebra(int x, int y)
    {
        GameObject plataforma = NovoSprite("PlataformaQuebra", "tile_quebra", x + 0.5f, y + 0.5f, -10);
        plataforma.tag = "Ground";      // o Player pisa nela como no chão

        BoxCollider2D colisor = plataforma.AddComponent<BoxCollider2D>();
        colisor.size = new Vector2(1f, 1f);

        plataforma.AddComponent<PlataformaQuebra>();
    }


    void CriarPortao(int x, int y)
    {
        // O portão tem 2 blocos de largura e 3 de altura
        GameObject portao = NovoSprite("Portao", "portao", x + 0.5f, y + 1.5f, -2);

        BoxCollider2D colisor = portao.AddComponent<BoxCollider2D>();
        colisor.isTrigger = true;               // trigger: o player passa por dentro
        colisor.size = new Vector2(1.2f, 2.6f);

        portao.AddComponent<Portao>();

        if (gm != null)
        {
            gm.RegistrarPortao(portao.transform.position);  // o GameManager usa para mostrar a seta e a distância
        }

        // Nas fases altas, uma coluna de luz sobre o portão mostra o destino de longe
        if (tema == "desfiladeiro")
        {
            float alturaDoFeixe = 50f;
            GameObject feixe = NovoSprite("FeixeDeLuz", "feixe_luz", x + 0.5f, y + 3f + alturaDoFeixe / 2f, -70);
            feixe.GetComponent<SpriteRenderer>().sortingLayerName = "Background";

            // A imagem tem 2 x 16 unidades: estica até 5 de largura e a altura desejada
            Vector2 tamanho = feixe.GetComponent<SpriteRenderer>().sprite.bounds.size;
            feixe.transform.localScale = new Vector3(5f / tamanho.x, alturaDoFeixe / tamanho.y, 1f);
        }
    }


    void CriarInimigo(int x, int y)
    {
        GameObject inimigo = Instantiate(inimigoModelo, raiz);
        inimigo.name = "Inimigo";
        inimigo.SetActive(true);    // o modelo fica desligado, a cópia liga

        if (gm != null)
        {
            gm.RegistrarInimigo();  // conta quantos inimigos tem na fase
        }

        ColocarNoChao(inimigo, x + 0.5f, y);
    }


    // O chefe é um inimigo maior, dourado, com mais vida e que atira mais rápido
    void CriarChefe(int x, int y)
    {
        GameObject chefe = Instantiate(inimigoModelo, raiz);
        chefe.name = "Chefe";
        chefe.transform.localScale *= 1.7f;
        chefe.GetComponent<SpriteRenderer>().color = new Color(1f, 0.78f, 0.45f);   // tom dourado

        Enemy script = chefe.GetComponent<Enemy>();
        script.ehChefe = true;
        script.vida = 6;
        script.velocidade = 1.5f;
        script.distancia = 5f;
        script.distanciaTiro = 12f;
        script.tempoEntreTiros = 1.2f;

        chefe.SetActive(true);
        ColocarNoChao(chefe, x + 0.5f, y);

        if (gm != null)
        {
            gm.RegistrarInimigo();
            gm.RegistrarChefe(script);
        }
    }


    // Cela com o amigo preso (3 x 3 blocos). Só abre quando o chefe cai.
    void CriarCela(int x, int y)
    {
        GameObject cela = new GameObject("Cela");
        cela.transform.parent = raiz;
        cela.transform.position = new Vector3(x + 0.5f, y + 1.5f, 0f);

        // Fundo de pedra com o vão escuro
        GameObject fundo = NovoSprite("CelaFundo", "cela_fundo", x + 0.5f, y + 1.5f, -6);
        fundo.transform.parent = cela.transform;

        // O amigo, de pé dentro da cela
        GameObject amigo = NovoSprite("Amigo", "amigo", x + 0.5f, y + 0.8f, -5);
        amigo.transform.localScale = new Vector3(0.62f, 0.62f, 1f);
        amigo.transform.parent = cela.transform;

        // As grades, na frente do amigo
        GameObject grade = NovoSprite("Grade", "cela_grade", x + 0.5f, y + 1.5f, -4);
        grade.transform.parent = cela.transform;

        BoxCollider2D colisor = cela.AddComponent<BoxCollider2D>();
        colisor.isTrigger = true;
        colisor.size = new Vector2(2.6f, 2.8f);

        Cela script = cela.AddComponent<Cela>();
        script.grade = grade;

        if (gm != null)
        {
            gm.RegistrarPortao(cela.transform.position);    // a seta e a distância do HUD apontam para a cela
        }
    }


    void ColocarPlayer(int x, int y)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        ColocarNoChao(player, x + 0.5f, y);
        player.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
    }


    // Coloca o objeto em pé em cima do chão (y = altura do chão)
    void ColocarNoChao(GameObject obj, float x, float y)
    {
        obj.transform.position = new Vector3(x, y + 1f, 0f);   // posição provisória
        Physics2D.SyncTransforms();                              // atualiza a colisão para medir

        Collider2D colisor = obj.GetComponent<Collider2D>();
        float diferenca = y - colisor.bounds.min.y;              // quanto falta para os pés encostarem no chão

        obj.transform.position += new Vector3(0f, diferenca + 0.02f, 0f);
    }


    // A câmera segue o player e não sai do mapa
    void ConfigurarCamera()
    {
        Camera cam = Camera.main;
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        float metadeAltura = cam.orthographicSize;
        float metadeLargura = metadeAltura * cam.aspect;

        CameraFollow seguir = cam.gameObject.AddComponent<CameraFollow>();
        seguir.alvo = player.transform;
        seguir.alturaExtra = alturaExtraCamera;
        seguir.alturaExtraQueda = alturaExtraQuedaCamera;
        seguir.minX = metadeLargura;
        seguir.maxX = Mathf.Max(metadeLargura, largura - metadeLargura);
        seguir.minY = metadeAltura - 1f;
        seguir.maxY = Mathf.Max(metadeAltura, altura - metadeAltura);

        seguir.IrParaOAlvo();   // já começa no lugar certo
    }


    // Efeitos das fases de floresta e desfiladeiro: luzinhas flutuando e uma sombra nas bordas da tela (vinheta)
    void CriarEfeitos()
    {
        if (tema == "vila")
        {
            return;
        }

        GameObject vagalumes = new GameObject("Vagalumes");
        vagalumes.transform.parent = raiz;
        Vagalumes script = vagalumes.AddComponent<Vagalumes>();

        if (tema == "desfiladeiro")
        {
            // Poeirinhas de névoa: grandes e fracas
            script.sprite = Carregar("mota");
            script.quantidade = 32;
            script.tamanhoMin = 1.2f;
            script.tamanhoMax = 3f;
            script.intensidade = 0.35f;
        }
        else if (tema == "fortaleza")
        {
            // Brasas subindo dos braseiros
            script.sprite = Carregar("brasa");
            script.quantidade = 28;
            script.tamanhoMin = 0.5f;
            script.tamanhoMax = 1.2f;
            script.intensidade = 0.9f;
        }
        else
        {
            script.sprite = Carregar("vagalume");
        }

        // Vinheta: imagem escura nas bordas, presa à câmera e por cima de tudo
        Camera cam = Camera.main;
        Sprite spriteVinheta = Carregar("vinheta");

        GameObject vinheta = new GameObject("Vinheta");
        vinheta.transform.parent = cam.transform;
        vinheta.transform.localPosition = new Vector3(0f, 0f, 10f);

        SpriteRenderer sr = vinheta.AddComponent<SpriteRenderer>();
        sr.sprite = spriteVinheta;
        sr.sortingLayerName = "Player";
        sr.sortingOrder = 100;

        float alturaTela = cam.orthographicSize * 2f;
        float larguraTela = alturaTela * 2.4f;
        Vector2 tamanho = spriteVinheta.bounds.size;
        float escala = Mathf.Max(alturaTela / tamanho.y, larguraTela / tamanho.x);
        vinheta.transform.localScale = new Vector3(escala, escala, 1f);
    }
}
