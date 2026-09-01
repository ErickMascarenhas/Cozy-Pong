using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Janela do editor para preparar uma sessao experimental.
///
/// Escreve o arquivo experiment_session.json que o
/// <see cref="ExperimentBootstrap"/> le ao iniciar. Enquanto o arquivo nao
/// existir, o jogo roda como a versao publica.
/// </summary>
public class ExperimentSessionWindow : EditorWindow
{
    private string _participantId = "P01";
    private int _sessionNumber = 1;
    private ExperimentCondition _condition = ExperimentCondition.C1_CozyPongCalm;
    private float _exposureSeconds = ExperimentConfigs.DefaultExposureSeconds;
    private int _seed = ExperimentConfigs.DefaultSeed;
    private string _notes = "";
    private Vector2 _scroll;

    [MenuItem("Cozy Pong/Sessao experimental", false, 0)]
    public static void Open()
    {
        ExperimentSessionWindow window = GetWindow<ExperimentSessionWindow>(true, "Sessao experimental");
        window.minSize = new Vector2(430, 560);
    }

    private void OnGUI()
    {
        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        EditorGUILayout.LabelField("Identificacao", EditorStyles.boldLabel);
        _participantId = EditorGUILayout.TextField("Codigo do participante", _participantId);
        _sessionNumber = EditorGUILayout.IntField("Numero da sessao", _sessionNumber);
        _condition = (ExperimentCondition)EditorGUILayout.EnumPopup("Condicao", _condition);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Parametros", EditorStyles.boldLabel);
        _exposureSeconds = EditorGUILayout.FloatField("Exposicao (s)", _exposureSeconds);
        _seed = EditorGUILayout.IntField("Semente", _seed);
        EditorGUILayout.LabelField("Observacoes");
        _notes = EditorGUILayout.TextArea(_notes, GUILayout.Height(48));

        EditorGUILayout.Space();
        DrawConfigSummary();

        EditorGUILayout.Space();
        bool exists = File.Exists(SessionPath);
        EditorGUILayout.HelpBox(
            (exists
                ? "O modo experimento esta ARMADO. A proxima execucao sera uma sessao."
                : "O modo experimento esta desligado. A proxima execucao sera o jogo normal.") +
            "\n\nArquivo: " + SessionPath,
            exists ? MessageType.Warning : MessageType.Info);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Armar sessao", GUILayout.Height(32))) Arm();
        GUI.enabled = exists;
        if (GUILayout.Button("Desarmar", GUILayout.Height(32))) Disarm();
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        if (GUILayout.Button("Abrir a pasta de dados"))
        {
            string folder = Path.Combine(Application.persistentDataPath, SessionLogger.FolderName);
            Directory.CreateDirectory(folder);
            EditorUtility.RevealInFinder(folder);
        }

        EditorGUILayout.EndScrollView();
    }

    private static string SessionPath
    {
        get { return Path.Combine(Application.persistentDataPath, ExperimentBootstrap.SessionFileName); }
    }

    private void DrawConfigSummary()
    {
        ExperimentConfig config = ExperimentConfigs.Get(_condition);
        EditorGUILayout.LabelField("Resumo da condicao", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(config.label);
        EditorGUILayout.LabelField(config.description, EditorStyles.wordWrappedMiniLabel);
        EditorGUILayout.LabelField(string.Format(
            "jogo: {0}   trilha: {1}   HMD: {2}   imortal: {3}   placar: {4}",
            config.usesGameplay ? "sim" : "nao",
            config.usesSoundtrack ? "sim" : "nao",
            config.usesHeadset ? "sim" : "nao",
            config.immortal ? "sim" : "nao",
            config.showScoreUi ? "sim" : "nao"), EditorStyles.miniLabel);

        if (config.playlist != null && config.playlist.Count > 0)
        {
            float cumulative = 0f;
            EditorGUILayout.LabelField(string.Format("Playlist ({0:F0} s no total)",
                config.TotalPlaylistSeconds), EditorStyles.miniBoldLabel);
            for (int i = 0; i < config.playlist.Count; i++)
            {
                ExperimentTrack t = config.playlist[i];
                cumulative += t.approximateSeconds;
                EditorGUILayout.LabelField(string.Format("  {0}. {1} - {2} BPM - {3:F1} bolas/min - acum {4:F0} s{5}",
                    i + 1, t.songObjectName, t.bpm, t.NominalBallsPerMinute, cumulative,
                    t.isReserve ? "  [reserva]" : ""), EditorStyles.miniLabel);
            }
        }
    }

    private void Arm()
    {
        if (_condition == ExperimentCondition.None)
        {
            EditorUtility.DisplayDialog("Sessao experimental",
                "Escolha uma condicao diferente de None.", "Entendi");
            return;
        }

        ExperimentSessionRequest request = new ExperimentSessionRequest
        {
            participantId = _participantId,
            sessionNumber = _sessionNumber,
            condition = ExperimentSessionRequest.ToShortCode(_condition),
            exposureSeconds = _exposureSeconds,
            seed = _seed,
            notes = _notes
        };

        if (ExperimentBootstrap.WriteToFile(request, SessionPath))
            Debug.Log("[Experimento] Sessao armada em " + SessionPath);
    }

    private void Disarm()
    {
        try
        {
            if (File.Exists(SessionPath)) File.Delete(SessionPath);
            Debug.Log("[Experimento] Sessao desarmada. A proxima execucao sera o jogo normal.");
        }
        catch (System.Exception e)
        {
            Debug.LogError("[Experimento] Nao foi possivel apagar " + SessionPath + ": " + e.Message);
        }
    }
}
