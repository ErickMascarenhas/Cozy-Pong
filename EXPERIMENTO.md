# Operação do modo experimento

Manual do que foi construído no aplicativo, como conduzir uma sessão e o que
ainda falta você fazer.

---

## 1. As playlists

Cinco faixas por condição, ordem fixa, idêntica para todos os participantes.
Os dez mapas rítmicos já estão prontos (Seção 2).

**C1 (relaxante) — a mesma playlist é usada em C3**

Andamento decrescente, princípio *iso*: começa perto do estado provável depois
do estressor e desacelera.

| # | Faixa | BPM | Áudio | Jogável | Bolas/min | Acum. |
|---|---|---|---|---|---|---|
| 1 | Colorful Flowers | 92 | 243,9 s | 10,8–220,8 | 23,0 | 243,9 |
| 2 | Slowly | 89 | 249,9 s | 2,0–230,3 | 22,3 | 493,8 |
| 3 | Your Little Wings | 89 | 247,2 s | 2,0–235,5 | 22,2 | 741,0 |
| 4 | Way Home | 85 | 278,2 s | 20,5–252,7 | 21,3 | 1019,2 |
| 5 | Day In Paris | 75 | 240,1 s | 2,0–232,8 | 18,8 | 1259,3 |

Total 1259,3 s. O corte dos 20 min cai dentro de *Day In Paris*, com **59 s de
folga**.

**C2 (animada)**

Andamento crescente.

| # | Faixa | BPM | Áudio | Jogável | Bolas/min | Acum. |
|---|---|---|---|---|---|---|
| 1 | Leaving | 110 | 268,4 s | 2,8–230,2 | 27,5 | 268,4 |
| 2 | STRANDED | 114 | 216,1 s | 2,0–206,1 | 28,5 | 484,5 |
| 3 | Helen 2 | 120 | 224,0 s | 11,6–209,5 | 30,0 | 708,5 |
| 4 | Herbal Tea | 130 | 184,8 s | 2,0–182,3 | 32,5 | 893,3 |
| 5 | DAYDREAM | 133 | 174,6 s | 2,0–170,6 | 33,3 | 1067,9 |

Total 1067,9 s: **faltam 132 s** para os 20 minutos.

A biblioteca não tem cinco faixas rápidas longas o bastante. A solução foi
repetir a **última** faixa, e não a primeira: assim o fecho da condição fica no
ponto mais intenso do arco, em vez de cair de 133 para 110 BPM justamente no
final da condição feita para energizar. A repetição é idêntica para todos e vai
para o log como `TRACK_REPEAT_LAST`.

---

## 2. Os beatmaps

Os dez mapas foram gerados por análise do áudio, não à mão. Estão gravados em
`Assets/Songs/Txts/Use this/`, no mesmo formato do editor, e o cabeçalho de cada
arquivo registra o BPM ajustado, a fase e a região com pulso.

**Como foram feitos.** Para cada faixa, a envoltória de ataques do áudio é
calculada e busca-se, em torno do BPM nominal, o período que maximiza a
magnitude do coeficiente de Fourier correspondente; a fase sai do argumento
desse mesmo coeficiente. Depois, batida a batida, verifica-se se há ataque
detectável, para delimitar onde o pulso realmente existe e não lançar bolas
sobre introduções e desfechos sem percussão.

**Como sei que está certo.** Comparei a grade estimada com os mapas manuais que
já existiam. Em 17 das 24 faixas examinadas a dispersão ficou abaixo de 10 ms, e
a mediana dos desvios foi de **+2,1 ms** — ou seja, o método não tem viés. Os
mapas manuais, por sua vez, tinham desvios próprios de −15 a +60 ms conforme a
faixa, e alguns bem piores: o mapa antigo de *CELESTIAL GOLD* estava 282 ms fora.

Alguns números do resultado: cobertura média de 91% da duração de cada faixa, e
de 85% a 99,7% das notas caem sobre uma batida com ataque detectável.

Uma consequência útil: agora **todos os dez mapas têm uma nota por batida**, com
fator de subdivisão 4 em todas as faixas. Antes a subdivisão variava (*Helen 2*
tinha uma nota a cada duas batidas, *Path Of The Fireflies* uma a cada 1,5), o
que obrigava um fator diferente por faixa.

*Path Of The Fireflies* chegou a entrar na seleção de C2 e foi descartada: só
74,7% de suas batidas têm ataque detectável, contra 96,0% de *DAYDREAM*. Em um
jogo de ritmo, uma bola em cada quatro caindo onde não há nada audível é um
defeito, não uma variação.

---

## 3. O que falta você fazer

### 3.1 Escolher o aplicativo de meditação (C4)

Precisa ficar registrado, antes da coleta e sem mudar depois:

- o aplicativo e o **número exato da versão**
- a **sessão** usada dentro dele, sempre a mesma
- a **sequência de sessões** a usar se a primeira acabar antes dos 20 min
- o **volume do sistema** no dispositivo
- as configurações internas que afetem conteúdo: voz, idioma, ambiente visual
- **modo avião durante toda a coleta**, para o conteúdo não ser atualizado no meio do estudo

E confirmar que a **licença de uso permite emprego em pesquisa acadêmica** —
isso vai anexado ao processo do CEP.

### 3.2 Calibrar o volume no piloto

Os níveis do mixer estão em 0 dB (mestre e música) e −3 dB (efeitos). Esses
valores são um ponto de partida, não uma medição: o nível que chega ao ouvido
depende também do volume de hardware do dispositivo.

No piloto, fixe e anote o volume de hardware do Quest e do computador usado em
C3. Se precisar ajustar o nível, mude em
`Assets/My/Scripts/Experiment/ExperimentConfig.cs`, não no dispositivo — assim o
valor fica registrado nos metadados de cada sessão.

---

## 4. Os dois modos de execução

Há duas formas de rodar uma condição, e elas servem a propósitos diferentes.

| | Sessão armada | Playlist pelo lobby |
|---|---|---|
| Como começa | arquivo de sessão (Seção 5) | botão no painel do lobby |
| Duração | cronômetro de 20 min | acaba quando a playlist acaba |
| Entre faixas | encadeia sozinho, sem parar | tela de conclusão, o jogador avança |
| Registro em disco | três CSVs | nenhum |
| Placar e recordes | conforme a condição, sem gravar | conforme a condição, sem gravar |
| Para que serve | **coletar dados** | jogar, testar, conferir |

**A coleta usa a sessão armada.** A exposição precisa ser contínua e ter duração
idêntica nas quatro condições; uma tela de conclusão entre as faixas quebra as
duas coisas. Use a playlist do lobby para jogar e verificar as condições, não
para coletar.

### 4.1 A lista de condições no lobby

O painel que listava as 53 músicas agora lista as condições. Apertar uma leva
direto à primeira faixa da playlist — sempre a primeira, independentemente de
onde você parou antes.

Ao terminar cada faixa aparece a tela de conclusão de sempre, com as
estatísticas da partida. O botão que antes repetia a música agora leva à
**próxima faixa da playlist**, e mostra qual é. Na última faixa ele vira
*Finish playlist*. **Return to Lobby** continua ali e encerra a playlist a
qualquer momento.

C3 não tem tela de conclusão entre as faixas: sem jogo não há resultado a
mostrar, então ela encadeia sozinha. C4 aparece na lista, mas desabilitada — a
meditação é conduzida por aplicativo externo.

Nada disso exigiu alteração na cena. Os objetos de estado, os carregadores de
transição e os botões existentes são localizados em tempo de execução, e as
entradas da lista são clonadas de uma entrada de música, o que preserva o
leiaute original.

---

## 5. Como conduzir uma sessão armada

### 5.1 Armar

No editor: menu **Cozy Pong → Sessão experimental**. Preencha o código do
participante, o número da sessão e a condição, e clique em **Armar sessão**.

A janela mostra o resumo da condição e a playlist com os tempos acumulados, para
você conferir antes de começar.

Enquanto não houver sessão armada, o jogo abre no lobby com a lista de condições
(Seção 4.1), e os controles de preferências e volume continuam disponíveis.

Para voltar ao lobby: **Desarmar**.

### 5.2 No Quest

O arquivo de sessão também pode ser enviado por adb:

```bash
adb push experiment_session.json /sdcard/Android/data/<pacote>/files/
```

Conteúdo:

```json
{
  "participantId": "P07",
  "sessionNumber": 3,
  "condition": "C1",
  "exposureSeconds": 1200,
  "seed": 20260901,
  "notes": ""
}
```

Ou por linha de comando, em builds de computador:

```bash
CozyPong.exe -participant P07 -session 3 -condition C1
```

### 5.3 Durante

O painel do pesquisador aparece na tela do operador — nunca dentro do HMD.
Mostra tempo decorrido e restante, faixa atual, se o registro está aberto,
quantas notas já foram gravadas e qualquer erro de gravação.

- **P** — pausa e retoma. O tempo pausado não conta para os 20 minutos.
- **F10** — interrompe a exposição imediatamente, gravando o motivo.

Em C3 e C4 o painel escurece o resto da tela, para o monitor não virar um
estímulo visual não previsto.

### 5.4 Depois

Os arquivos ficam em `Application.persistentDataPath/ExperimentData/`.

```bash
adb pull /sdcard/Android/data/<pacote>/files/ExperimentData ./dados
```

**Confirme a cópia antes de apagar qualquer coisa do dispositivo.**

---

## 6. O que sai de cada sessão

Três arquivos, nomeados `P07_S03_C1_2026-09-14T15-22-10.*`.

**`.meta.csv`** — uma linha com tudo que o Capítulo 4 precisa reportar: versão
do app e do Unity, dispositivo, semente, volumes travados, limiares de
julgamento, playlist efetivamente usada, duração alvo e real, motivo do
encerramento, reinícios de faixa, FPS mediano e percentil 5, quadros perdidos.

**`.markers.csv`** — `unix_ms, dsp_s, session_s, marker, detail`.

Marcadores: `SESSION_START`, `BUILD`, `SEED`, `AUDIO_LOCKED`, `VISUAL_PRESET`,
`EXPOSURE_START`, `TRACK_START`, `TRACK_END`, `TRACK_RESTART`,
`TRACK_FORCED_PLAY`, `TRACK_MISSING`, `TRACK_REPEAT_LAST`, `PLAYLIST_LOOP`,
`PAUSE_START`, `PAUSE_END`, `INTERRUPTION`, `EXPOSURE_END`, `SESSION_END`,
`APP_QUIT`.

Os três relógios existem por motivos diferentes: `unix_ms` alinha com a Polar e
o sensor de EDA, `dsp_s` é o relógio de áudio e `session_s` é o que você lê.

**`.events.csv`** — uma linha por bola, só em C1 e C2. A coluna `e_i_ms` é
literalmente a Equação 3.1 do TCC: a menor distância entre a rebatida e alguma
batida da música. A média dessa coluna é o **ē** que o Capítulo 4 reporta.

Bolas que expiram sem rebatida também entram, com `classification = Miss` e as
colunas de tempo vazias — vazio significa "não se aplica", nunca zero.

---

## 7. O que o modo experimento muda no jogo

| | Jogo normal | Sessão experimental |
|---|---|---|
| Duração | acaba com a música | cronômetro de 20 min |
| Escolha no lobby | livre, 53 faixas | lobby suspenso |
| Aleatoriedade | não semeada | semeada pela condição |
| Volume | dois sistemas, persistente | travado pelo mixer |
| Recordes | salvos e exibidos | não lidos nem gravados |
| Placar em C1 | visível | oculto |
| Ao perder | tela de derrota | faixa reinicia, cronômetro segue |
| Luz reativa | ciclo de matiz | cor fixa |
| Telemetria | nenhuma | três CSVs |

---

## 8. Antes de valer

- [ ] Definir e travar o aplicativo de meditação
- [ ] Rodar o piloto e conferir P1 a P5 do `PILOTO_ROTEIRO.md`
- [ ] Fixar o volume de hardware e anotá-lo
- [ ] Confirmar 20 min como duração adequada, ou ajustar `exposureSeconds`
- [ ] Definir `Application.version` no ProjectSettings (hoje `0.1`) — ele vai em todo log
- [ ] Congelar a build e não mexer mais durante a coleta
