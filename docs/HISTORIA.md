# História e fases

Primeira versão do roteiro. Está aberto para mudar nomes, locais e detalhes.

## A história

Um ninja e seu melhor amigo treinaram juntos a vida inteira no mesmo clã. Uma noite, os soldados do senhor da
fortaleza invadem a vila e levam o amigo preso, sem deixar rastro além de pegadas na direção da montanha.
O ninja vai atrás. Para chegar à cela do amigo, ele precisa atravessar a vila em chamas, o desfiladeiro e a
fortaleza, tudo usando o que treinou: pulos longos, paredes e shurikens.

(Os nomes do ninja e do amigo ainda não foram definidos.)

## Visual

Pixel art sombrio de cenas à noite, com clima de ninja: **azul-marinho e verde-floresta bem escuros, silhuetas de
pagodes e bambuzais, névoa leve entre as camadas do fundo e luz quente só nas lanternas**. Pedra com musgo escuro,
telhados verde-acinzentados, portão de jade e luar. O ninja mantém sua arte preta e vermelha, que se destaca contra o
cenário.

## Trilogia de fases

### Fase 1: Telhados da Vila (tutorial de parkour)
- **Cenário:** vila à noite, telhados, lanternas, lua grande, silhuetas de pagodes.
- **Ensina:** andar, pular vãos, espinhos, subir em plataformas, pulo na parede e atirar shurikens.
- **Ritmo:** começo fácil, o primeiro inimigo no meio e um poço de parede (pular de uma parede para a outra) antes do portão.
- **Final:** o portão da vila. Uma mensagem avisa que o amigo foi levado na direção do desfiladeiro e da montanha.

### Fase 2: Desfiladeiro Nebuloso (descida) - pronta
- **Cenário:** um desfiladeiro cortado por cachoeiras, de dia e coberto de névoa, com paredões de pedra, pontes de
  tábuas e um rio lá embaixo. Muito diferente da noite da Fase 1.
- **Novidades:** é uma **fase que desce**, com espinhos nas paredes dos poços, tábuas velhas que quebram, tetos de
  pedra com espinhos e três altares de lanterna de jade (checkpoints).
- **Estrutura:** do mirante, desce por degraus de tábuas, passa por um poço com espinhos nas paredes, atravessa uma
  ponte, desce em zigue-zague até o rio e chega ao portão do vale.
- **Inimigos:** guardas que atiram nas pontes e nos ledges.
- **Final:** do outro lado do portão, uma trilha sobe até a fortaleza.

### Fase 3: Fortaleza (desafio final)
- **Cenário:** muralhas, torres, pátio interno, corredores de pedra.
- **Mistura tudo:** pulos longos, poços de parede, espinhos e vários inimigos.
- **Final:** a cela. O ninja encontra o amigo, abre a cela e aparece a tela de vitória.

## Sistema do jogo (como as fases funcionam)

- Cada fase é uma cena (`Fase1`, `Fase2`, `Fase3`).
- O mapa de cada fase é um arquivo de texto em `Assets/Resources/Fases/` (veja o `LevelBuilder`).
- Tocar no portão no fim da fase mostra o cartão da história e vai para a próxima fase.
- As vidas continuam entre as fases. Ao perder todas, o jogo recomeça na Fase 1.
