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
    /// Os dez mapas usados no experimento foram regerados com uma nota por
    /// batida, entao aqui a densidade e simplesmente BPM / beatSkipFactor
    /// bolas por minuto. As demais faixas do projeto mantem subdivisoes
    /// variadas e nao entram em nenhuma playlist experimental.
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

    /// <summary>
    /// Se a playlist acabar antes do cronometro, repete a ULTIMA faixa em vez de
    /// voltar ao inicio da lista.
    ///
    /// Usado em C2. A biblioteca nao tem cinco faixas rapidas longas o bastante
    /// para cobrir vinte minutos, entao faltam cerca de dois minutos. Repetir a
    /// ultima mantem a exposicao no ponto mais intenso do arco crescente;
    /// voltar a primeira derrubaria o andamento de 133 para 110 BPM justamente
    /// no fecho da condicao concebida para energizar.
    /// </summary>
    public bool repeatLastTrackWhenShort;

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
/// Cinco faixas por condicao. Os dez mapas correspondentes foram regerados
/// sobre a grade metrica estimada do proprio audio, com uma nota por batida,
/// de modo que a densidade e BPM / 4 bolas por minuto: 18,8 a 23,0 em C1 e
/// 27,5 a 33,3 em C2. As faixas foram escolhidas por duracao e por apoio
/// ritmico medido, isto e, pela fracao de batidas em que o audio de fato
/// apresenta um ataque detectavel.
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
        // Cinco faixas, andamento decrescente de 92 para 75 BPM, densidade de
        // 23,0 a 18,8 bolas por minuto. Somam 1259,3 s de audio, entao o corte
        // dos 20 min cai dentro de "Day In Paris", com 59 s de folga.
        return new List<ExperimentTrack>
        {
            new ExperimentTrack("Colorful Flowers",  4, 92, 243.9f),
            new ExperimentTrack("Slowly",            4, 89, 249.9f),
            new ExperimentTrack("Your Little Wings", 4, 89, 247.2f),
            new ExperimentTrack("Way Home",          4, 85, 278.2f),
            new ExperimentTrack("Day In Paris",      4, 75, 240.1f)
        };
    }

    private static List<ExperimentTrack> LivelyPlaylist()
    {
        // Cinco faixas, andamento crescente de 110 para 133 BPM, densidade de
        // 27,5 a 33,3 bolas por minuto, cerca de 1,5 vez a de C1. Somam
        // 1067,9 s, entao faltam 132 s para os 20 min: a ultima faixa e
        // repetida (ver repeatLastTrackWhenShort).
        return new List<ExperimentTrack>
        {
            new ExperimentTrack("Leaving",    4, 110, 268.4f),
            new ExperimentTrack("STRANDED",   4, 114, 216.1f),
            new ExperimentTrack("Helen 2",    4, 120, 224.0f),
            new ExperimentTrack("Herbal Tea", 4, 130, 184.8f),
            new ExperimentTrack("DAYDREAM",   4, 133, 174.6f)
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
                    repeatLastTrackWhenShort = true,
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
