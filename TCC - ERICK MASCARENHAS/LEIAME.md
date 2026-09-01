# TCC — Cozy Pong (projeto LaTeX)

Trabalho de Conclusão de Curso sobre o **Cozy Pong**, jogo de RV rítmico para
redução de estresse e ansiedade. Usa a classe institucional `JITH.cls` (IC-UFAL).

## Como compilar

```bash
latexmk -pdf main.tex
```

Depois, para limpar os temporários:

```bash
latexmk -c
```

O `.latexmkrc` do projeto já cuida do `makeglossaries` (glossário de siglas) e
do `bibtex` automaticamente. Estado atual: **compila limpo, 62 páginas, sem
referências ou citações pendentes**.

## Estrutura

```
main.tex              -> arquivo principal
Identificacao.tex     -> autoria, orientador, banca, data   [banca e data pendentes]
macros.tex            -> macro \todo{} (nota vermelha inline)
JITH.cls              -> classe institucional
.latexmkrc            -> automação de glossário/bibliografia
Cap00/                -> capa/logo, Resumo, Abstract, Glossário, Símbolos,
                         Abreviações, Agradecimentos, Epígrafe
Cap01/                -> Introdução (perguntas PP1-PP3, objetivos, hipóteses H1-H6)
Cap02/                -> Fundamentação Teórica
Cap03/                -> Metodologia (desenvolvimento do jogo + protocolo)
Cap04/                -> Resultados e Discussões  [esqueleto, aguarda coleta]
Conc/                 -> Conclusão                [parcialmente placeholder]
Apend/Apendices.tex   -> Apêndices A-D: roteiros padronizados para o CEP
Ref/SampleReferences.bib -> bibliografia (33 entradas)
images/               -> (criar) figuras; ver IMAGENS_NECESSARIAS.md
```

## Apêndices (roteiros para o CEP)

| | Conteúdo |
|---|---|
| **A** | Roteiro geral da sessão: recepção, conformidade, sensores, linha de base |
| **B** | Tarefa de indução de estresse: instrução, execução, verificação |
| **C** | Roteiros das 4 condições + **narração completa da meditação guiada** |
| **D** | Encerramento, *debriefing* obrigatório, critérios de interrupção, higiene |

Convenções de leitura dos roteiros: `[A]` = ação do pesquisador; texto recuado
em itálico = fala lida em voz alta, sem improviso; `[D]` = ponto de decisão com
critério explícito.

**Meditação (C4):** decidido construir a cena no próprio projeto Unity, com
narração gravada a partir do roteiro do Apêndice C. Evita dependência de app de
terceiros (licença, atualizações no meio da coleta, ausência de marcadores de
evento) e mantém plataforma idêntica à das condições de jogo.

## Desenho experimental consolidado (30/07/2026)

Intra-sujeito (*crossover*), contrabalanceado por quadrado de Williams,
**N = 32** completadores (recrutar 40), **4 sessões** por participante.

| | Condição | Papel |
|---|---|---|
| **C1** | Cozy Pong relaxante | intervenção avaliada |
| **C2** | Cozy Pong animado | isola a caracterização relaxante |
| **C3** | Trilha lo-fi sentado, sem jogar | isola o efeito do jogo (música constante) |
| **C4** | Meditação guiada em RV | comparador do estado da arte (meio constante) |

- **Desfecho primário psicológico:** Δ VAS-Ansiedade (pós-estressor → pós-intervenção)
- **Desfecho primário fisiológico:** Δ ln(RMSSD) (linha de base → recuperação)
- **Equipamentos:** Polar H10 (cardíaco) + Shimmer3 GSR+ (EDA; alternativa: BITalino)
- **Indução de estresse:** aritmética mental sob pressão de tempo (5 min), antes de cada condição

## O que ainda falta

**Decisões suas (não dá para eu resolver):**
- Banca examinadora e data da defesa (`Identificacao.tex`).
- Submissão ao CEP + anexar parecer e TCLE.
- Agradecimentos e epígrafe (texto pessoal).

**Produção de artefato:**
- Cena de **meditação guiada** no projeto Unity (condição C4) e sua narração.
- Parametrização numérica final das configurações **relaxante × animada**.
- Descrição do cenário final do jogo (paleta, iluminação, decoração).
- Imagens: ver `IMAGENS_NECESSARIAS.md`.

**Pesquisa:**
- Executar a **revisão sistemática** e preencher Trabalhos Relacionados (Cap. 2).
- 6 referências de domínio marcadas com `[TODO]` no texto (epidemiologia,
  RV/relaxamento, música lo-fi, exergames, jogos de ritmo, sincronização
  sensório-motora).

**Após a coleta:**
- Todo o Cap. 4 (as tabelas já estão montadas, com células `[--]`).
- Conclusão principal e ajuste das contribuições.

## Convenções

- `[TODO: ...]` em vermelho no PDF = pendência pontual.
- `[PLACEHOLDER: ...]` = trecho a redigir.
- Ambos são fáceis de localizar: `grep -rn "TODO\|PLACEHOLDER" --include=*.tex .`
