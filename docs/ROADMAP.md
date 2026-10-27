# Roadmap

## Já feito
- [x] Movimento, pulo no chão e pulo na parede (com deslizar)
- [x] Tiro do player e do inimigo, animação de ataque do inimigo
- [x] Vidas, respawn com invencibilidade, Game Over e queda do mapa
- [x] Menu inicial, HUD de vidas, música e sons
- [x] Animação de andar do inimigo e de parado do player
- [x] Comentários nos scripts

## Trilogia (veja HISTORIA.md)
- [x] Roteiro das 3 fases
- [x] Fase 1: Telhados da Vila (mapa, arte, câmera, portão, história)
- [x] Fase 2: Desfiladeiro Nebuloso (fase que desce: espinhos na parede, altares, tábuas que quebram, névoa e cachoeiras)
- [ ] Fase 3: Fortaleza e a cela do amigo (final do jogo e tela de vitória)
- [ ] Arte nova de cada fase (hoje é pixel art simples gerada por código)

## Para refinar (próxima rodada)
- [ ] Ajustar a "sensação" do pulo (`speed`, `jumpForce`, `wallSlideSpeed`, `wallJumpX`) jogando e testando
- [ ] Animação de pulo/queda do player (precisa de arte)
- [ ] Frames de ataque do inimigo (hoje é só um piscar vermelho)
- [ ] Os frames do inimigo têm tamanhos de recorte diferentes, então o sprite balança um pouco ao andar
- [ ] Bala e inimigo atravessam paredes (andam por `Translate`)
- [ ] Trocar os sons gerados por sons de verdade

## Ideias futuras
- [ ] Objetivo da fase (bandeira ou porta) e tela de vitória
- [ ] Mais fases, mais inimigos e itens (vida, moedas)
- [ ] Câmera que segue o player
- [ ] Organizar assets (renomear frames "Imagem do ChatGPT…", apagar controllers duplicados da shuriken)
- [ ] Reduzir o tamanho dos PNGs
- [ ] Nomes das classes iguais aos dos arquivos (`PlayerAtack.cs` tem `PlayerTiro`, `ammunition.cs` tem `Bala`, `Espinhos.cs` tem `Espinho`)
