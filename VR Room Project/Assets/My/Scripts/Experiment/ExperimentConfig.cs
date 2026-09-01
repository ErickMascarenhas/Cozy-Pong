using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Uma faixa dentro da playlist de uma condicao.
/// </summary>
[System.Serializable]
public class ExperimentTrack
{
    /// <summary>Nome do GameObject da musica em "## INGAME/### GAME LOGIC/Songs".</summary>
    public string songObjectName;

    /// <summary>
    /// Uma bola e servida a cada beatSkipFactor notas do arquivo de beatmap.
    ///
    /// ATENCAO: a subdivisao dos mapas NAO e uniforme no projeto. A maioria foi
    /// escrita com uma nota por batida, mas "Helen 2", "METEORITES",
    /// "Herbal Tea" e "DAYDREAM" tem uma nota a cada DUAS batidas. Por isso o
    /// fator e definido por faixa, e nao globalmente: e ele que traz cada
    /// faixa para a banda de densidade pretendida pela condicao.
    ///
    /// Ao completar um mapa, preserve o espacamento que ele ja usa. Reescrever
    /// um mapa de meia subdivisao como uma nota por batida dobra a densidade
    /// daquela faixa e quebra a parametrizacao da condicao.
    /// </summary>
    public int beatSkipFactor;

    /// <summary>Andamento da faixa, em BPM. Documentacao e telemetria.</summary>
    public int bpm;

    /// <summary>Duracao aproximada da faixa, em segundos. Usada so para planejamento.</summary>
    public float approximateSeconds;

    /// <summary>
    /// Faixa de reserva: so entra se alguma anterior for encurtada ou removida.
    /// Em execucao normal o cronometro fecha antes de chegar nela.
    /// </summary>
    public bool isReserve;

    public ExperimentTrack(string songObjectName, int beatSkipFactor, int bpm,
                           float approximateSeconds, bool isReserve = false)
    {
        this.songObjectName = songObjectName;
        this.beatSkipFactor = beatSkipFactor;
        this.bpm = bpm;
        this.approximateSeconds = approximateSeconds;
        this.isReserve = isReserve;
    }

    /// <summary>Intervalo nominal entre bolas, em segundos.</summary>
    public float NominalInterOnsetSeconds
    {
        get { return bpm > 0 ? (60f / bpm) * beatSkipFactor : 0f; }
    }

    /// <summary>Densidade nominal, em bolas por minuto.</summary>
    public float NominalBallsPerMinute
    {
        get
        {
            float ioi = NominalInterOnsetSeconds;
            return ioi > 0f ? 60f / ioi : 0f;
        }
    }
}

/// <summary>
/// Parametrizacao completa de uma condicao experimental.
///
/// Estes valores sao deliberadamente definidos em codigo, e nao em assets do
/// Unity: ficam versionados, sao diffaveis entre commits, nao dependem de
/// desserializacao correta de YAML e podem ser citados diretamente no
/// Capitulo 3 do TCC.
/// </summary>
[System.Serializable]
public class ExperimentConfig
{
    // ---------- identificacao ----------
    public ExperimentCondition condition;
    public string label;
    public string description;

    // ---------- exposicao ----------
    /// <summary>Duracao alvo da exposicao, em segundos. Identica nas quatro condicoes.</summary>
    public float exposureSeconds = 1200f;

    /// <summary>Duracao do esmaecimento de audio imediatamente antes do corte.</summary>
    public float endFadeSeconds = 2f;

    // ---------- o que a condicao usa ----------
    /// <summary>Serve bolas e pontua. Falso em C3 e C4.</summary>
    public bool usesGameplay;

    /// <summary>Reproduz a playlist. Falso em C4.</summary>
    public bool usesSoundtrack;

    /// <summary>Participante usa o HMD. Falso em C3 e C4.</summary>
    public bool usesHeadset;

    /// <summary>Escurece a tela do operador para nao servir de estimulo visual.</summary>
    public bool blankDisplay;

    // ---------- regime de erro ----------
    public bool immortal = true;
    public bool useErrorBoxes;

    /// <summary>Em C2, ao esgotar as caixas de erro a faixa reinicia e o cronometro continua.</summary>
    public bool restartTrackOnFailure;

    // ---------- fisica e julgamento ----------
    /// <summary>Tempo ate a bola ser destruida e contada como Miss, em segundos.</summary>
    public float ballLifeTime = 1.72f;

    /// <summary>Altura extra do arco da bola. Menor = trajetoria mais reta e mais rapida.</summary>
    public float arcHeight = 0.25f;

    public float velocityHomeRunThreshold = 3.5f;
    public float velocityOkThreshold = 1.5f;

    /// <summary>Limiar de erro de sincronizacao para Perfect, em segundos.</summary>
    public float timingPerfectThreshold = 0.2f;

    /// <summary>Limiar de erro de sincronizacao para Ok, em segundos.</summary>
    public float timingOkThreshold = 0.4f;

    // ---------- retroalimentacao ----------
    /// <summary>Mostra placar, combo e nota. Falso em C1: a retroalimentacao e de apoio.</summary>
    public bool showScoreUi = true;

    // ---------- audio ----------
    /// <summary>Volume mestre aplicado ao AudioMixer, em dB. Travado durante a sessao.</summary>
    public float masterVolumeDb;

    public float musicVolumeDb;
    public float sfxVolumeDb = -3f;

    // ---------- visual ----------
    /// <summary>Cicla o matiz da luz reativa. Sempre falso no experimento.</summary>
    public bool lightHueCycle;

    public Color lightColor = new Color(1f, 0.85f, 0.65f);
    public float lightMinIntensity = 0.5f;
    public float lightMaxIntensity = 1.6f;

    // ---------- playlist ----------
    public List<ExperimentTrack> playlist = new List<ExperimentTrack>();

    /// <summary>Soma das duracoes aproximadas da playlist, em segundos.</summary>
    public float TotalPlaylistSeconds
    {
        get
        {
            float t = 0f;
            for (int i = 0; i < playlist.Count; i++) t += playlist[i].approximateSeconds;
            return t;
        }
    }
}

/// <summary>
/// As configuracoes das condicoes experimentais.
///
/// PLAYLISTS
/// A densidade de bolas de uma faixa e 60 / (espacamento das notas no arquivo x
/// beatSkipFactor). O espacamento varia entre os mapas do projeto, entao o
/// fator de cada faixa foi escolhido para trazer todas as faixas de uma
/// condicao para a mesma banda de densidade: 17,7 a 20,0 bolas por minuto em
/// C1 e 27,5 a 33,2 em C2.
///
/// A ordem das faixas segue o principio iso da musicoterapia: a intervencao
/// parte de um andamento proximo ao estado provavel do participante logo apos
/// o estressor e o conduz gradualmente. Em C1 o andamento decresce de 80 para
/// 71 BPM; em C2 cresce de 110 para 133 BPM.
/// </summary>
public static class ExperimentConfigs
{
    /// <summary>Duracao padrao da exposicao. Ver Secao 3.9 do TCC.</summary>
    public const float DefaultExposureSeconds = 1200f;

    /// <summary>Semente padrao. Fixada para que a sequencia espacial seja identica entre participantes.</summary>
    public const int DefaultSeed = 20260901;

    private static List<ExperimentTrack> CalmPlaylist()
    {
        // Densidade de 17,7 a 20,0 bolas por minuto (intervalo de 3,0 a 3,4 s).
        // Sete faixas obrigatorias somam 1227,8 s; o corte de 20 min cai dentro
        // de "Eastridge Turnstile". "Windy" e reserva.
        return new List<ExperimentTrack>
        {
            new ExperimentTrack("CELESTIAL GOLD",      4,  80, 203.7f),
            new ExperimentTrack("REMEMBER",            4,  77, 154.8f),
            new ExperimentTrack("Miss You",            4,  75, 179.3f),
            new ExperimentTrack("Day In Paris",        4,  75, 240.2f),
            new ExperimentTrack("Distant",             4,  75, 147.3f),
            new ExperimentTrack("Faithful Mission",    4,  74, 152.8f),
            new ExperimentTrack("Eastridge Turnstile", 4,  74, 149.7f),
            new ExperimentTrack("Windy",               4,  71, 155.6f, true)
        };
    }

    private static List<ExperimentTrack> LivelyPlaylist()
    {
        // Densidade de 27,5 a 33,2 bolas por minuto (intervalo de 1,8 a 2,2 s),
        // cerca de 1,7 vez a de C1. Seis faixas obrigatorias somam 1252,5 s;
        // o corte de 20 min cai dentro de "Herbal Tea". "DAYDREAM" e reserva.
        return new List<ExperimentTrack>
        {
            new ExperimentTrack("Leaving",      4, 110, 268.5f),
            new ExperimentTrack("Warm Horizon", 4, 110, 175.5f),
            new ExperimentTrack("STRANDED",     4, 114, 216.2f),
            new ExperimentTrack("Helen 2",      2, 120, 224.1f),
            new ExperimentTrack("METEORITES",   2, 126, 183.5f),
            new ExperimentTrack("Herbal Tea",   2, 130, 184.8f),
            new ExperimentTrack("DAYDREAM",     2, 133, 174.7f, true)
        };
    }

    public static ExperimentConfig Get(ExperimentCondition condition)
    {
        switch (condition)
        {
            case ExperimentCondition.C1_CozyPongCalm:
                return new ExperimentConfig
                {
                    condition = condition,
                    label = "C1 - Cozy Pong relaxante",
                    description = "Intervencao avaliada. Regime imortal, densidade baixa, " +
                                  "trilha lo-fi calma, retroalimentacao de apoio.",
                    usesGameplay = true,
                    usesSoundtrack = true,
                    usesHeadset = true,
                    blankDisplay = false,
                    immortal = true,
                    useErrorBoxes = false,
                    restartTrackOnFailure = false,
                    ballLifeTime = 1.72f,
                    arcHeight = 0.25f,
                    velocityHomeRunThreshold = 3.5f,
                    velocityOkThreshold = 1.5f,
                    timingPerfectThreshold = 0.25f,
                    timingOkThreshold = 0.50f,
                    showScoreUi = false,
                    lightHueCycle = false,
                    lightColor = new Color(1f, 0.84f, 0.66f),
                    lightMinIntensity = 0.6f,
                    lightMaxIntensity = 1.4f,
                    playlist = CalmPlaylist()
                };

            case ExperimentCondition.C2_CozyPongLively:
                return new ExperimentConfig
                {
                    condition = condition,
                    label = "C2 - Cozy Pong animado",
                    description = "Controle de projeto. Caixas de erro, densidade alta, " +
                                  "trilha rapida, bola mais reta e julgamento mais exigente.",
                    usesGameplay = true,
                    usesSoundtrack = true,
                    usesHeadset = true,
                    blankDisplay = false,
                    immortal = false,
                    useErrorBoxes = true,
                    restartTrackOnFailure = true,
                    ballLifeTime = 1.40f,
                    arcHeight = 0.15f,
                    velocityHomeRunThreshold = 3.5f,
                    velocityOkThreshold = 1.5f,
                    timingPerfectThreshold = 0.15f,
                    timingOkThreshold = 0.30f,
                    showScoreUi = true,
                    lightHueCycle = false,
                    lightColor = new Color(0.72f, 0.82f, 1f),
                    lightMinIntensity = 0.7f,
                    lightMaxIntensity = 2.2f,
                    playlist = LivelyPlaylist()
                };

            case ExperimentCondition.C3_SoundtrackOnly:
                return new ExperimentConfig
                {
                    condition = condition,
                    label = "C3 - Trilha lo-fi sentado",
                    description = "Controle passivo. Playlist identica a C1, mesma ordem e " +
                                  "mesmo volume, sem jogo e sem RV.",
                    usesGameplay = false,
                    usesSoundtrack = true,
                    usesHeadset = false,
                    blankDisplay = true,
                    immortal = true,
                    useErrorBoxes = false,
                    showScoreUi = false,
                    lightHueCycle = false,
                    playlist = CalmPlaylist()
                };

            case ExperimentCondition.C4_ExternalMeditation:
                return new ExperimentConfig
                {
                    condition = condition,
                    label = "C4 - Meditacao guiada (aplicativo externo)",
                    description = "A meditacao e conduzida por aplicativo validado, fora deste " +
                                  "projeto. Aqui roda apenas o cronometro de exposicao e o " +
                                  "registro de marcadores, para que a precisao temporal seja " +
                                  "identica a das demais condicoes.",
                    usesGameplay = false,
                    usesSoundtrack = false,
                    usesHeadset = false,
                    blankDisplay = true,
                    immortal = true,
                    useErrorBoxes = false,
                    showScoreUi = false,
                    lightHueCycle = false,
                    playlist = new List<ExperimentTrack>()
                };

            case ExperimentCondition.FAM_Familiarization:
                return new ExperimentConfig
                {
                    condition = condition,
                    label = "FAM - Familiarizacao com RV",
                    description = "Sessao de orientacao. Tarefa neutra e curta, sem servir bolas " +
                                  "e sem reproduzir a jogabilidade do Cozy Pong.",
                    exposureSeconds = 240f,
                    usesGameplay = false,
                    usesSoundtrack = false,
                    usesHeadset = true,
                    blankDisplay = false,
                    // A familiarizacao acontece no ambiente do lobby, que tem
                    // musica de fundo propria. Ela fica muda para que o
                    // participante nao seja exposto a trilha antes da primeira
                    // condicao.
                    musicVolumeDb = -80f,
                    immortal = true,
                    useErrorBoxes = false,
                    showScoreUi = false,
                    lightHueCycle = false,
                    playlist = new List<ExperimentTrack>()
                };

            default:
                return new ExperimentConfig
                {
                    condition = ExperimentCondition.None,
                    label = "Fora do experimento",
                    playlist = new List<ExperimentTrack>()
                };
        }
    }
}
