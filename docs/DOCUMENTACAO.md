# Documentação do Jogo_2D

> O `README.md` da raiz é o diário de aulas. Esta pasta `docs/` é a documentação técnica do jogo.

| Documento | Conteúdo |
|---|---|
| [DOCUMENTACAO.md](DOCUMENTACAO.md) | Visão geral, como rodar, controles, estrutura de pastas, scripts |
| [ARQUITETURA.md](ARQUITETURA.md) | Como as peças se conectam, tags, física, animação, problemas conhecidos |
| [ROADMAP.md](ROADMAP.md) | Próximos passos priorizados |

## Visão geral

Plataforma 2D de ação em visão lateral. Um personagem (ninja) anda e pula por plataformas, atira shurikens
nos inimigos e reinicia a fase ao tocar nos espinhos. Projeto de estudo (versionamento + game design) feito
em Unity.

- **Engine:** Unity 6000.1.4f1 (Unity 6)
- **Render pipeline:** URP 17.1 com Renderer 2D
- **Entrada:** Input System 1.14 instalado, projeto configurado com `activeInputHandler: 2` (aceita os dois sistemas). Os scripts usam a API antiga (`Input.GetAxis`, `Input.GetKeyDown`).
- **Cena única:** `Assets/Scenes/jogo.unity` (build index 0)

## Como rodar

1. Instale o Unity Hub e o editor **6000.1.4f1**.
2. Clone o repositório e adicione a pasta no Unity Hub (Add project from disk).
3. Abra `Assets/Scenes/jogo.unity` e aperte Play.

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
├─ Scenes/          jogo.unity
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
| `GameManager.cs` | `GameManager` | Objeto vazio "GameManager" | Menu, vidas, Game Over, respawn, queda do mapa, música e sons |

### Parâmetros expostos no Inspector

- `Player`: `speed` (6), `jumpForce` (10), `wallSlideSpeed` (2), `wallJumpX` (7), `wallJumpTime` (0,2), `checkDistance` (0,1)
- `PlayerTiro`: `bala`, `pontoDeTiro`
- `Bala`: `velocidade` (7), `direcao`
- `Enemy`: `velocidade`, `distancia`, `bala`, `pontoTiro`, `distanciaTiro`, `tempoEntreTiros`
- `GameManager`: `vidasIniciais` (3), `tempoInvencivel` (1,5), `limiteQueda` (-15)

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
