# Documentação do Jogo_2D

> O `README.md` da raiz é o diário de aulas. Esta pasta `docs/` é a documentação técnica do jogo.

| Documento | Conteúdo |
|---|---|
| [DOCUMENTACAO.md](DOCUMENTACAO.md) | Visão geral, como rodar, controles, estrutura de pastas, scripts |
| [ARQUITETURA.md](ARQUITETURA.md) | Como as peças se conectam, tags, física, animação, problemas conhecidos |
| [ROADMAP.md](ROADMAP.md) | Próximos passos priorizados |

> Atualização: o jogo agora tem uma trilogia de fases (veja [HISTORIA.md](HISTORIA.md)). As três fases estão prontas, com tela inicial e tela final.
> As fases são montadas por um arquivo de texto. A seção "Como editar o mapa" explica como.

## Visão geral

Plataforma 2D de ação em visão lateral. Um personagem (ninja) anda e pula por plataformas, atira shurikens
nos inimigos e reinicia a fase ao tocar nos espinhos. Projeto de estudo (versionamento + game design) feito
em Unity.

- **Engine:** Unity 6000.1.4f1 (Unity 6)
- **Render pipeline:** URP 17.1 com Renderer 2D
- **Entrada:** Input System 1.14 instalado, projeto configurado com `activeInputHandler: 2` (aceita os dois sistemas). Os scripts usam a API antiga (`Input.GetAxis`, `Input.GetKeyDown`).
- **Cenas:** `Fase1.unity` (índice 0), `Fase2.unity` (índice 1) e `Fase3.unity` (índice 2), todas na lista do Build Settings.
  Ao tocar no portão, o jogo vai para a próxima cena da lista.

## Como rodar

1. Instale o Unity Hub e o editor **6000.1.4f1**.
2. Clone o repositório e adicione a pasta no Unity Hub (Add project from disk).
3. Abra `Assets/Scenes/Fase1.unity` e aperte Play.

## Tela inicial e tela final

Desenhadas pelo `GameManager`. As ilustrações (pixel art feita com a arte do próprio ninja) ficam em
`Assets/Resources/Arte/tela_inicial.png` e `tela_final.png`: [tela_inicial.png](tela_inicial.png) e
[tela_final.png](tela_final.png).

- **Tela inicial:** ilustração com o ninja contra a lua, nome do jogo, subtítulo e botões **Jogar**, **Como jogar** e
  **Sair** (Enter também começa). O nome do jogo e o subtítulo ficam nos campos `Nome Do Jogo` e `Subtitulo` do objeto
  `GameManager`.
- **Tela final:** aparece ao abrir a cela. Mostra o ninja e o amigo lado a lado ao amanhecer, e o botão volta ao menu.
## Interface (HUD)

Desenhada pelo `GameManager` (`OnGUI`), em escala com o tamanho da tela. Ícones em `Assets/Resources/Arte/ui_*.png`.

- **Canto esquerdo:** corações de vida (cheios = restam, vazios = perdidos), altares acesos (por exemplo 1/3) e inimigos
  derrotados (por exemplo 2/4).
- **Canto direito:** nome da fase, objetivo (`@dica`), barra de progresso até o portão e a distância em metros.
- **Seta na borda da tela:** quando o portão não está à vista, uma seta pulsando aponta para ele, com a distância.
- **Dica dos controles** embaixo da tela nos primeiros 9 segundos de cada fase.
## Como editar o mapa

O mapa da Fase 1 é o arquivo de texto `Assets/Resources/Fases/fase1.txt`. Cada letra vira uma parte da fase quando
o jogo começa (o script `LevelBuilder` lê o arquivo). Dá para editar no Bloco de Notas, salvar e apertar Play.
Uma imagem do mapa inteiro está em [fase1_mapa.png](fase1_mapa.png).

| Letra | O que vira |
|---|---|
| `#` | chão sólido (pedra na vila e no desfiladeiro, terra na floresta; a borda de cima aparece sozinha quando não há chão em cima) |
| `T` | plataforma fina (telhado na vila, passarela de madeira no desfiladeiro, balsa de bambu na floresta) |
| `V` | pé de bambu, uma parede para pular (só existe no tema floresta, hoje sem fase) |
| `Q` | plataforma que quebra: treme, cai em 0,6 s e volta depois de 3 s |
| `^` | espinhos no chão |
| `<` `>` | espinhos na parede: `>` fica preso na parede da esquerda e aponta para a direita, `<` o contrário |
| `C` | altar de lanterna (checkpoint): ao passar, o player volta para ele quando perder uma vida |
| `P` | onde o player começa |
| `E` | inimigo |
| `G` | portão do fim da fase |
| `L` | lanterna (só enfeite) |
| `r` `e` `v` `u` | seta brilhante que mostra o caminho: `r` direita, `e` esquerda, `v` baixo, `u` cima |
| `B` | chefe: um inimigo maior, dourado, com 6 de vida e barra de vida na tela |
| `A` | cela do amigo (3 x 3 blocos). Só abre quando o chefe for derrotado e termina o jogo |
| `.` | vazio |

As linhas que começam com `@` não são mapa: `@titulo`, `@historia` e `@fim` são os textos que aparecem nas telas, `@dica` é uma frase curta com o objetivo da fase (aparece na tela da história e no HUD) e `@camera tamanho alturaExtra alturaExtraQueda` ajusta a câmera da fase (tamanho maior = mais zoom out; alturaExtra negativa = a câmera fica abaixo do player e mostra mais o que vem embaixo; o padrão é 5, 2 e -1,5). A Fase 2 usa `@camera 7 -2 -4.5`, e
`@tema desfiladeiro` troca o visual (sem essa linha o tema é a vila). Imagens de um tema começam com o nome dele, por exemplo `desfiladeiro_`
(por exemplo `desfiladeiro_tile_topo.png`); se uma imagem do tema não existir, o jogo usa a normal.

### Fase 2: Desfiladeiro Nebuloso (fase que desce)
Mapa: `Assets/Resources/Fases/fase2.txt` (76 x 68 blocos, tema `desfiladeiro`). Imagens: [fase2_mapa.png](fase2_mapa.png),
[fase2_poco.png](fase2_poco.png) e [fase2_visual.png](fase2_visual.png).
- **Diferente da Fase 1:** é de dia nublado, com paredões claros, cachoeiras e névoa branca, e o jogo é uma **descida**
  em vez de uma subida e de uma corrida para a direita. A câmera olha mais para baixo quando você cai rápido, e o
  player tem velocidade máxima de queda (`maxFallSpeed`).
- **Sala 1:** do mirante, desça por degraus de tábuas velhas que quebram.
- **Sala 2:** o poço com **espinhos nas paredes** (em lados alternados) e espinhos no chão. Desvie caindo ou
  deslizando na parede. Um altar fica logo antes.
- **Sala 3:** ponte de tábuas sobre o abismo, com um inimigo e o altar 2.
- **Sala 4:** zigue-zague descendo por ledges, com inimigo, espinhos na parede e o altar 3.
- **Sala 5:** o rio, com tábuas como pedras para atravessar até o portão, no fundo, embaixo do mirante.
- **Como se orientar:** setas brilhantes nos pontos de decisão, uma coluna de luz verde sobre o portão (aparece no fundo) e a dica "Desça até o portão de luz verde" na tela da história e no canto da tela.
- **Tetos de pedra com espinhos** impedem de cair direto do começo até o fim.
- Uma busca automática com simulação de física confirmou que há caminho do início ao portão (passando pelos três
  altares) e que o poço dos espinhos de parede é possível de atravessar. Isso não substitui jogar: ajuste vãos e
  espinhos no mapa se achar difícil ou fácil demais.

### Fase 3: A Fortaleza (fase final)
Mapa: `Assets/Resources/Fases/fase3.txt` (176 x 48 blocos, tema `fortaleza`). Imagens: [fase3_mapa.png](fase3_mapa.png),
[fase3_visual.png](fase3_visual.png) e [fase3_arena.png](fase3_arena.png).
- **Visual:** noite de tempestade, pedra azulada de castelo, colunas com estandartes verdes, torres ao longe, braseiros
  com fogo tremendo e brasas flutuando. Os `@camera 6.5 0 -3` mostram mais da fase.
- **Pátio:** ponte quebrada de tábuas, um guarda e espinhos, e a primeira torre (chaminé de pedra para subir).
- **Muralha e torre alta:** corredor no alto com um guarda e tábuas que quebram, altar 1, e uma chaminé de pedra
  com espinhos nas paredes (em lados alternados).
- **Salão superior:** guardas, espinhos, altar 2 e o poço que desce até a masmorra, com espinhos nas paredes.
- **Masmorra e arena:** corredor com guarda, espinhos e altar 3, uma ponte de tábuas e a arena do **capitão da guarda**
  (chefe com 6 de vida, que atira mais rápido e patrulha). Dá para desviar pelas plataformas da arena.
- **Cela:** no fim da arena. Se você tentar abrir antes de derrotar o chefe, aparece o aviso "A cela está trancada".
  Ao abrir, as grades somem e o jogo mostra a tela final.
- **Mecânicas:** só as que você já conhece (andar, pular, pulo de parede, atirar), mais o chefe com vida. Uma busca
  automática com simulação de física confirmou que existe caminho do início até a cela, passando pelos três altares.
Para criar uma nova fase: copie `fase1.txt` para `fase2.txt`, mude o desenho, duplique a cena `Fase1`, troque o
campo "Arquivo Mapa" do objeto `LevelBuilder` para `Fases/fase2` e coloque a cena nova no Build Settings.

A arte dos blocos e do fundo está em `Assets/Resources/Arte/` (imagens simples feitas por código, em pixel art noturna
azul-marinho e verde escuro, com luz quente só nas lanternas). O fundo tem 4 camadas: céu, montanhas, bambu e vila. Dá para trocar qualquer imagem mantendo o
mesmo nome.

## Controles

| Ação | Tecla |
|---|---|
| Andar | A / D ou setas (`Horizontal`) |
| Pular | Espaço (só no chão) |
| Atirar shuriken | F |

## Mecânicas atuais

- **Movimento horizontal** por velocidade direta no `Rigidbody2D`.
- **Pulo** só no chão (detectado com o collider do player, tag `Ground`). Soltar o espaço no meio do pulo corta a subida (pulo curto).
- **Parede:** no ar, encostado numa parede (`Ground` ou `Wall`) e empurrando contra ela, o player desliza devagar. Espaço faz o pulo da parede, que joga o player para longe dela.
- **Tiro:** instancia a shuriken no `PontoTiro`, que troca de lado conforme o personagem vira. A shuriken some após 3 s ou ao acertar um `Enemy`.
- **Inimigo:** patrulha de um lado para o outro, vira nos limites e atira no player quando ele está perto e na frente. Pisca vermelho ao atirar (animação de ataque).
- **Dano e vidas:** espinhos, inimigos e balas inimigas tiram 1 vida (começa com 3). Ao perder vida o player volta ao início, pisca e fica invencível por 1,5 s. Cair do mapa também tira uma vida. Sem vidas aparece Game Over.
- **Menu inicial** com botão Jogar (ou Enter), vidas na tela, **música e sons** (pulo, tiro, dano, etc.).

## Estrutura de pastas

```
Assets/
├─ Art/
│  ├─ Background/   Background_city.png
│  ├─ Player/       Player_art.png, Run/ (5 frames + Player.controller + Player_run.anim)
│  ├─ Enemy/        Enemy_art.png, Run/ (3 frames + Enemy.controller + enemy_run.anim)
│  └─ Shuriken/     8 frames + Shuriken.controller + Shuriken_Spin.anim
├─ PreFabs/         Shuriken.prefab, Triangle (1).prefab (espinho)
├─ Resources/       Arte/ (tiles e fundos), Fases/ (mapas em texto), Sons/
├─ Scenes/          Fase1.unity
├─ Scripts/         Player.cs, PlayerAtack.cs, Enemy.cs, ammunition.cs, Espinhos.cs
├─ Settings/        URP / Renderer2D / template de cena
├─ InputSystem_Actions.inputactions   (padrão do template, ainda não usado)
└─ Player_idle.anim                    (fora da pasta do Player)
```

## Scripts

| Arquivo | Classe | Objeto | Função |
|---|---|---|---|
| `Player.cs` | `Player` | Player (objeto "Sprite") | Movimento, pulo, pulo da parede, deslizar, flip do sprite, parâmetro `trans` do Animator |
| `PlayerAtack.cs` | `PlayerTiro` | Player | Posiciona o `PontoTiro`, instancia a shuriken ao apertar F |
| `ammunition.cs` | `Bala` | Shuriken e bala do inimigo | Anda em linha reta, some em 3 s, mata `Enemy` ao encostar (trigger) |
| `Enemy.cs` | `Enemy` | Enemy | Patrulha, vira, atira no player e dispara a animação de ataque |
| `Espinhos.cs` | `Espinho` | Espinho, inimigo, bala inimiga | Ao encostar no `Player`, chama `GameManager.PerderVida()` |
| `GameManager.cs` | `GameManager` | Objeto vazio "GameManager" | Menu, história da fase, vidas, Game Over, fim de fase, respawn, queda do mapa, música e sons |
| `LevelBuilder.cs` | `LevelBuilder` | Objeto vazio "LevelBuilder" | Lê o mapa de texto e cria chão (Tilemap), espinhos, portão, lanternas, inimigos, fundo e câmera |
| `CameraFollow.cs` | `CameraFollow` | Câmera (adicionado pelo LevelBuilder) | Segue o player sem sair do mapa |
| `Parallax.cs` | `Parallax` | Camadas do fundo | Faz o céu, as montanhas e a vila andarem em velocidades diferentes |
| `Portao.cs` | `Portao` | Portão do fim da fase | Ao receber o Player, termina a fase |
| `Checkpoint.cs` | `Checkpoint` | Altar (letra `C`) | Acende ao Player passar e guarda o ponto de retorno no GameManager |
| `PlataformaQuebra.cs` | `PlataformaQuebra` | Tábuas (letra `Q`) | Treme quando o Player pisa, some e volta depois de um tempo |
| `Seta.cs` | `Seta` | Setas (letras `r` `e` `v`) | Balança na direção que aponta e pisca, para guiar o jogador |
| `Cela.cs` | `Cela` | Cela (letra `A`) | Se o chefe ainda vive, avisa que está trancada. Senão, abre as grades e termina o jogo |
| `PiscarLuz.cs` | `PiscarLuz` | Braseiros da fortaleza | Faz o fogo tremer |
| `Vagalumes.cs` | `Vagalumes` | Objeto criado pelo LevelBuilder | Luzinhas flutuando ao redor da câmera (vaga-lumes ou poeirinhas de névoa) |

### Parâmetros expostos no Inspector

- `Player`: `speed` (6), `jumpForce` (10), `maxFallSpeed` (14), `wallSlideSpeed` (2), `wallJumpX` (7), `wallJumpTime` (0,2), `checkDistance` (0,1)
- `PlayerTiro`: `bala`, `pontoDeTiro`
- `Bala`: `velocidade` (7), `direcao`
- `Enemy`: `vida` (quantos tiros aguenta), `ehChefe`, `velocidade`, `distancia`, `bala`, `pontoTiro`, `distanciaTiro`, `tempoEntreTiros`
- `GameManager`: `vidasIniciais` (3), `tempoInvencivel` (1,5), `limiteQueda` (-15), `volumeEfeitos` (0,3), `volumeMusica` (0,12)

## Tags e layers

Tags usadas: `Ground` (chão, plataformas e tilemap, inclusive paredes), `Wall` (opcional, também vale como parede), `Enemy`, `Player`. A tag `Dano` existe no espinho mas nenhum script depende dela.

## Sons

Ficam em `Assets/Resources/Sons/` (pulo, tiro, tiro_inimigo, dano, inimigo_morre, clique, gameover e musica). O `GameManager.Tocar("nome")` carrega pelo nome do arquivo, então dá para trocar qualquer som mantendo o mesmo nome. Os sons atuais foram gerados por código (estilo 8 bits) e servem de provisórios.

## Animação

| Controller | Parâmetro | Clips |
|---|---|---|
| `Player.controller` | int `trans` (0 parado, 1 correndo) | `Player_idle`, `Player_run` |
| `Enemy.controller` | trigger `Atacar` | `Enemy_Run` (3 frames em loop), `Enemy_Attack` (pisca vermelho) |
| `Shuriken.controller` | — | `Shuriken_Spin` |

## Histórico de desenvolvimento

Resumo pelo `git log`: fundo e colisões básicas → player e animação → prefabs → inimigo patrulhando → tiro →
bala com tempo de vida → shuriken animada → estética do inimigo → novo fundo. Branches remotas: `main` e `testes`.
