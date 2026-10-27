# Arquitetura

## Visão geral

Arquitetura simples, orientada a componentes da Unity. Cada comportamento é um `MonoBehaviour` em um GameObject.
O único script "central" é o `GameManager`, que guarda vidas, estado do menu e sons. Os outros scripts falam com
ele por métodos `static` (`GameManager.PerderVida()` e `GameManager.Tocar("nome")`).

```
 teclas ──► Player ──► Rigidbody2D (velocidade)
              │  ├──► Animator (int "trans") + flipX
              │  └──► detecta chão/parede com Collider2D.Cast (tags Ground / Wall)
              └──► PlayerTiro ──► Instantiate(Shuriken) ──► Bala ──► Destroy(Enemy)

 Enemy ──► patrulha ──► atira (EnemyBullet = Bala + Espinho) ──► Animator trigger "Atacar"

 Espinho (espinho, inimigo, bala inimiga) ──► GameManager.PerderVida()
 GameManager ──► vidas, respawn, Game Over, menu (OnGUI), música e sons (Resources/Sons)
```

## Cena `jogo`

`Main Camera` (fixa), `Background`, `Grid/GroundTilemap` (tag Ground), `Floors/Statics` (plataformas `Floor`, tag
Ground), `Enemys/Enemy` e `Enemy (1)`, `Player/Sprite` (o player de verdade, com tag Player), espinhos e o objeto
`GameManager`.

## Decisões e convenções

- **Menu e HUD com `OnGUI`:** é a forma mais simples de fazer tela sem montar Canvas. Pode ser trocado por UI (uGUI) depois.
- **Pausa:** menu e Game Over usam `Time.timeScale = 0`. `Player`, `PlayerTiro` e `Enemy` saem do `Update` quando o tempo está parado.
- **`static` no GameManager** (`vidas`, `jaComecou`) mantém o valor quando a cena recarrega, por isso o menu só aparece na primeira vez.
- **Detecção de chão/parede:** `Collider2D.Cast` ao redor do player, filtrando por tag. Não precisa de objetos extras na cena.
- **Sons por nome** com `Resources.Load`: trocar o arquivo mantendo o nome já troca o som.
- **Entrada:** `Input.GetAxis/GetKeyDown` (sistema antigo). O Input System novo está instalado, mas não usado.

## Pontos para melhorar

- Bala e inimigo andam por `Translate` e atravessam paredes.
- Os frames do inimigo têm recortes de tamanhos diferentes, então o sprite balança um pouco.
- Nomes de classe diferentes dos arquivos (`PlayerTiro` em `PlayerAtack.cs`, `Bala` em `ammunition.cs`, `Espinho` em `Espinhos.cs`).
- Câmera fixa: se a fase ficar maior que a tela, será preciso uma câmera que siga o player.
- Assets com nomes genéricos (`Imagem do ChatGPT …`), controllers duplicados da shuriken e PNGs grandes.
- O projeto está dentro do OneDrive, que pode sincronizar a pasta `Library/` e causar conflitos.
