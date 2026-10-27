# Arquitetura

## Visão geral

Arquitetura simples e orientada a componentes da Unity. Cada comportamento é um `MonoBehaviour` independente
em um GameObject. Não há gerenciador de jogo, singletons nem eventos: a comunicação é por colisão/trigger e
por `GetComponent` direto.

```
                      ┌────────────┐  GetComponent   ┌─────────────┐
 tecla F ───────────► │ PlayerTiro │ ──────────────► │ SpriteRend. │ (lê flipX)
                      └─────┬──────┘                 └─────────────┘
                            │ Instantiate + define direcao
                            ▼
                      ┌────────────┐  trigger "Enemy"  ┌────────┐
                      │   Bala     │ ────────────────► │ Enemy  │ Destroy()
                      └────────────┘                   └────────┘

 Input ──► Player ──► Rigidbody2D (velocidade/impulso)
              │  └──► Animator (int "trans") + SpriteRenderer.flipX
              └── OnCollision "Ground" ──► isGraunded

 Espinho ── colisão "Player" ──► SceneManager.LoadScene(0)
```

## Cena `jogo`

Hierarquia (resumida): `Main Camera`, `Background_City`, `Grid/GroundTilemap` (chão em tilemap),
`Floors/Statics` (plataformas `Floor…`), `Enemys/Enemy`, `Player` (com filho `PontoTiro`), espinhos (instâncias
do prefab `Triangle (1)`).

## Decisões e convenções atuais

- **Física:** movimento do player via `Rigidbody2D.linearVelocity` (API do Unity 6). Bala e inimigo andam por `transform.Translate`, sem física.
- **Detecção de chão:** por colisão com a tag `Ground` (`OnCollisionEnter2D/Exit2D`).
- **Dano/morte:** inexistente para o jogador além dos espinhos. Inimigo morre instantaneamente.
- **Direção do player:** é guardada só em `SpriteRenderer.flipX` e lida pelo `PlayerTiro`.
- **Reinício:** recarregar a cena de índice 0.

## Problemas conhecidos

Ordenados por impacto.

1. **Nome da classe diferente do nome do arquivo.** `PlayerAtack.cs` contém `PlayerTiro`, `ammunition.cs` contém `Bala` e `Espinhos.cs` contém `Espinho`. A Unity espera os dois iguais. Funciona hoje pelos GUIDs já gravados na cena, mas é frágil: dá aviso/erro ao adicionar o script por código ou pelo Inspector e confunde na hora de achar o código.
2. **Inimigo não machuca o player.** `Enemy.cs` só patrulha. O jogador atravessa ou empurra o inimigo sem consequência (a tag `Dano` existe e não é usada).
3. **Chão por contagem de colisão frágil.** `isGraunded` vira `false` ao sair de *qualquer* colisão com `Ground`. Em tilemap com vários colliders ou ao tocar numa parede de `Ground`, o pulo pode falhar ou permitir pulo no ar. O ideal é `OverlapCircle`/raycast nos pés.
4. **Input antigo com Input System instalado.** O projeto usa `Input.GetAxis/GetKeyDown` e tem `InputSystem_Actions.inputactions` sem uso. Funciona porque `activeInputHandler` = ambos, mas é duplicação.
5. **Movimento em `Update` mexendo no Rigidbody.** Física deve ir em `FixedUpdate`. Hoje a velocidade é setada por frame (aceitável), mas o pulo por `AddForce` em `Update` depende do framerate de entrada.
6. **Bala e inimigo sem física.** `Translate` ignora colisão com parede: a bala atravessa o cenário até os 3 s e o inimigo anda "no ar" se a plataforma for menor que `distancia`.
7. **Inimigo é destruído com `Destroy` direto na bala.** Sem vida, sem animação de morte, sem pontuação. A bala também colide só com tag `Enemy`; no cenário ela passa por tudo.
8. **Inimigo vira por `localScale.x`, player por `flipX`.** Duas formas diferentes de espelhar, e o `Animator` do inimigo está incompleto (commit "falta arrumar a animação dele").
9. **Limpeza de código:** `using static UnityEngine.RuleTile.TilingRuleOutput;` sobrando em `Enemy.cs`; variáveis `animator`/`spriteRenderer` do inimigo nunca usadas; `isGraunded` com erro de digitação; comentário "Vira para a esquerda" repetido nos dois lados; `Player.cs` salvo em codificação Windows-1252 (acentos quebram em editores UTF-8); `using SceneManagement` não usado em `Player.cs`.
10. **Assets:** frames com nomes `Imagem do ChatGPT …` (espaços e vírgulas), `Player_idle.anim` solto na raiz de `Assets`, `Triangle (1).prefab` com nome genérico, controllers duplicados na pasta da shuriken (`Munition.controller`, `Shuriken.controller` e um terceiro com nome de imagem). PNGs grandes (0,5 a 2,4 MB cada) para sprites pequenos.
11. **Repositório:** `.gitignore` está correto (nenhum arquivo de `Library/`, `Temp/` ou `Logs/` versionado). O projeto está dentro do OneDrive, que pode sincronizar `Library/` e causar conflitos.
