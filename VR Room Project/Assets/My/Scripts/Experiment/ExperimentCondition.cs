/// <summary>
/// Condicoes previstas no protocolo experimental (Capitulo 3 do TCC).
///
/// C1, C2 e C3 sao executadas por este aplicativo. C4 acontece fora dele,
/// em um aplicativo de meditacao guiada ja validado; ainda assim existe aqui
/// como condicao, para que o cronometro de exposicao e os marcadores temporais
/// tenham exatamente a mesma precisao nas quatro condicoes.
/// </summary>
public enum ExperimentCondition
{
    /// <summary>Nenhuma. O jogo roda normalmente, fora do experimento.</summary>
    None = 0,

    /// <summary>C1: Cozy Pong em configuracao relaxante. Intervencao avaliada.</summary>
    C1_CozyPongCalm = 1,

    /// <summary>C2: Cozy Pong em configuracao animada. Controle de projeto.</summary>
    C2_CozyPongLively = 2,

    /// <summary>C3: mesma trilha de C1, sentado, sem jogo e sem RV. Controle passivo.</summary>
    C3_SoundtrackOnly = 3,

    /// <summary>C4: meditacao guiada em aplicativo externo. Aqui, so cronometro e marcadores.</summary>
    C4_ExternalMeditation = 4,

    /// <summary>Familiarizacao neutra com RV, na sessao de orientacao.</summary>
    FAM_Familiarization = 5
}
