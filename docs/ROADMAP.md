# Roadmap

Ordem sugerida. Cada fase deixa o jogo jogável.

## Fase 1 — Base sólida (arrumar a casa)
- [ ] Renomear as classes para bater com os arquivos (`PlayerTiro`/`Bala`/`Espinho` ou renomear os arquivos), mantendo os `.meta` para não quebrar a cena.
- [ ] Converter `Player.cs` para UTF-8 e limpar usings e variáveis sem uso.
- [ ] Detecção de chão com `OverlapCircle` nos pés (`groundCheck` + `LayerMask`), movimento em `FixedUpdate`.
- [ ] Organizar assets: renomear frames (`player_run_01…`), mover `Player_idle.anim`, apagar controllers duplicados da shuriken.
- [ ] Padronizar o espelhamento (usar `flipX` para player e inimigo) e terminar o `Animator` do inimigo.

## Fase 2 — Combate e vida
- [ ] Componente `Vida` reutilizável (player e inimigo) com `TomarDano(int)` e evento de morte.
- [ ] Inimigo causa dano ao tocar no player (usar a tag `Dano`), com invulnerabilidade curta e knockback.
- [ ] Bala colide com o cenário (`Ground`) e some.
- [ ] Cooldown e limite de munição da shuriken.
- [ ] Morte do player com tela de "Game Over" no lugar do reload direto.

## Fase 3 — Estrutura do jogo
- [ ] `GameManager` (estado, pontuação, vidas, reinício).
- [ ] HUD com vidas e pontos (uGUI já instalado).
- [ ] Checkpoints em vez de voltar ao início.
- [ ] Menu inicial, pausa e fim de fase; adicionar cenas ao Build Settings.
- [ ] Migrar entrada para o Input System (`InputSystem_Actions` já está no projeto) com suporte a gamepad.

## Fase 4 — Conteúdo
- [ ] Segunda e terceira fase, usando prefabs para plataformas e inimigos.
- [ ] Novos inimigos (que atiram, que perseguem) e itens (vida, munição, moedas).
- [ ] Som: música, pulo, tiro, dano (`AudioSource`/`AudioMixer`).
- [ ] Efeitos: partículas de morte e impacto, tremor de câmera, Cinemachine para seguir o player.

## Fase 5 — Polimento e entrega
- [ ] Otimizar tamanho dos sprites (Max Size, compressão, atlas) ou usar Sprite Atlas.
- [ ] Teste de build (Windows/WebGL) e página no itch.io.
- [ ] Capturas de tela e GIF para a documentação.
- [ ] Testes de Play Mode para pulo, tiro e dano (Test Framework já instalado).

## Dúvidas de design a decidir
- Tem objetivo de fase (chegar a uma porta, derrotar chefe, coletar itens)?
- O jogador tem vida em corações ou morre de um toque?
- A shuriken é ilimitada ou coletável?
