using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Liga o modo experimento antes de a cena carregar, sem exigir nenhuma
/// alteracao na cena.
///
/// A sessao e descrita por um arquivo JSON. Enquanto ele nao existir, o jogo
/// roda exatamente como a versao publica; quando existir, a sessao passa a ser
/// conduzida pelo <see cref="ExperimentSessionManager"/>.
///
/// Onde colocar o arquivo:
///   Editor / Windows : %USERPROFILE%/AppData/LocalLow/&lt;empresa&gt;/&lt;produto&gt;/experiment_session.json
///   Quest            : adb push experiment_session.json /sdcard/Android/data/&lt;pacote&gt;/files/
///
/// Tambem aceita argumentos de linha de comando, uteis para automatizar:
///   -experimentSession &lt;caminho&gt;
///   -participant P07 -session 3 -condition C1 [-exposure 1200] [-seed 20260901]
/// </summary>
public static class ExperimentBootstrap
{
    public const string SessionFileName = "experiment_session.json";

    public static string DefaultSessionFilePath
    {
        get { return Path.Combine(Application.persistentDataPath, SessionFileName); }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        ExperimentSessionRequest request = ReadFromCommandLine();
        if (request == null) request = ReadFromFile(DefaultSessionFilePath);

        if (request != null) ExperimentMode.Activate(request);

        if (ExperimentMode.IsActive)
        {
            GameObject host = new GameObject("[Experiment]");
            UnityEngine.Object.DontDestroyOnLoad(host);
            host.AddComponent<SessionLogger>();
            host.AddComponent<FrameRateMonitor>();
            host.AddComponent<ExperimentSessionManager>();
            host.AddComponent<ResearcherOverlay>();
            return;
        }

        // Sem sessao armada o jogo roda normalmente, mas o lobby passa a listar
        // as condicoes em vez das 53 musicas, e a playlist escolhida e encadeada
        // pela tela de conclusao.
        ExperimentMode.Deactivate();
        ExperimentRandom.Clear();

        GameObject playlistHost = new GameObject("[Playlist]");
        UnityEngine.Object.DontDestroyOnLoad(playlistHost);
        playlistHost.AddComponent<PlaylistNavigator>();
    }

    // ------------------------------------------------------------------
    // leitura
    // ------------------------------------------------------------------

    public static ExperimentSessionRequest ReadFromFile(string path)
    {
        try
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return null;
            string json = File.ReadAllText(path);
            if (string.IsNullOrEmpty(json)) return null;
            ExperimentSessionRequest request = JsonUtility.FromJson<ExperimentSessionRequest>(json);
            if (request == null || string.IsNullOrEmpty(request.condition)) return null;
            Debug.Log("[Experimento] Sessao lida de " + path);
            return request;
        }
        catch (Exception e)
        {
            Debug.LogError("[Experimento] Falha ao ler " + path + ": " + e.Message);
            return null;
        }
    }

    public static bool WriteToFile(ExperimentSessionRequest request, string path)
    {
        try
        {
            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            File.WriteAllText(path, JsonUtility.ToJson(request, true));
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("[Experimento] Falha ao gravar " + path + ": " + e.Message);
            return false;
        }
    }

    private static ExperimentSessionRequest ReadFromCommandLine()
    {
        string[] args;
        try
        {
            args = Environment.GetCommandLineArgs();
        }
        catch
        {
            return null;
        }
        if (args == null || args.Length < 2) return null;

        string sessionPath = null;
        ExperimentSessionRequest inline = null;

        for (int i = 0; i < args.Length; i++)
        {
            string a = args[i];
            string next = i + 1 < args.Length ? args[i + 1] : null;
            switch (a)
            {
                case "-experimentSession":
                    sessionPath = next;
                    break;
                case "-participant":
                    if (inline == null) inline = new ExperimentSessionRequest();
                    inline.participantId = next;
                    break;
                case "-session":
                    if (inline == null) inline = new ExperimentSessionRequest();
                    int sessionNumber;
                    if (int.TryParse(next, out sessionNumber)) inline.sessionNumber = sessionNumber;
                    break;
                case "-condition":
                    if (inline == null) inline = new ExperimentSessionRequest();
                    inline.condition = next;
                    break;
                case "-exposure":
                    if (inline == null) inline = new ExperimentSessionRequest();
                    float exposure;
                    if (float.TryParse(next, System.Globalization.NumberStyles.Float,
                                       System.Globalization.CultureInfo.InvariantCulture, out exposure))
                        inline.exposureSeconds = exposure;
                    break;
                case "-seed":
                    if (inline == null) inline = new ExperimentSessionRequest();
                    int seed;
                    if (int.TryParse(next, out seed)) inline.seed = seed;
                    break;
            }
        }

        if (!string.IsNullOrEmpty(sessionPath)) return ReadFromFile(sessionPath);
        return inline;
    }
}
