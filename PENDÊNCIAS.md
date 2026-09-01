# Pendências

Atualizado depois da implementação do modo experimento
(branch `feat/experiment-mode`).

---

## 1. Para submeter ao CEP

**a) As 9 informações do TCLE.** Título aprovado, seu telefone e e-mail, nome
completo e contatos do orientador, laboratório onde será feito, e — a que
costuma demorar mais — endereço, telefone, e-mail e horário de atendimento do
CEP/UFAL. Peça na secretaria antes, não deixe para a véspera.

**b) A decisão sobre material biológico.** Coleta de saliva/cortisol ficou fora
do TCLE, deliberadamente: incluir aciona regras de biorrepositório e alonga a
análise. É o único lugar onde "errar por exagero" custa caro. Converse com o
orientador — se um dia quiser medir cortisol e isso não estiver no termo, é
submissão nova.

**c) Alinhar os critérios de elegibilidade** nos três lugares onde aparecem: o
TCLE, o bloco F1 do formulário e o Capítulo 3. Hoje se referem às mesmas coisas
mas não estão redigidos de forma idêntica, e o CEP compara.

**d) A licença do aplicativo de meditação.** Confirmar que o app escolhido para
C4 permite uso em pesquisa acadêmica e anexar essa confirmação ao processo.

---

## 2. O jogo

Implementado. O que resta está detalhado em [`EXPERIMENTO.md`](EXPERIMENTO.md):

- **Completar 13 beatmaps** (~117 s em C1, ~127 s em C2). Preservando o
  espaçamento que cada mapa já usa — quatro faixas de C2 têm uma nota a cada
  duas batidas, e reescrevê-las a uma nota por batida dobraria a densidade.
- **Escolher e travar o aplicativo de meditação** de C4: versão, sessão, volume,
  modo avião durante toda a coleta.
- **Calibrar o volume de hardware** no piloto e anotá-lo.
- **Definir `Application.version`** no ProjectSettings (hoje `0.1`).

---

## 3. O piloto

Antes dele, escolher duas ferramentas:

- o app/software que exporta os intervalos RR — confirme com o professor de
  Educação Física o que o laboratório já usa antes de escolher outro
- a ferramenta de análise — Kubios gratuito ou NeuroKit2

Faça o teste de 2 minutos da seção 3 do `PILOTO_ROTEIRO.md` antes de qualquer
outra coisa. Se o arquivo exportado tiver "72 bpm" em vez de "812 ms", nada mais
importa.

Depois do piloto, três números podem mudar no TCC: duração da exposição, duração
das janelas de repouso, e os parâmetros do estressor no Apêndice.

---

## 4. O formulário

Faltam as transcrições oficiais — PSS-10 (Luft et al. 2007), BRUMS
Tensão/Vigor/Fadiga (Rohlfs et al. 2008), IPQ (igroup.org), SSQ (os 16 itens,
todos obrigatórios para pontuar) e as figuras oficiais do SAM. Transcrever
escala validada de memória invalida a comparação com a literatura.

Falta também decidir **quais respostas da triagem desqualificam** — e essa
decisão precisa bater com o item 1(c).

---

## 5. O TCC

**13 `\todo{}`** — todos dependem de você. São 8 tarefas distintas:

- 6 inserções de imagem (as descrições estão no texto, logo abaixo de cada `\todo`)
- descrever o cenário final do jogo: paleta, iluminação, decoração
- registrar o aplicativo de meditação, versão, sessão e volume *(aparece 2×: Cap. 3 e Apêndice)*
- fixar os parâmetros do estressor por sessão, depois do piloto
- confirmar no piloto a parametrização das duas configurações
- a epígrafe: escolher uma ou remover a linha do `main.tex`
- submeter ao CEP e anexar o parecer

**16 `\ph{}`** — não são seus agora. Aguardam a coleta: 9 no Capítulo 4, 2 na
Conclusão, 2 no Resumo, 2 no Abstract, 1 nos Agradecimentos (esse você pode
escrever quando quiser).

Resolvidos nesta rodada: a parametrização numérica das duas configurações, os
limiares do `HitType`, a produção da cena de meditação e a gravação da narração.
