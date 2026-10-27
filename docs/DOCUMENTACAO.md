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
- **Pulo** por impulso vertical, permitido só quando em contato com objeto de tag `Ground`.
- **Tiro:** instancia o prefab da shuriken no `PontoTiro`, que troca de lado conforme o personagem vira. A shuriken some após 3 s ou ao acertar um `Enemy`.
- **Inimigo:** patrulha de um lado para o outro e vira o sprite nos limites. Morre com um tiro.
- **Espinhos:** tocar neles recarrega a cena (reinício da fase).

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
| `Player.cs` | `Player` | Player | Movimento, pulo, flip do sprite, parâmetro `trans` do Animator, detecção de chão |
| `PlayerAtack.cs` | `PlayerTiro` | Player | Posiciona o `PontoTiro`, instancia a bala ao apertar F e define a direção |
| `ammunition.cs` | `Bala` | Prefab Shuriken | Anda em linha reta, destrói-se em 3 s, mata `Enemy` ao encostar (trigger) |
| `Enemy.cs` | `Enemy` | Enemy | Patrulha entre `posição inicial ± distancia` e vira o `localScale.x` |
| `Espinhos.cs` | `Espinho` | Prefab espinho | Em colisão com `Player`, chama `SceneManager.LoadScene(0)` |

### Parâmetros expostos no Inspector

- `Player.speed` (3)
- `PlayerTiro.bala`, `PlayerTiro.pontoDeTiro`
- `Bala.velocidade` (7), `Bala.direcao`
- `Enemy.velocidade` (2), `Enemy.distancia` (2)

## Tags e layers

Tags: `Ground`, `Dano`, `Enemy`, `Player` (padrão da Unity). `Dano` existe mas nenhum script usa ainda.

## Animação

| Controller | Parâmetro | Clips |
|---|---|---|
| `Player.controller` | int `trans` (0 parado, 1 correndo) | `Player_idle`, `Player_run` |
| `Enemy.controller` | — | `enemy_run` |
| `Shuriken.controller` | — | `Shuriken_Spin` |

## Histórico de desenvolvimento

Resumo pelo `git log`: fundo e colisões básicas → player e animação → prefabs → inimigo patrulhando → tiro →
bala com tempo de vida → shuriken animada → estética do inimigo → novo fundo. Branches remotas: `main` e `testes`.
