# Plano de adequação do Cozy Pong à versão experimental

**O que este documento é:** a lista das mudanças que o aplicativo precisa sofrer
para que o experimento descrito no Capítulo 3 do TCC seja executável, e para que
os dados que ele produzir sejam analisáveis e reportáveis.

**O que ele não é:** um plano de melhoria do jogo. Nada aqui é sobre deixar o
Cozy Pong melhor. É sobre deixá-lo *medível* e *idêntico entre participantes*.
Alguns itens tornam o jogo deliberadamente menos flexível.

**Estado do código analisado:** Unity 6000.2.10f1, 29 scripts próprios em
`Assets/My/Scripts`, 53 músicas configuradas como GameObjects na cena
`Cozy Pong.unity`, 53 beatmaps em 4 níveis de dificuldade, cena
`BeatMapEditor.unity` separada.

---

## 1. O que o protocolo exige do aplicativo

Extraído do Capítulo 3 e dos Apêndices. Cada requisito abaixo é uma frase que já
está escrita no TCC e que hoje o aplicativo não cumpre.

| # | Requisito | Origem | Estado |
|---|---|---|---|
| R1 | Exposição de **duração idêntica** nas quatro condições | §3.9, cronograma, etapa 8 | ausente |
| R2 | O app **emite os marcadores temporais** em C1, C2 e C4 | §3.10 | ausente |
| R3 | Telemetria com **erro de sincronização** `e_i` por evento | §3.10, Eq. 3.1 | ausente |
| R4 | Trilha, **ordem e volume idênticos** entre C1 e C3; volume reduzido padronizado em C4 | §3.9 | parcial |
| R5 | **Meditação guiada no mesmo binário**, não em app de terceiros | §3.2.9 | ausente |
| R6 | **Regime imortal** em C1, **caixas de erro** em C2 | §3.2.8 | existe, mal exposto |
| R7 | Tarefa neutra de **familiarização com RV** que não reproduz a jogabilidade | §3.9 | ausente |
| R8 | **Versão do aplicativo** registrada junto aos dados | Cap. 4, relato de equipamentos | ausente |
| R9 | Parametrização numérica **documentada e fixa** das duas configurações | §3.2.8 (`\todo`) | disperso no Inspector |
| R10 | **Interrupção imediata** da atividade a pedido do participante | TCLE §6, §3.14 | parcial |

---

## 2. Diagnóstico: sete defeitos a corrigir antes de qualquer coleta

Estes não são pedidos de recurso novo. São problemas no código atual que
invalidariam os dados.

### D1 — Cada rebatida é contabilizada duas vezes

`BallGameLogic` e `GuidedBall` são **ambos** componentes da bola, e **ambos**
respondem a `OnCollisionEnter` com a raquete chamando
`GameScoreManager.RegisterHit`.

Pior: usam limiares diferentes para a mesma decisão.

- `BallGameLogic`: perfeito ≤ 0,2 s, ok ≤ 0,4 s
- `GuidedBall`: perfeito ≤ 0,3 s, ok ≤ 0,6 s

Uma única rebatida pode ser registrada como `Perfect` por um e `Ok` pelo outro,
inflando `totalHits`, o combo e o placar. **Qualquer métrica de desempenho
extraída hoje está corrompida.**

*Correção:* remover o bloco de pontuação de `GuidedBall.OnCollisionEnter`,
deixando ali apenas a física do retorno. `BallGameLogic` passa a ser a única
autoridade sobre classificação de acerto.

### D2 — O volume não é determinístico

Existem **dois sistemas de volume concorrentes**:

- `AudioSettingsManager` → AudioMixer, chaves `MasterVol` / `MusicVol` / `SFXVol`
- `VolumeManager` → `AudioListener.volume`, chave `soundVolume`

E ainda `WristMenuManager.ToggleMute`, que ao desmutar joga
`AudioListener.volume = 1f`, descartando o valor salvo.

O protocolo exige volume idêntico entre participantes e entre C1 e C3. Hoje o
volume persiste de um participante para o outro em `PlayerPrefs` e pode ser
alterado no meio da sessão pelo menu de pulso.

*Correção:* um único caminho de áudio (o mixer). Em modo experimento, volume
travado no valor do `ExperimentConfig`, sliders desabilitados, mute removido.

### D3 — A aleatoriedade não é semeada

`ServeManager.CalculatePhysicsForBeat` sorteia o ponto de saída e o de chegada;
`PerformServe` sorteia o lado do retorno; `BallSoundController` sorteia o clipe e
o pitch. Tudo com `UnityEngine.Random`, sem semente.

Dois participantes ouvindo a mesma música recebem **sequências espaciais
diferentes**. Isso não é apenas irreprodutível: é variância adicional dentro da
condição, exatamente onde o desenho intra-sujeito tenta reduzi-la.

*Correção:* `System.Random` semeado, com a semente vinda do `ExperimentConfig` e
gravada no log da sessão. Mesma condição ⇒ mesma sequência para todos.

### D4 — Estado persistente vaza entre participantes

`ScoreResultUI` e `SongSelectionMenuUI` gravam recorde, nota e cor em
`PlayerPrefs` por música. O participante 2 vê o recorde do participante 1.

Recorde alheio na tela é pressão de desempenho — o construto que o estudo mede.

*Correção:* em modo experimento, nenhuma leitura nem escrita de `PlayerPrefs` de
desempenho, e limpeza automática no início de cada sessão.

### D5 — A duração da exposição é definida pela música

`MonitorMusicEnd` encerra quando o clipe acaba. Uma faixa lo-fi tem 2 a 4
minutos; a etapa 8 do cronograma pede **15 minutos idênticos nas quatro
condições**. Hoje a sessão dura o que a música durar.

*Correção:* o encerramento passa a ser por **cronômetro**, não por fim de faixa,
com a playlist encadeada até o tempo fechar.

### D6 — Não existe nenhum registro temporal

Nenhum timestamp, nenhum arquivo de saída, nenhum marcador. §3.10 afirma que o
aplicativo emite os marcadores em C1, C2 e C4, e a Equação 3.1 define uma métrica
que depende de tempos de evento. Nada disso existe.

Além disso, `ServeManager` usa `Time.time` como relógio da sessão. Áudio e
`Time.time` divergem ao longo de 15 minutos. Para alinhar eventos a batidas, o
relógio precisa ser `AudioSettings.dspTime`.

*Correção:* Bloco B, adiante.

### D7 — A luz reativa pisca em arco-íris

```csharp
_light.color = Color.HSVToRGB(Mathf.Repeat(Time.time * colorChangeSpeed, 1f), 1f, 1f);
```

Saturação 1, brilho 1, ciclo contínuo de matiz, intensidade modulada pelos
graves. Isso é uma luz de balada.

Dois problemas. Contradiz frontalmente a caracterização "aconchegante" que o
Capítulo 3 atribui à configuração relaxante — trabalhando *contra* a manipulação
justamente em C1. E estímulo visual pulsante é o risco de crise fotossensível que
o TCLE declara.

*Correção:* paleta fixa e quente por configuração, com modulação de intensidade
suave e limitada. Ciclo de matiz apenas na configuração animada, se você quiser
mantê-lo, e ainda assim com limite de frequência.

---

## 3. As mudanças

Sete blocos. A ordem entre eles está na Seção 6.

### Bloco A — Modo Experimento

**Problema que resolve:** hoje a configuração de cada condição está espalhada por
53 GameObjects no Inspector. Não há como afirmar, num TCC, "a configuração
relaxante usava estes parâmetros" — nem como garantir que ela foi a mesma nas 32
sessões.

**A criar:**

`ExperimentConfig` (ScriptableObject) — um asset por condição, versionável e
citável no texto:

```
identificador da condição (C1 / C2 / C4 / FAM)
playlist ordenada (faixa + beatmap + fator de densidade)
duração da exposição (s)
regime de erro: imortal | caixas de erro
tempo de vida da bola (s)          <- hoje privado, 1.72f
altura do arco (m)                 <- hoje privado, 0.25f
limiares de força (homerun / ok)
limiares de tempo (perfeito / ok)
volume mestre (dB) e volume de música (dB)
preset visual (paleta, intensidade de luz, estilo do AudioWaveRing)
exibe placar / combo / nota?
semente do gerador aleatório
```

Quatro assets: `C1_Relaxante`, `C2_Animado`, `C4_Meditacao`, `FAM_Familiarizacao`.

`ExperimentSessionManager` — ponto de entrada único. Recebe **ID do participante,
número da sessão e condição**, aplica o config correspondente, executa a exposição
do começo ao fim e encerra. Sem escolha de música, sem lobby, sem menu de
dificuldade.

**A modificar:**

- `ServeManager`: expor `ballLifeTime` e `arcHeight`; ler os parâmetros do
  `ExperimentConfig` ativo em vez do Inspector; trocar `Time.time` por
  `AudioSettings.dspTime`.
- `GameScoreManager`: `isImmortal` e `usingErrorBoxes` passam a vir do config.
- Lobby de 53 músicas: desativado em modo experimento. Continua existindo para
  desenvolvimento e para a versão pública do jogo.

### Bloco B — Marcadores e telemetria

**Problema que resolve:** R2, R3, R8 — e viabiliza o Capítulo 4 inteiro.

**A criar:** `SessionLogger`, singleton, escrevendo em
`Application.persistentDataPath`. Três arquivos por sessão, nomeados
`P07_S03_C1_2026-09-14T15-22-10.<tipo>.csv`:

**1. `meta`** — uma linha, tudo que o Capítulo 4 precisa reportar:

```
participante, sessao, condicao, config_asset, semente,
versao_app, versao_unity, data_build, dispositivo,
volume_mestre_db, volume_musica_db, duracao_alvo_s,
fps_mediano, quadros_perdidos, taxa_de_atualizacao_hz,
inicio_unix_ms, fim_unix_ms
```

O `fps_mediano` e os quadros perdidos entram porque §3.9 exige ambiente estável e
porque queda de taxa de quadros é causa conhecida de *cybersickness*. Se um
participante teve uma sessão instável, isso precisa estar visível no dado, não ser
descoberto depois.

**2. `markers`** — um evento por linha:

```
unix_ms, dsp_s, sessao_s, marcador, detalhe
```

Marcadores mínimos: `SESSAO_INICIO`, `EXPOSICAO_INICIO`, `FAIXA_INICIO`,
`FAIXA_FIM`, `PAUSA_INICIO`, `PAUSA_FIM`, `EXPOSICAO_FIM`, `INTERRUPCAO` (com
código de motivo), `SESSAO_FIM`. Em C4, também o início de cada um dos cinco
blocos da narração do Apêndice C.

**3. `eventos`** — só em C1 e C2, uma linha por nota:

```
indice, tipo_bola, ponto_saida, ponto_chegada,
batida_alvo_ms, lancamento_ms, contato_ms,
e_i_ms, classificacao, velocidade_raquete, combo_no_momento
```

`e_i_ms` é literalmente a Equação 3.1: o menor valor absoluto de
(tempo do evento − tempo da batida) sobre as batidas do beatmap. Com esse arquivo,
a média `ē` do Capítulo 4 sai de uma linha de código.

**Transferência:** `adb pull` do `persistentDataPath` ao fim de cada sessão. Nunca
apagar do dispositivo antes de confirmar a cópia.

**Privacidade (exigido pelo TCLE §8):** o log grava **apenas o código do
participante** (P07). Nome, número de série do headset e qualquer identificador de
dispositivo ficam fora do arquivo.

### Bloco C — Padronização do estímulo

**Problema que resolve:** R1, R4, D2, D5.

- **Playlist encadeada por tempo.** O `ExperimentSessionManager` toca a lista na
  ordem definida até o cronômetro fechar, com corte no tempo exato. Intervalo entre
  faixas fixo e idêntico em C1 e C3.
- **A playlist de C1 é a playlist de C3.** Mesmas faixas, mesma ordem, mesmo volume
  absoluto. É isso que torna o contraste C1×C3 interpretável.
- **Volume travado.** Um único caminho (mixer), valor em dB vindo do config,
  sliders e mute desabilitados em modo experimento.
- **Atenuação de C4 explícita.** "Volume reduzido" precisa virar um número em dB,
  registrado no config e citado no Capítulo 3.

### Bloco D — Cena de meditação guiada (C4)

**Problema que resolve:** R5. Hoje esta condição simplesmente não existe.

O roteiro completo já está escrito no **Apêndice C**, em cinco blocos cronometrados
(0:00–2:00 acomodação, 2:00–5:00 respiração, 5:00–9:00 contagem 4-6, 9:00–12:30
varredura corporal, 12:30–15:00 encerramento), com os silêncios marcados.

**A produzir:**

- Cena sóbria e estática: sem mesa, sem raquete, sem placar, sem partículas, sem
  movimento súbito de câmera. Iluminação constante.
- Gravação da narração seguindo o Apêndice C, em blocos separados, para que o app
  emita um marcador no início de cada um.
- Trilha lo-fi no volume atenuado do Bloco C.
- Encerramento por cronômetro, na mesma duração das demais condições.

### Bloco E — Familiarização neutra

**Problema que resolve:** R7. §3.9 pede "uma tarefa neutra e curta de familiarização
com a RV que **não reproduz a jogabilidade** do Cozy Pong". A restrição existe para
que a primeira sessão de C1 não tenha vantagem de treino sobre as outras.

Cena curta e fixa: ambiente neutro, olhar em volta, pegar e recolocar um objeto
simples, ajustar o HMD, confirmar conforto. Sem bola, sem raquete, sem ritmo, sem
pontuação. Duração fixa, marcador de início e fim.

### Bloco F — Correções

D1 a D7 da Seção 2. Todas são pré-requisito do piloto, não do estudo.

### Bloco G — Segurança e conformidade com o TCLE

O TCLE declara garantias específicas. O aplicativo precisa sustentá-las.

- **Interrupção imediata** (TCLE §6): controle do pesquisador que encerra a
  exposição em qualquer instante e grava `INTERRUPCAO` com código de motivo. Hoje
  só existe o menu de pulso do participante.
- **Botão único de parada para o participante**: grande, sempre acessível, sem
  navegação de menu. Um participante enjoado não navega submenus.
- **Verificação da área de jogo** antes de começar, com limite visível — o TCLE
  lista tropeço e colisão entre os riscos de jogo em pé.
- **Redução de estímulo pulsante** (D7), que reduz na prática o risco fotossensível
  que declaramos.
- **Sem locomoção artificial.** O jogo já é estacionário; é a escolha correta para
  *cybersickness* e vale registrar no Capítulo 3 como tal.

---

## 4. Decisões que dependem de você

Estas mudam o que eu implemento. Marquei minha recomendação, mas a escolha é sua.

**D1 — Como C3 é entregue?**
O TCC diz "sentado, sem RV, com fones". Mas a trilha precisa ser bit a bit a mesma
de C1, no mesmo volume absoluto.
→ *Recomendo:* modo `C3_Trilha` no mesmo aplicativo, rodando no PC, com o mesmo
mixer e o mesmo config de playlist, tela apagada e áudio por fones. Mesmo código,
mesmo caminho de áudio, marcadores automáticos. A alternativa (tocar de um celular)
quebra R4 e obriga marcação manual.

**D2 — A duração da exposição é 15 min?**
Está assim no TCC, mas o piloto (P4) pode reduzir para 12 ou 10.
→ *Recomendo:* implementar como parâmetro do config desde já e fixar o valor
**depois** do piloto. Nada no código deve assumir 15.

**D3 — Quais faixas em cada playlist?**
Você tem 53 faixas com o BPM no nome, em 4 dificuldades. C1 pede lo-fi calma (a
biblioteca tem faixas de 71 a 80 BPM); C2 pede trilha animada (126 a 144 BPM).
→ *Preciso de:* a lista ordenada de cada uma, com o nível de dificuldade do beatmap
e o `beatSkipFactor` de cada faixa. Isso é metade do `\todo` de parametrização do
Capítulo 3.

**D4 — O critério de tempo do `HitType` passa a ser o mesmo `e_i` da Equação 3.1?**
Hoje o jogo julga por `|tempo de voo − tempo esperado|`; a Equação define
`|tempo do evento − tempo da batida|`. São próximas, mas não iguais.
→ *Recomendo:* unificar. O que o jogador é pontuado e o que o TCC reporta passam a
ser a mesma quantidade, e some uma inconsistência que uma banca pode perguntar.
*Custo:* muda levemente a sensação de julgamento e exige reteste no piloto.

**D5 — A cena de meditação tem guia visual de respiração?**
A narração conta 4-6. Um guia lento (esfera que expande e contrai no mesmo ritmo)
ajuda a aderência e é padrão em apps de meditação em RV — que é exatamente o que C4
representa.
→ *Recomendo:* incluir, bem sutil. *Contra-argumento honesto:* acrescenta um
elemento visual dinâmico que C1 não tem. Se preferir rigor máximo, cena totalmente
estática e só a narração.

**D6 — Onde ficam os controles do pesquisador?**
→ *Recomendo:* painel 2D no PC (via Link durante o desenvolvimento; em standalone,
uma tela de entrada antes de colocar o HMD no participante). Nunca um menu que o
participante possa abrir por acidente.

**D7 — Placar, combo e nota aparecem em C1?**
§3.2.8 diz que na configuração relaxante "a retroalimentação é de apoio, sem
elementos de punição". Placar visível é pressão de desempenho.
→ *Recomendo:* em C1, esconder placar, combo e nota final, mantendo só o retorno
positivo imediato (partícula, som, háptica). Em C2, mostrar tudo. Isso reforça o
contraste de pacotes de projeto que o TCC já afirma existir.
*Atenção:* a telemetria continua gravando tudo — muda apenas o que o participante vê.

**D8 — O que acontece se o participante "morrer" em C2 antes do tempo?**
C2 tem caixas de erro e pode terminar antes dos 15 minutos. Se terminar, a exposição
deixa de ter duração idêntica e R1 quebra.
→ *Recomendo:* reinício imediato e automático da faixa, com o cronômetro correndo. O
participante experimenta a consequência do erro, que é o ponto de C2, sem que a
exposição encurte. Cada reinício vai para o log.

---

## 5. O que não muda

Para evitar retrabalho, estas coisas ficam como estão.

- A cena `BeatMapEditor.unity` e o `BeatmapEditorManager` — ferramenta de autoria,
  fora do caminho do experimento.
- O lobby de 53 músicas, o sistema de recordes e a seleção livre — continuam
  existindo para a versão pública do jogo, apenas **desativados** em modo
  experimento, não removidos.
- A física da raquete, o `GripManager`, o `RacketColorManager` e a personalização.
- Os beatmaps já produzidos.

E, depois do piloto e da aprovação do CEP: **a build congela**. A string de versão
vai em todo log; nenhuma alteração no meio da coleta, por menor que seja.

---

## 6. Ordem de execução

**Antes do piloto** — sem isto, o piloto não mede o que precisa medir:

1. Bloco F (correções D1–D7)
2. Bloco A (modo experimento e configs)
3. Bloco B (marcadores e telemetria)
4. Bloco C (padronização de áudio e duração)

O piloto precisa de marcadores para responder P5 ("consigo localizar os blocos no
arquivo?") e de duração fixa para responder P4 ("15 min é adequado?").

**Depois do piloto, antes da coleta:**

5. Fixar nos assets de config os números que o piloto determinou
6. Bloco D (meditação) e Bloco E (familiarização)
7. Bloco G (segurança)
8. Congelar a build e registrar a versão no Capítulo 3

---

## 7. O que isto resolve no TCC

Fechando este plano, cinco `\todo` deixam de existir:

- **Cap. 3** — parametrização numérica das duas configurações: passa a ser a
  documentação dos assets `C1_Relaxante` e `C2_Animado`
- **Cap. 3** — limiares finais do `HitType`: idem, mais a decisão D4
- **Cap. 3** — produzir a cena de meditação e definir a narração: Bloco D
- **Apêndice C** — produzir a cena e gravar o áudio: Bloco D
- **Cap. 3** — descrever o cenário final: sai da definição dos presets visuais do
  Bloco A e da correção D7

Restam as capturas de tela, que só podem ser tiradas depois que a versão
experimental estiver rodando — mais um motivo para fazer isto antes.
