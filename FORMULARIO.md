# Formulário do estudo — especificação para implementação

Consolida POMS, VAS, SAM, BRUMS, NASA-TLX, Borg RPE, IPQ, SSQ, PSS-10 e GAD-7
em seis blocos, eliminando sobreposições. Pronto para montar em Google Forms.

---

## Princípio que orientou os cortes

> **Escala validada não se encurta.** Retirar itens de dentro de uma subescala
> destrói a validade e impede comparar seus resultados com a literatura. O que
> se pode fazer, e é aceito, é **selecionar subescalas inteiras** e **escolher
> qual instrumento usar quando dois medem a mesma coisa**.

Foi isso que fiz. Nenhum instrumento foi mutilado por dentro.

### Sobreposições encontradas e o que foi feito

| Sobreposição | Decisão | Motivo |
|---|---|---|
| **POMS × BRUMS** | Fica só o **BRUMS** | O BRUMS *é* a versão abreviada do POMS. Usar os dois é medir a mesma coisa duas vezes; o POMS tem 65 itens contra 24 |
| **VAS-ansiedade × BRUMS-Tensão × SAM-ativação** | **VAS + SAM** no bloco repetido; **BRUMS** só 2× por sessão | Os três captam tensão/ativação. A VAS é a mais rápida e a mais sensível a mudança de curto prazo — é ela que vai 4× por sessão |
| **NASA-TLX "Demanda Física" × Borg RPE** | Fica só o **Borg** | Item praticamente idêntico. O Borg é padrão-ouro para esforço e seu professor de Educação Física o conhece. Aplico o TLX com as outras 5 dimensões |
| **SAM-Dominância** | **Removida** | Mede controle percebido, que não é pergunta deste estudo, e é a dimensão mais confusa de responder |
| **BRUMS completo (6 subescalas)** | Só **Tensão, Vigor e Fadiga** | Depressão, Raiva e Confusão não respondem a nenhuma hipótese. Vigor e Fadiga entram porque as condições diferem em esforço físico, e isso importa |
| **PSS-10 × GAD-7** | **Ambos mantidos** | Não são redundantes: estresse percebido no último mês × sintomas de ansiedade nas últimas 2 semanas. Só 1× cada, no ingresso |
| **Seção "Base" do seu formulário atual** | **Substituída** | Os 7 itens misturavam GAD-7, PHQ-9 e POMS numa escala caseira, sem validação. Vira GAD-7 + PSS-10 oficiais |
| **Seções "Estado" do seu formulário atual** | **Substituídas** | Os 8 adjetivos eram um POMS caseiro. Viram VAS + SAM |

---

## Carga por sessão

| Bloco | Quando | Itens | Tempo |
|---|---|---|---|
| F1 Cadastro | 1× no ingresso | ~15 | 3 min |
| F2 Basal | 1× no ingresso | 17 | 5 min |
| **F3 Estado** | **4× por sessão** | **5** | **45 s cada** |
| F4 Humor | 2× por sessão | 12 | 2 min cada |
| F5 Pós-atividade | 1× por sessão | 8 a 38 | 3 a 8 min |
| F6 Opinião | 1× (só jogo) | 3 abertas | 3 min |

Sessão sem RV: ~35 respostas. Sessão com RV: ~70. Aceitável para 60–90 min.

---

# F1 — CADASTRO
*Uma única vez, no ingresso.*

1. Código do participante: `P___` *(preenchido pelo pesquisador)*
2. Idade: ____ anos
3. Gênero: ( ) Feminino ( ) Masculino ( ) Não-binário ( ) Prefiro não informar ( ) Outro: ____
4. Curso e período: ____________________
5. Mão dominante: ( ) Direita ( ) Esquerda ( ) Ambidestro

**Experiência prévia** — escala de 1 a 5:

6. Experiência com jogos de realidade virtual — *1 = Nenhuma · 5 = Muita*
7. Frequência com que joga jogos de ritmo — *1 = Nunca joguei · 5 = Diariamente*
8. Habilidade em tênis de mesa real — *1 = Iniciante · 5 = Avançado*
9. Frequência com que ouve música lo-fi — *1 = Nunca · 5 = Diariamente*
10. Experiência com meditação ou mindfulness — *1 = Nenhuma · 5 = Pratico regularmente*

**Triagem de elegibilidade** — sim/não:

11. Você tem alguma condição cardíaca, neurológica, vestibular (labirinto) ou musculoesquelética diagnosticada?
12. Você já teve crise epiléptica ou tem epilepsia fotossensível?
13. Você tem histórico de enjoo forte em carro, barco, simulador ou realidade virtual?
14. Você tem alguma lesão recente que limite movimentos dos braços ou do tronco?
15. Você faz uso contínuo de medicação que afete os batimentos cardíacos ou a sudorese?

> **[TODO seu]** Definir com o orientador quais respostas afirmativas são
> impeditivas e quais apenas exigem conversa. O Capítulo 3 do TCC já traz os
> critérios; alinhe os dois documentos.

---

# F2 — BASAL
*Uma única vez, no ingresso. Caracteriza a amostra; não é desfecho.*

## F2.1 — PSS-10 (Escala de Estresse Percebido)

**Instrução:** *As perguntas abaixo se referem aos seus sentimentos e pensamentos
durante o último mês. Em cada caso, indique com que frequência você se sentiu ou
pensou de determinada maneira. Responda com rapidez, sem tentar contar as vezes;
escolha a alternativa que parecer uma estimativa razoável.*

**Resposta:** 0 = Nunca · 1 = Quase nunca · 2 = Às vezes · 3 = Com alguma frequência · 4 = Muito frequentemente

Estrutura da escala — 10 itens, sendo os itens **4, 5, 7 e 8 de pontuação
invertida** (0=4, 1=3, 2=2, 3=1, 4=0). Escore total de 0 a 40.

| # | Tema do item |
|---|---|
| 1 | Ficar aborrecido por algo inesperado |
| 2 | Sentir-se incapaz de controlar as coisas importantes da vida |
| 3 | Sentir-se nervoso e estressado |
| 4 | *(invertido)* Sentir confiança na capacidade de lidar com problemas pessoais |
| 5 | *(invertido)* Sentir que as coisas estão acontecendo como você gostaria |
| 6 | Perceber que não conseguiria dar conta de tudo que tinha para fazer |
| 7 | *(invertido)* Conseguir controlar as irritações da vida |
| 8 | *(invertido)* Sentir que estava no controle das coisas |
| 9 | Ficar irritado por coisas fora do seu controle |
| 10 | Sentir que as dificuldades se acumularam a ponto de não poder superá-las |

> **[TODO seu]** Inserir a **redação exata dos 10 itens** a partir da versão
> brasileira validada (Luft et al., 2007, *Revista de Saúde Pública* 41(4)).
> **Não traduza por conta própria** — a validação vale para aquela redação
> específica. A tabela acima é só o mapa da estrutura.

## F2.2 — GAD-7

**Instrução:** *Nas últimas 2 semanas, com que frequência você foi incomodado(a)
pelos problemas abaixo?*

**Resposta:** 0 = Nenhuma vez · 1 = Vários dias · 2 = Mais da metade dos dias · 3 = Quase todos os dias

1. Sentir-se nervoso(a), ansioso(a) ou muito tenso(a)
2. Não ser capaz de impedir ou de controlar as preocupações
3. Preocupar-se muito com diversas coisas
4. Dificuldade para relaxar
5. Ficar tão agitado(a) que se torna difícil permanecer sentado(a)
6. Ficar facilmente aborrecido(a) ou irritado(a)
7. Sentir medo como se algo horrível fosse acontecer

Escore de 0 a 21.

> **[TODO seu]** Conferir a redação acima contra Moreno et al. (2016),
> *Temas em Psicologia* 24(1), que é a validação brasileira citada no TCC.

---

# F3 — ESTADO
*O bloco mais importante. Aplicado **4 vezes por sessão**: (1) pré-estressor,
(2) pós-estressor, (3) pós-atividade, (4) pós-recuperação.*

**Precisa ser rápido.** São 5 itens, cerca de 45 segundos. Se demorar mais que
isso, o estado que você quer medir já mudou.

**Instrução:** *Neste exato momento, como você se sente? Arraste cada marcador
para o ponto que melhor representa o seu estado agora. Não pense muito: a
primeira impressão é o que interessa.*

## F3.1 — Escalas visuais analógicas

Régua contínua de 0 a 100, **sem números visíveis** para o participante, apenas
as âncoras nas pontas. O sistema registra o valor numérico.

| # | Pergunta | Âncora esquerda (0) | Âncora direita (100) |
|---|---|---|---|
| 1 | Quão **ansioso(a)** você se sente agora? | Nada ansioso | Extremamente ansioso |
| 2 | Quão **estressado(a)** você se sente agora? | Nada estressado | Extremamente estressado |
| 3 | Quão **calmo(a)** você se sente agora? | Nada calmo | Extremamente calmo |

> O item 3 é intencionalmente na direção oposta. Serve para detectar quem está
> respondendo no automático: quem marca 100 em ansiedade e 100 em calma não leu.

## F3.2 — SAM (Self-Assessment Manikin)

Escala pictórica de **9 pontos**, com os bonecos do instrumento original.

| # | Dimensão | Extremo esquerdo | Extremo direito |
|---|---|---|---|
| 4 | **Valência** — como você se sente | Muito infeliz / desagradável | Muito feliz / agradável |
| 5 | **Ativação** — quão agitado ou calmo | Muito calmo / sonolento | Muito agitado / excitado |

> **[TODO seu]** Obter as figuras oficiais do SAM (Bradley & Lang, 1994) e
> inseri-las no formulário. **A escala é pictórica**: sem os bonecos, ela vira
> outra coisa. As figuras circulam livremente na literatura.

---

# F4 — HUMOR (BRUMS reduzido)
*2 vezes por sessão: início (antes do estressor) e fim (após a recuperação).*

Três subescalas completas: **Tensão, Vigor e Fadiga** — 12 itens.

Não entram Depressão, Raiva e Confusão, que não respondem a nenhuma hipótese
deste estudo.

**Instrução:** *Indique o quanto você está se sentindo assim agora.*

**Resposta:** 0 = Nada · 1 = Um pouco · 2 = Moderadamente · 3 = Bastante · 4 = Extremamente

> **[TODO seu]** Inserir os **12 itens oficiais** das subescalas Tensão, Vigor e
> Fadiga a partir da versão brasileira validada (Rohlfs et al., 2008,
> *Revista Brasileira de Medicina do Esporte* 14(3)). Cada subescala tem 4
> adjetivos. **Não invente os adjetivos** — a pontuação normativa depende deles.
>
> Ao pedir o instrumento, mencione que é para pesquisa acadêmica sem fins
> lucrativos; o BRUMS costuma ser cedido nessas condições.

---

# F5 — PÓS-ATIVIDADE
*1 vez por sessão, logo após a condição. O conteúdo varia conforme a condição.*

## F5.1 — Esforço físico (todas as condições)

**Borg CR10 — 1 item**

*Como você classifica o esforço físico que fez durante a atividade?*

| Valor | Descrição |
|---|---|
| 0 | Nenhum esforço |
| 0,5 | Extremamente leve |
| 1 | Muito leve |
| 2 | Leve |
| 3 | Moderado |
| 4 | Um pouco intenso |
| 5–6 | Intenso |
| 7–9 | Muito intenso |
| 10 | Extremamente intenso (máximo) |

## F5.2 — Carga de trabalho (todas as condições)

**NASA-TLX sem a dimensão física** — 5 itens, escala de 0 a 100 em passos de 5.

1. **Demanda mental** — Quanta atividade mental foi necessária? *(Muito baixa → Muito alta)*
2. **Demanda temporal** — Quanta pressão de tempo você sentiu? *(Muito baixa → Muito alta)*
3. **Desempenho** — Quão bem-sucedido você acha que foi? *(Perfeito → Fracasso)*
4. **Esforço** — Quanto você teve que se esforçar para atingir seu nível de desempenho? *(Muito baixo → Muito alto)*
5. **Frustração** — Quão irritado, estressado ou incomodado você se sentiu? *(Muito baixo → Muito alto)*

> A dimensão **Demanda Física** foi suprimida por duplicar o Borg CR10.
> Registre essa escolha no TCC: é o uso do *Raw TLX* com seleção de subescalas,
> prática documentada e aceita.

## F5.3 — Fruição (todas as condições)

Escala de 1 a 5 — *1 = Discordo totalmente · 5 = Concordo totalmente*

6. Eu gostei desta atividade.
7. Eu faria esta atividade de novo por vontade própria.

## F5.4 — Somente condições em realidade virtual

**IPQ — Presença** (14 itens)

> **[TODO seu]** Inserir os 14 itens do *Igroup Presence Questionnaire*, que são
> de acesso livre no site do grupo (igroup.org), com a tradução para o português
> devidamente registrada. Escala de 7 pontos.

**SSQ — Cybersickness** (16 sintomas)

> **[TODO seu]** Inserir os 16 sintomas do *Simulator Sickness Questionnaire*, de
> preferência a partir da adaptação validada para o português (Gonçalves et al.,
> 2024, *IEEE Access* 12). Resposta em 4 pontos: Nenhum, Leve, Moderado, Severo.
> **Os 16 itens são obrigatórios** — a pontuação das três subescalas (náusea,
> oculomotor, desorientação) só fecha com todos.

## F5.5 — Somente condições de jogo

Escala de 1 a 5 — *1 = Discordo totalmente · 5 = Concordo totalmente*

8. Os movimentos que eu fazia acompanhavam o ritmo da música.
9. Eu me senti confortável durante toda a sessão.
10. Eu perdi a noção do tempo enquanto jogava.
11. A dificuldade estava adequada para mim.
12. O cenário reforçou a sensação transmitida pela música.
13. Os retornos visuais, sonoros e de vibração foram agradáveis.

> Itens 9 a 13 vêm do seu formulário atual, preservados. O item que perguntava
> sobre "esforço mental" foi retirado daqui: já está no NASA-TLX (F5.2), com
> escala validada.

## F5.6 — Apreciação da trilha (condições com música)

14. Quanto você gostou da música desta sessão? *(1 a 5)*
15. Você já conhecia essa música ou esse estilo? ( ) Sim ( ) Não

> Este bloco é uma **verificação de manipulação**, não um desfecho. Ele confirma
> que a diferença entre "jogar com a música" e "só ouvir a música" não se deve a
> uma pessoa ter gostado mais da trilha em uma sessão do que na outra.

---

# F6 — OPINIÃO
*Uma vez, ao final da última sessão de jogo. Respostas abertas.*

1. Qual foi a parte mais satisfatória da sua experiência com o jogo?
2. Houve algum momento, mecânica ou elemento visual que causou frustração? Se sim, qual?
3. Se você pudesse mudar, adicionar ou remover algo no jogo para torná-lo o passatempo calmo ideal para você, o que seria?
4. Comparando as atividades que você fez nos diferentes dias, alguma te deixou visivelmente mais relaxado(a)? Qual e por quê?

> As três primeiras são as suas, mantidas. A quarta foi acrescentada: como o
> participante passa por todas as condições, ele é a única pessoa capaz de
> compará-las por dentro. É dado qualitativo que nenhuma escala captura.

---

## Notas de implementação

**Não use uma única resposta longa do Google Forms.** O bloco F3 é aplicado 4
vezes por sessão e 16 vezes ao todo. Crie **um formulário separado por
momento**, ou um formulário único com um campo de identificação
`P__ / sessão __ / momento __`. Do contrário, você não conseguirá parear as
medidas na análise.

**Ordem dos itens dentro de cada bloco:** mantenha fixa entre participantes e
entre sessões. Ordem aleatória parece mais rigorosa, mas atrapalha a comparação
de medidas repetidas.

**Escalas visuais analógicas em papel** exigem régua para medir cada resposta, o
que dá muito trabalho e introduz erro. Se puder, aplique em tela.

**Antes de rodar com participantes reais**, preencha o formulário inteiro você
mesmo, do começo ao fim, cronometrando. É a forma mais rápida de descobrir que
algo está longo demais ou confuso.
