# Operação do modo experimento

Manual do que foi construído no aplicativo, como conduzir uma sessão e o que
ainda falta você fazer.

---

## 1. O que falta você fazer

Três itens. Nenhum deles é código.

### 1.1 Completar 13 beatmaps

Todos os mapas rítmicos do projeto terminam antes da música. Isso não importava
quando a sessão acabava junto com a faixa, mas agora a exposição dura 20 minutos
encadeando faixas inteiras: onde o mapa acaba, as bolas param e o participante
fica ouvindo música sem jogar.

Só estas faixas precisam de mapa completo. As demais 40 podem ficar como estão.

**Condição C1 (relaxante) — também usada em C3**

| # | Faixa | BPM | Skip | Música | Mapa | Falta |
|---|---|---|---|---|---|---|
| 1 | CELESTIAL GOLD | 80 | 4 | 203,7 s | 174,1 s | **29,7 s** |
| 2 | REMEMBER | 77 | 4 | 154,8 s | 146,9 s | **7,9 s** |
| 3 | Miss You | 75 | 4 | 179,3 s | 153,3 s | **26,0 s** |
| 4 | Day In Paris | 75 | 4 | 240,2 s | 205,7 s | **34,5 s** |
| 5 | Distant | 75 | 4 | 147,3 s | 144,5 s | **2,8 s** |
| 6 | Faithful Mission | 74 | 4 | 152,8 s | 145,2 s | **7,6 s** |
| 7 | Eastridge Turnstile | 74 | 4 | 149,7 s | 141,0 s | **8,7 s** |
| 8 | Windy *(reserva)* | 71 | 4 | 155,6 s | 133,2 s | 22,5 s |

As sete primeiras somam 1227,8 s, então o corte dos 20 min cai dentro de
*Eastridge Turnstile*. **Windy é reserva**: só entra se você encurtar ou trocar
alguma das anteriores. Deixe por último.

Total de mapa a acrescentar nas obrigatórias: **117 s**.

**Condição C2 (animada)**

| # | Faixa | BPM | Skip | Música | Mapa | Falta |
|---|---|---|---|---|---|---|
| 1 | Leaving | 110 | 4 | 268,5 s | 228,1 s | **40,4 s** |
| 2 | Warm Horizon | 110 | 4 | 175,5 s | 169,7 s | **5,8 s** |
| 3 | STRANDED | 114 | 4 | 216,2 s | 184,5 s | **31,6 s** |
| 4 | Helen 2 | 120 | **2** | 224,1 s | 210,4 s | **13,7 s** |
| 5 | METEORITES | 126 | **2** | 183,5 s | 151,7 s | **31,7 s** |
| 6 | Herbal Tea | 130 | **2** | 184,8 s | 181,1 s | **3,7 s** |
| 7 | DAYDREAM *(reserva)* | 133 | **2** | 174,7 s | 156,2 s | 18,5 s |

As seis primeiras somam 1252,5 s; o corte cai dentro de *Herbal Tea*.
Total a acrescentar: **127 s**.

> ### ⚠ Preserve o espaçamento que cada mapa já usa
>
> A subdivisão **não é uniforme** no projeto. Onze das quinze faixas acima têm
> uma nota por batida; **Helen 2, METEORITES, Herbal Tea e DAYDREAM têm uma nota
> a cada duas batidas**, e é por isso que elas usam `skip 2` enquanto as outras
> usam `skip 4`.
>
> Se você completar uma dessas quatro escrevendo uma nota por batida, a
> densidade daquela faixa **dobra** e a configuração animada vira algo
> impraticável, com bolas se sobrepondo no ar. Continue no mesmo espaçamento em
> que o mapa já está.

### 1.2 Escolher o aplicativo de meditação (C4)

Precisa ficar registrado, antes da coleta e sem mudar depois:

- o aplicativo e o **número exato da versão**
- a **sessão** usada dentro dele, sempre a mesma
- a **sequência de sessões** a usar se a primeira acabar antes dos 20 min
- o **volume do sistema** no dispositivo
- as configurações internas que afetem conteúdo: voz, idioma, ambiente visual
- **modo avião durante toda a coleta**, para o conteúdo não ser atualizado no meio do estudo

E confirmar que a **licença de uso permite emprego em pesquisa acadêmica** —
isso vai anexado ao processo do CEP.

### 1.3 Calibrar o volume no piloto

Os níveis do mixer estão em 0 dB (mestre e música) e −3 dB (efeitos). Esses
valores são um ponto de partida, não uma medição: o nível que chega ao ouvido
depende também do volume de hardware do dispositivo.

No piloto, fixe e anote o volume de hardware do Quest e do computador usado em
C3. Se precisar ajustar o nível, mude em
`Assets/My/Scripts/Experiment/ExperimentConfig.cs`, não no dispositivo — assim o
valor fica registrado nos metadados de cada sessão.

---

## 2. Como conduzir uma sessão

### 2.1 Armar

No editor: menu **Cozy Pong → Sessão experimental**. Preencha o código do
participante, o número da sessão e a condição, e clique em **Armar sessão**.

A janela mostra o resumo da condição e a playlist com os tempos acumulados, para
você conferir antes de começar.

Enquanto não houver sessão armada, o jogo roda exatamente como a versão pública:
lobby, escolha de música, recordes, controles de volume. Nada disso muda.

Para voltar ao jogo normal: **Desarmar**.

### 2.2 No Quest

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

### 2.3 Durante

O painel do pesquisador aparece na tela do operador — nunca dentro do HMD.
Mostra tempo decorrido e restante, faixa atual, se o registro está aberto,
quantas notas já foram gravadas e qualquer erro de gravação.

- **P** — pausa e retoma. O tempo pausado não conta para os 20 minutos.
- **F10** — interrompe a exposição imediatamente, gravando o motivo.

Em C3 e C4 o painel escurece o resto da tela, para o monitor não virar um
estímulo visual não previsto.

### 2.4 Depois

Os arquivos ficam em `Application.persistentDataPath/ExperimentData/`.

```bash
adb pull /sdcard/Android/data/<pacote>/files/ExperimentData ./dados
```

**Confirme a cópia antes de apagar qualquer coisa do dispositivo.**

---

## 3. O que sai de cada sessão

Três arquivos, nomeados `P07_S03_C1_2026-09-14T15-22-10.*`.

**`.meta.csv`** — uma linha com tudo que o Capítulo 4 precisa reportar: versão
do app e do Unity, dispositivo, semente, volumes travados, limiares de
julgamento, playlist efetivamente usada, duração alvo e real, motivo do
encerramento, reinícios de faixa, FPS mediano e percentil 5, quadros perdidos.

**`.markers.csv`** — `unix_ms, dsp_s, session_s, marker, detail`.

Marcadores: `SESSION_START`, `BUILD`, `SEED`, `AUDIO_LOCKED`, `VISUAL_PRESET`,
`EXPOSURE_START`, `TRACK_START`, `TRACK_END`, `TRACK_RESTART`,
`TRACK_FORCED_PLAY`, `TRACK_MISSING`, `PLAYLIST_LOOP`, `PAUSE_START`,
`PAUSE_END`, `INTERRUPTION`, `EXPOSURE_END`, `SESSION_END`, `APP_QUIT`.

Os três relógios existem por motivos diferentes: `unix_ms` alinha com a Polar e
o sensor de EDA, `dsp_s` é o relógio de áudio e `session_s` é o que você lê.

**`.events.csv`** — uma linha por bola, só em C1 e C2. A coluna `e_i_ms` é
literalmente a Equação 3.1 do TCC: a menor distância entre a rebatida e alguma
batida da música. A média dessa coluna é o **ē** que o Capítulo 4 reporta.

Bolas que expiram sem rebatida também entram, com `classification = Miss` e as
colunas de tempo vazias — vazio significa "não se aplica", nunca zero.

---

## 4. O que o modo experimento muda no jogo

| | Jogo normal | Sessão experimental |
|---|---|---|
| Duração | acaba com a música | cronômetro de 20 min |
| Escolha de música | livre, 53 faixas | playlist fixa da condição |
| Aleatoriedade | não semeada | semeada pela condição |
| Volume | dois sistemas, persistente | travado pelo mixer |
| Recordes | salvos e exibidos | não lidos nem gravados |
| Placar em C1 | visível | oculto |
| Ao perder | tela de derrota | faixa reinicia, cronômetro segue |
| Luz reativa | ciclo de matiz | cor fixa |
| Telemetria | nenhuma | três CSVs |

---

## 5. Antes de valer

- [ ] Completar os 13 beatmaps da Seção 1.1
- [ ] Definir e travar o aplicativo de meditação
- [ ] Rodar o piloto e conferir P1 a P5 do `PILOTO_ROTEIRO.md`
- [ ] Fixar o volume de hardware e anotá-lo
- [ ] Confirmar 20 min como duração adequada, ou ajustar `exposureSeconds`
- [ ] Definir `Application.version` no ProjectSettings (hoje `0.1`) — ele vai em todo log
- [ ] Congelar a build e não mexer mais durante a coleta
