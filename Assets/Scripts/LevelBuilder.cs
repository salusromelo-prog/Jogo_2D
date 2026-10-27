using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// Monta a fase a partir de um arquivo de texto (em Assets/Resources/Fases/).
// Cada letra do mapa vira uma parte da fase:
//   #  bloco de pedra         T  telhado (plataforma)      ^  espinhos
//   P  onde o player começa   E  inimigo                   G  portão (fim da fase)
//   L  lanterna (enfeite)     .  vazio
// Linhas que começam com @ não fazem parte do mapa: @titulo, @historia e @fim são os textos da fase.
public class LevelBuilder : MonoBehaviour
{
    public string arquivoMapa = "Fases/fase1";  // nome do arquivo de texto (sem .txt)
    public GameObject inimigoModelo;            // inimigo da cena (fica desligado, só serve de modelo)

    private string[] linhas;    // as linhas do mapa (a primeira é a de cima)
    private int largura;        // quantidade de colunas
    private int altura;         // quantidade de linhas

    private Transform raiz;     // objeto que guarda tudo que for criado


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
    }


    // Separa os textos (@) do desenho do mapa
    void LerMapa(string texto)
    {
        string titulo = "";
        string historia = "";
        string fim = "";
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
            else if (linha.Length > 0)
            {
                mapa.Add(linha);
            }
        }

        linhas = mapa.ToArray();
        altura = linhas.Length;

        foreach (string linha in linhas)
        {
            largura = Mathf.Max(largura, linha.Length);
        }

        // Passa os textos para o GameManager mostrar nas telas
        GameManager gm = FindFirstObjectByType<GameManager>();
        if (gm != null)
        {
            gm.DefinirFase(titulo, historia, fim);
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


    Sprite Carregar(string nome)
    {
        Sprite sprite = Resources.Load<Sprite>("Arte/" + nome);

        if (sprite == null)
        {
            Debug.LogError("Imagem não encontrada: Assets/Resources/Arte/" + nome);
        }

        return sprite;
    }


    // Céu e camadas de montanha/vila que se mexem mais devagar que a câmera
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

        CriarCamada("Montanhas", "bg_montanha", -90, 0.9f, -1.0f);
        CriarCamada("Bambu", "bg_bambu", -85, 0.8f, -1.5f);
        CriarCamada("Vila", "bg_vila", -80, 0.7f, -2.0f);
    }


    // Uma camada de fundo que repete para os lados
    void CriarCamada(string nome, string imagem, int ordem, float fator, float deslocamentoY)
    {
        Sprite sprite = Carregar(imagem);

        GameObject camada = new GameObject(nome);
        camada.transform.parent = raiz;

        SpriteRenderer sr = camada.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.drawMode = SpriteDrawMode.Tiled;                 // repete a imagem
        sr.size = new Vector2(400f, sprite.bounds.size.y);  // 400 unidades de largura
        sr.sortingLayerName = "Background";
        sr.sortingOrder = ordem;

        Parallax paralaxe = camada.AddComponent<Parallax>();
        paralaxe.fator = fator;
        paralaxe.deslocamentoY = deslocamentoY;
    }


    // Cria o Tilemap com os blocos # e T
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

        for (int x = 0; x < largura; x++)
        {
            for (int y = 0; y < altura; y++)
            {
                char letra = Letra(x, y);
                Vector3Int posicao = new Vector3Int(x, y, 0);

                if (letra == '#')
                {
                    // Se não tem pedra em cima, usa o bloco com a borda vermelha
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


    // Cria espinhos, lanternas, portão, inimigos e coloca o player
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
                else if (letra == 'L')
                {
                    NovoSprite("Lanterna", "lanterna", x + 0.5f, y + 1f, -8);
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


    void CriarPortao(int x, int y)
    {
        // O portão tem 2 blocos de largura e 3 de altura
        GameObject portao = NovoSprite("Portao", "portao", x + 0.5f, y + 1.5f, -2);

        BoxCollider2D colisor = portao.AddComponent<BoxCollider2D>();
        colisor.isTrigger = true;               // trigger: o player passa por dentro
        colisor.size = new Vector2(1.2f, 2.6f);

        portao.AddComponent<Portao>();
    }


    void CriarInimigo(int x, int y)
    {
        GameObject inimigo = Instantiate(inimigoModelo, raiz);
        inimigo.name = "Inimigo";
        inimigo.SetActive(true);    // o modelo fica desligado, a cópia liga

        ColocarNoChao(inimigo, x + 0.5f, y);
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
        seguir.minX = metadeLargura;
        seguir.maxX = Mathf.Max(metadeLargura, largura - metadeLargura);
        seguir.minY = metadeAltura - 1f;
        seguir.maxY = Mathf.Max(metadeAltura, altura - metadeAltura);

        seguir.IrParaOAlvo();   // já começa no lugar certo
    }
}
