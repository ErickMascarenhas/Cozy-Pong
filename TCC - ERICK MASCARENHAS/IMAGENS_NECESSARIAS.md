# Imagens necessárias — TCC Cozy Pong

O texto **compila sem as imagens**: cada figura pendente aparece como uma caixa
com borda (`\fbox`) contendo a descrição do que deve entrar ali. Quando tiver
cada imagem, crie a pasta `images/` (ainda não existe), coloque o arquivo dentro
e troque o bloco `\fbox{...}` por:

```latex
\includegraphics[width=0.85\textwidth]{gameplay}
```

Formatos: `.png`, `.jpg` ou `.pdf` (vetorial de preferência).

| # | Rótulo (`\label`) | Arquivo sugerido | Seção | O que mostrar |
|---|---|---|---|---|
| 1 | `fig:gameplay` | `images/gameplay.png` | 3.2.2 | Captura in-game em 1ª pessoa: raquete na mão, mesa, bolas se aproximando pelas raias e a interface de pontuação/combo. |
| 2 | `fig:editor` | `images/editor.png` | 3.2.4 | Editor de *beatmaps*: trilha de notas rolando na vertical, tempo atual da música, tipo de nota selecionado, régua de tempo. |
| 3 | `fig:hud` | `images/hud.png` | 3.2.5 | Detalhe do HUD: placar, multiplicador de combo, indicador de vida/caixas de erro, texto de feedback (Perfect/Ok/Bad/Miss). |
| 4 | `fig:cenario` | `images/cenario.png` | 3.2.6 | Captura ampla do cenário *lo-fi*: iluminação suave, elementos decorativos, paleta quente. |
| 5 | `fig:modos` | `images/modos.png` | 3.3 | **4 painéis, um por condição:** (a) **C1** jogando Cozy Pong relaxante (poucas notas, ritmo calmo); (b) **C2** o mesmo jogo animado (mais notas, ritmo intenso); (c) **C3** sentado, de fones, só ouvindo a trilha, sem RV; (d) **C4** com o HMD na cena de meditação guiada. |
| 6 | `fig:setup` | `images/setup.jpg` | 3.8 | Participante equipado: **Polar H10** no tórax, eletrodos de **EDA nos dedos indicador e médio da mão não dominante**, HMD Meta Quest 3S, raquete na mão dominante. *O texto argumenta que a mão não dominante fica livre nas 4 condições — vale a foto deixar isso evidente.* |
| 7 | `fig:configs` | `images/configs.png` | 3.2.6 | **(nova, opcional)** As duas configurações de jogo lado a lado na mesma faixa, evidenciando a diferença de densidade de notas. Torna visual a manipulação testada pela hipótese H2. |

## Observações
- O logotipo do IC-UFAL (`Cap00/IC.jpg`) já está no lugar e é usado na capa.
- A figura **#5** mudou com a consolidação de 30/07/2026: agora são **quatro
  condições** (C1 relaxante, C2 animado, C3 trilha, C4 meditação em RV). A
  condição de **tênis de mesa real saiu** do trabalho.
- As figuras **#5(b)**, **#5(d)** e **#7** dependem de artefatos ainda a
  produzir (a configuração animada e a cena de meditação), então ficam por
  último.
- Se renomear os arquivos, ajuste o nome dentro do `\includegraphics{...}`.
