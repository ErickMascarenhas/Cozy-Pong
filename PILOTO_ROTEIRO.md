# Estudo-piloto — captação de VFC e calibração do tempo de exposição

**Objetivo:** verificar, antes de submeter ao CEP e antes de recrutar, se a
cadeia de medição funciona e se os tempos escolhidos produzem sinal utilizável.

**O piloto não testa hipóteses.** Ele testa o *aparato*. Nenhum dado daqui entra
na análise do estudo; serve para você descobrir os problemas enquanto eles ainda
são baratos de corrigir.

**Participantes:** 3 a 5 pessoas, podendo ser você mesmo, colegas ou voluntários
conhecidos. Como não há coleta de dados de pesquisa nem produção de
conhecimento generalizável, é teste técnico de equipamento. Ainda assim,
**confirme esse entendimento com seu orientador** antes de começar — a fronteira
entre "teste técnico" e "pesquisa com seres humanos" é julgada pelo CEP, não por
você.

---

## 1. O que o piloto precisa responder

Cinco perguntas objetivas. Anote a resposta de cada uma ao final.

| # | Pergunta | Como saber que passou |
|---|---|---|
| P1 | A cinta entrega **intervalos RR**, e não só a média? | O arquivo exportado tem uma coluna com valores em torno de 600–1100 ms, um por batimento |
| P2 | Quanto sinal se perde por artefato? | Menos de **5%** dos intervalos precisam de correção em repouso |
| P3 | A janela de repouso de **5 min** basta para uma medida estável? | Duas janelas de 5 min no mesmo repouso dão RMSSD parecido |
| P4 | **15 min** de exposição é tempo adequado? | A pessoa não relata tédio nem cansaço; e o sinal se mantém utilizável |
| P5 | A marcação de tempo funciona? | Você consegue localizar, no arquivo, exatamente onde começou e terminou cada bloco |

---

## 2. Material

- Cinta **Polar H10**, com bateria nova e o eletrodo umedecido
- Celular ou notebook com app que **exporte RR** (ver seção 3)
- Cronômetro visível
- Cadeira com encosto, sem rodízios
- Termômetro ambiente
- Planilha de anotação (modelo na seção 7)

**Ambiente:** sala fechada, sem circulação de pessoas, temperatura entre 20 °C e
24 °C, iluminação constante, sem música e sem conversa. Desligue notificações
do celular usado na captura.

---

## 3. Antes de tudo: garanta o RR

Este é o ponto onde o piloto mais frequentemente falha, e é a **P1**.

Muitos aplicativos de frequência cardíaca mostram o gráfico bonito mas exportam
apenas a FC média por segundo. Isso é inútil para VFC.

**Faça este teste antes de qualquer coisa:** capture 2 minutos, exporte, e abra
o arquivo. Você precisa ver uma sequência de números em milissegundos, um por
batimento, variando algo como 780, 812, 795, 840... Se você vir apenas
"72 bpm, 73 bpm, 71 bpm", o app não serve.

> **[TODO seu]** Defina e registre qual app/software será usado para captura e
> exportação. Opções conhecidas incluem aplicativos de logger da própria Polar e
> softwares de análise de VFC. Confirme com o professor de Educação Física o que
> o laboratório já usa — se houver um fluxo estabelecido, adote-o em vez de criar
> outro.

---

## 4. Protocolo do piloto (≈ 50 min por pessoa)

Siga na ordem. Anote o horário de início de cada bloco.

### Bloco 0 — Preparo (10 min)

1. Umedeça a área dos eletrodos da cinta. Cinta seca é a causa nº 1 de sinal ruim.
2. Posicione a cinta logo abaixo do peitoral, ajustada mas não apertada.
3. Conecte e **observe o sinal por 2 minutos** antes de começar. Se houver falhas, reposicione.
4. Registre temperatura da sala e horário.
5. Peça que a pessoa vá ao banheiro antes de começar. Interromper no meio invalida a janela.

### Bloco 1 — Repouso A (10 min) → responde P2 e P3

Pessoa sentada, pés no chão, mãos apoiadas, olhos abertos, sem falar, sem celular.

Você fica em silêncio. **Não converse durante a janela** — fala altera a respiração
e a respiração altera a VFC.

Marque: início, e o instante exato de 5 min.

### Bloco 2 — Atividade leve (15 min) → responde P4

Use o próprio Cozy Pong na configuração relaxante, se já estiver jogável. Se não
estiver, qualquer atividade leve em pé serve para o propósito deste piloto.

Ao final, pergunte e anote:
- "De 0 a 10, quanto esforço físico você sentiu?"
- "Os 15 minutos passaram rápido, foram adequados ou ficaram longos?"
- "Em algum momento você se sentiu entediado ou cansado?"

### Bloco 3 — Repouso B / recuperação (10 min) → responde P3 e P4

Idêntico ao Bloco 1. Mesma cadeira, mesma postura.

Esta é a janela mais importante do estudo real — é dela que sai o desfecho
fisiológico primário.

### Bloco 4 — Encerramento (5 min)

Exporte os dados **na hora**, com a pessoa ainda presente. Se o arquivo não
existir ou estiver vazio, você ainda pode repetir.

Nomeie o arquivo assim: `piloto_P01_2026-07-31.csv`

---

## 5. Análise mínima

Não precisa de estatística. Precisa destas seis checagens.

1. **Abra o arquivo bruto.** Confirme que há intervalos RR em milissegundos.
2. **Conte os artefatos.** Quantos intervalos fogem da faixa 300–2000 ms ou diferem mais de 20% do anterior? Divida pelo total. Anote o percentual, por bloco.
3. **Calcule RMSSD** nos 5 min finais do Repouso A e nos 5 min finais do Repouso B.
4. **Divida o Repouso A em duas metades de 5 min** e calcule RMSSD de cada. Se os dois valores forem muito diferentes, 5 min pode não estar bastando (P3).
5. **Compare Repouso A e Repouso B.** Espera-se FC mais alta e RMSSD mais baixo logo após a atividade, voltando ao basal ao longo dos 10 min. Se não houver *nenhuma* diferença, desconfie da captura.
6. **Localize os marcos de tempo** no arquivo. Você consegue apontar onde começou o Bloco 2? Se não, seu esquema de marcação precisa mudar (P5).

> **[TODO seu]** Defina a ferramenta de análise. Kubios HRV (versão gratuita) faz
> tudo isso com interface gráfica. Alternativa: Python com a biblioteca NeuroKit2.
> Combine com o professor de Educação Física qual ele conhece.

---

## 6. Critérios de decisão

Ao final, para cada pergunta, decida e registre:

**P1 — RR disponível?**
Se não → troque de app/software antes de qualquer outra coisa. Nada mais importa.

**P2 — artefatos abaixo de 5%?**
Se não → revise umedecimento, posicionamento e ajuste da cinta. Repita. Se
persistir acima de 10% em repouso, há problema de equipamento ou de ambiente.

**P3 — 5 min bastam?**
Se as duas metades derem valores muito distintos → considere estender a janela de
repouso para 7 ou 10 min. Registre a decisão e leve ao orientador.

**P4 — 15 min de exposição?**
Se as pessoas relatarem tédio → reduza para 12. Se relatarem cansaço físico →
reduza para 10 e reavalie a intensidade. Se acharem adequado → mantenha.
**Qualquer mudança aqui muda o Capítulo 3 do TCC e o TCLE.** Decida antes de submeter.

**P5 — marcação temporal?**
Se você não consegue localizar os blocos no arquivo → adote marcação redundante:
anote o horário de relógio no papel *e* faça um gesto detectável (por exemplo,
uma pausa deliberada) no início de cada bloco.

---

## 7. Planilha de anotação

Copie uma folha destas por participante.

```
PILOTO — Participante: P___    Data: ___/___/______
Sala: __________  Temperatura: ____°C   Início: ____:____

App/software de captura: ______________________________
Bateria da cinta trocada? ( ) sim  ( ) não
Eletrodo umedecido?        ( ) sim  ( ) não

BLOCO 0 preparo      início ____:____  qualidade do sinal: ( )boa ( )instável
BLOCO 1 repouso A    início ____:____  fim ____:____
BLOCO 2 atividade    início ____:____  fim ____:____
BLOCO 3 repouso B    início ____:____  fim ____:____

Interrupções / imprevistos:
_______________________________________________________
_______________________________________________________

RELATO DO PARTICIPANTE
Esforço (0-10): ____
Os 15 min foram:  ( ) curtos  ( ) adequados  ( ) longos
Tédio ou cansaço? ( ) não  ( ) sim — quando? ______________
Desconforto com a cinta? ( ) não  ( ) sim — ______________

ANÁLISE (preencher depois)
Arquivo exportado tem RR?        ( ) sim ( ) não
% artefatos — repouso A: ____%   repouso B: ____%
RMSSD repouso A (5 min finais): ______ ms
RMSSD repouso A (primeira metade): ______ ms
RMSSD repouso B (5 min finais): ______ ms
FC média A: ____ bpm   FC média B: ____ bpm
Marcos de tempo localizáveis?    ( ) sim ( ) não
```

---

## 8. Fechamento do piloto

Depois de 3 a 5 pessoas, escreva **meia página** respondendo P1 a P5 e liste as
decisões tomadas. Esse texto tem dois destinos:

1. **A reunião com os professores** — é a evidência de que o aparato funciona, e
   vale mais que qualquer argumento teórico.
2. **O Capítulo 3 do TCC** — a Seção de Equipamentos passa a poder afirmar que os
   parâmetros foram verificados empiricamente, em vez de apenas escolhidos.

> **[TODO seu]** Após o piloto, atualize no TCC: a duração da exposição (se mudar),
> a duração das janelas de repouso (se mudar), e o modelo/app efetivamente usado
> na captura. Todos os três aparecem no Capítulo 3.
