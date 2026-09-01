using UnityEngine;

/// <summary>
/// Descricao de uma sessao experimental, tal como o pesquisador a informa.
/// Serializavel por JsonUtility para poder ser lida de um arquivo.
/// </summary>
[System.Serializable]
public class ExperimentSessionRequest
{
    /// <summary>Codigo do participante. Nunca o nome: ver Secao 8 do TCLE.</summary>
    public string participantId = "P00";

    /// <summary>Numero da sessao deste participante, de 1 em diante.</summary>
    public int sessionNumber = 1;

    /// <summary>"C1", "C2", "C3", "C4" ou "FAM".</summary>
    public string condition = "C1";

    /// <summary>Sobrescreve a duracao da exposicao. Zero usa o valor da condicao.</summary>
    public float exposureSeconds = 0f;

    /// <summary>Sobrescreve a semente do gerador aleatorio. Zero usa a padrao.</summary>
    public int seed = 0;

    /// <summary>Observacao livre do pesquisador, copiada para o arquivo de metadados.</summary>
    public string notes = "";

    public static ExperimentCondition ParseCondition(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return ExperimentCondition.None;
        switch (raw.Trim().ToUpperInvariant())
        {
            case "C1": return ExperimentCondition.C1_CozyPongCalm;
            case "C2": return ExperimentCondition.C2_CozyPongLively;
            case "C3": return ExperimentCondition.C3_SoundtrackOnly;
            case "C4": return ExperimentCondition.C4_ExternalMeditation;
            case "FAM": return ExperimentCondition.FAM_Familiarization;
            default: return ExperimentCondition.None;
        }
    }

    public static string ToShortCode(ExperimentCondition condition)
    {
        switch (condition)
        {
            case ExperimentCondition.C1_CozyPongCalm: return "C1";
            case ExperimentCondition.C2_CozyPongLively: return "C2";
            case ExperimentCondition.C3_SoundtrackOnly: return "C3";
            case ExperimentCondition.C4_ExternalMeditation: return "C4";
            case ExperimentCondition.FAM_Familiarization: return "FAM";
            default: return "NONE";
        }
    }
}

/// <summary>
/// Interruptor global do modo experimento.
///
/// Enquanto estiver desligado, o jogo se comporta exatamente como a versao
/// publica: lobby, escolha livre de musica, recordes e controles de volume.
/// Quando ligado, tudo isso e suspenso e a sessao passa a ser conduzida pelo
/// <see cref="ExperimentSessionManager"/>.
/// </summary>
public static class ExperimentMode
{
    public static bool IsActive { get; private set; }
    public static ExperimentSessionRequest Request { get; private set; }
    public static ExperimentConfig Config { get; private set; }
    public static ExperimentCondition Condition { get; private set; }
    public static int Seed { get; private set; }

    public static void Activate(ExperimentSessionRequest request)
    {
        if (request == null) return;

        ExperimentCondition condition = ExperimentSessionRequest.ParseCondition(request.condition);
        if (condition == ExperimentCondition.None)
        {
            Debug.LogError("[Experimento] Condicao invalida: '" + request.condition +
                           "'. O modo experimento nao foi ativado.");
            return;
        }

        ExperimentConfig config = ExperimentConfigs.Get(condition);
        if (request.exposureSeconds > 0f) config.exposureSeconds = request.exposureSeconds;

        Request = request;
        Config = config;
        Condition = condition;
        Seed = request.seed != 0 ? request.seed : ExperimentConfigs.DefaultSeed;
        IsActive = true;

        ExperimentRandom.Reseed(Seed);

        Debug.Log("[Experimento] Ativado. Participante " + request.participantId +
                  ", sessao " + request.sessionNumber + ", condicao " + config.label +
                  ", exposicao " + config.exposureSeconds + " s, semente " + Seed + ".");
    }

    public static void Deactivate()
    {
        IsActive = false;
        Request = null;
        Config = null;
        Condition = ExperimentCondition.None;
    }

    /// <summary>Codigo curto da condicao corrente, para nomes de arquivo e logs.</summary>
    public static string ConditionCode
    {
        get { return ExperimentSessionRequest.ToShortCode(Condition); }
    }
}
