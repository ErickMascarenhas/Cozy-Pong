using System;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// Registro em disco de uma sessao experimental.
///
/// Escreve tres arquivos CSV por sessao, em Application.persistentDataPath:
///
///   *.meta.csv     uma linha, com tudo que o Capitulo 4 precisa reportar
///   *.markers.csv  marcadores temporais, para alinhar com VFC e EDA
///   *.events.csv   uma linha por bola servida, so em C1 e C2
///
/// Cada linha carrega tres relogios: o tempo universal em milissegundos, o
/// relogio de audio do Unity e o tempo decorrido desde o inicio da sessao. O
/// primeiro alinha com os equipamentos externos; o segundo e o unico confiavel
/// para julgar ritmo; o terceiro e o que se le a olho nu.
///
/// Nenhum dado que identifique a pessoa entra nestes arquivos, apenas o codigo
/// do participante, conforme a Secao 8 do TCLE.
/// </summary>
public class SessionLogger : MonoBehaviour
{
    public const string FolderName = "ExperimentData";

    /// <summary>
    /// O registro da sessao em andamento, se houver. Existe para que a
    /// jogabilidade possa anotar eventos sem precisar conhecer o orquestrador
    /// da sessao.
    /// </summary>
    public static SessionLogger Current { get; private set; }

    private StreamWriter _markers;
    private StreamWriter _events;
    private string _directory;
    private string _baseName;

    private DateTime _startUtc;
    private double _startRealtime;
    private double _startDsp;

    private int _eventCount;
    private int _markerCount;

    public bool IsOpen { get; private set; }
    public string LastError { get; private set; }
    public string Directory { get { return _directory; } }
    public string BaseName { get { return _baseName; } }
    public int EventCount { get { return _eventCount; } }

    /// <summary>Segundos decorridos desde a abertura do registro.</summary>
    public double SessionSeconds
    {
        get { return IsOpen ? Time.realtimeSinceStartupAsDouble - _startRealtime : 0.0; }
    }

    // ------------------------------------------------------------------
    // abertura e fechamento
    // ------------------------------------------------------------------

    public bool Begin(ExperimentSessionRequest request, ExperimentConfig config)
    {
        if (IsOpen) return true;

        try
        {
            _startUtc = DateTime.UtcNow;
            _startRealtime = Time.realtimeSinceStartupAsDouble;
            _startDsp = AudioSettings.dspTime;

            _directory = Path.Combine(Application.persistentDataPath, FolderName);
            System.IO.Directory.CreateDirectory(_directory);

            _baseName = string.Format(CultureInfo.InvariantCulture, "{0}_S{1:00}_{2}_{3}",
                Sanitize(request.participantId),
                request.sessionNumber,
                ExperimentSessionRequest.ToShortCode(config.condition),
                DateTime.Now.ToString("yyyy-MM-dd'T'HH-mm-ss", CultureInfo.InvariantCulture));

            _markers = Open(_baseName + ".markers.csv");
            _markers.WriteLine("unix_ms,dsp_s,session_s,marker,detail");

            if (config.usesGameplay)
            {
                _events = Open(_baseName + ".events.csv");
                _events.WriteLine(
                    "unix_ms,dsp_s,session_s,track,note_index,ball_type," +
                    "spawn_point,target_point,beat_time_s,launch_time_s,contact_time_s," +
                    "e_i_ms,delta_target_ms,classification,racket_speed,combo");
            }

            IsOpen = true;
            LastError = null;
            return true;
        }
        catch (Exception e)
        {
            LastError = e.Message;
            Debug.LogError("[Experimento] Nao foi possivel abrir o registro da sessao: " + e);
            IsOpen = false;
            return false;
        }
    }

    private StreamWriter Open(string fileName)
    {
        StreamWriter w = new StreamWriter(Path.Combine(_directory, fileName), false, new UTF8Encoding(false));
        w.AutoFlush = true;
        return w;
    }

    public void Close()
    {
        try
        {
            if (_markers != null) { _markers.Flush(); _markers.Dispose(); _markers = null; }
            if (_events != null) { _events.Flush(); _events.Dispose(); _events = null; }
        }
        catch (Exception e)
        {
            LastError = e.Message;
        }
        IsOpen = false;
    }

    private void OnApplicationQuit()
    {
        Marker("APP_QUIT", "encerramento do aplicativo");
        Close();
    }

    private void Awake()
    {
        Current = this;
    }

    private void OnDestroy()
    {
        if (Current == this) Current = null;
        Close();
    }

    // ------------------------------------------------------------------
    // marcadores
    // ------------------------------------------------------------------

    public void Marker(string name, string detail = "")
    {
        if (!IsOpen || _markers == null) return;
        try
        {
            _markers.WriteLine(string.Concat(Clocks(), ",", Csv(name), ",", Csv(detail)));
            _markerCount++;
        }
        catch (Exception e)
        {
            LastError = e.Message;
        }
    }

    // ------------------------------------------------------------------
    // eventos de nota
    // ------------------------------------------------------------------

    /// <summary>
    /// Registra uma bola servida e o seu desfecho.
    /// </summary>
    /// <param name="errorToNearestBeatMs">
    /// O e_i da Equacao 3.1: menor distancia absoluta entre o instante do
    /// evento e qualquer batida do beatmap.
    /// </param>
    /// <param name="deltaToTargetBeatMs">
    /// Diferenca com sinal em relacao a batida que originou aquela bola.
    /// Negativo significa antecipacao.
    /// </param>
    public void LogNote(string track, int noteIndex, int ballType,
                        int spawnPoint, int targetPoint,
                        double beatTimeSeconds, double launchTimeSeconds, double contactTimeSeconds,
                        double errorToNearestBeatMs, double deltaToTargetBeatMs,
                        string classification, float racketSpeed, int combo)
    {
        if (!IsOpen || _events == null) return;
        try
        {
            StringBuilder sb = new StringBuilder(220);
            sb.Append(Clocks()).Append(',');
            sb.Append(Csv(track)).Append(',');
            sb.Append(noteIndex).Append(',');
            sb.Append(ballType).Append(',');
            sb.Append(spawnPoint).Append(',');
            sb.Append(targetPoint).Append(',');
            sb.Append(F(beatTimeSeconds, 4)).Append(',');
            sb.Append(F(launchTimeSeconds, 4)).Append(',');
            sb.Append(F(contactTimeSeconds, 4)).Append(',');
            sb.Append(F(errorToNearestBeatMs, 2)).Append(',');
            sb.Append(F(deltaToTargetBeatMs, 2)).Append(',');
            sb.Append(Csv(classification)).Append(',');
            sb.Append(F(racketSpeed, 3)).Append(',');
            sb.Append(combo);
            _events.WriteLine(sb.ToString());
            _eventCount++;
        }
        catch (Exception e)
        {
            LastError = e.Message;
        }
    }

    // ------------------------------------------------------------------
    // metadados
    // ------------------------------------------------------------------

    public void WriteMeta(ExperimentSessionRequest request, ExperimentConfig config,
                          FrameRateMonitor frames, string endReason,
                          double exposedSeconds, int trackRestarts, int playlistLoops)
    {
        if (string.IsNullOrEmpty(_directory)) return;
        try
        {
            string path = Path.Combine(_directory, _baseName + ".meta.csv");
            using (StreamWriter w = new StreamWriter(path, false, new UTF8Encoding(false)))
            {
                string[] keys =
                {
                    "participant", "session", "condition", "condition_label",
                    "app_version", "unity_version", "product", "device_model", "device_type",
                    "platform", "seed", "exposure_target_s", "exposure_actual_s", "end_reason",
                    "track_restarts", "playlist_loops", "note_events",
                    "master_volume_db", "music_volume_db", "sfx_volume_db",
                    "ball_life_time_s", "arc_height_m",
                    "timing_perfect_s", "timing_ok_s",
                    "velocity_homerun", "velocity_ok",
                    "immortal", "error_boxes", "score_ui",
                    "playlist",
                    "fps_median", "fps_p05", "dropped_frames", "frame_samples", "target_frame_rate",
                    "start_utc", "end_utc", "start_dsp_s", "notes"
                };
                string[] values =
                {
                    request.participantId,
                    request.sessionNumber.ToString(CultureInfo.InvariantCulture),
                    ExperimentSessionRequest.ToShortCode(config.condition),
                    config.label,
                    Application.version,
                    Application.unityVersion,
                    Application.productName,
                    SystemInfo.deviceModel,
                    SystemInfo.deviceType.ToString(),
                    Application.platform.ToString(),
                    ExperimentMode.Seed.ToString(CultureInfo.InvariantCulture),
                    F(config.exposureSeconds, 2),
                    F(exposedSeconds, 3),
                    endReason,
                    trackRestarts.ToString(CultureInfo.InvariantCulture),
                    playlistLoops.ToString(CultureInfo.InvariantCulture),
                    _eventCount.ToString(CultureInfo.InvariantCulture),
                    F(config.masterVolumeDb, 2),
                    F(config.musicVolumeDb, 2),
                    F(config.sfxVolumeDb, 2),
                    F(config.ballLifeTime, 3),
                    F(config.arcHeight, 3),
                    F(config.timingPerfectThreshold, 3),
                    F(config.timingOkThreshold, 3),
                    F(config.velocityHomeRunThreshold, 3),
                    F(config.velocityOkThreshold, 3),
                    config.immortal ? "1" : "0",
                    config.useErrorBoxes ? "1" : "0",
                    config.showScoreUi ? "1" : "0",
                    DescribePlaylist(config),
                    frames != null ? F(frames.MedianFps, 2) : "",
                    frames != null ? F(frames.Percentile05Fps, 2) : "",
                    frames != null ? frames.DroppedFrames.ToString(CultureInfo.InvariantCulture) : "",
                    frames != null ? frames.SampleCount.ToString(CultureInfo.InvariantCulture) : "",
                    Application.targetFrameRate.ToString(CultureInfo.InvariantCulture),
                    _startUtc.ToString("o", CultureInfo.InvariantCulture),
                    DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture),
                    F(_startDsp, 4),
                    request.notes
                };

                StringBuilder head = new StringBuilder();
                StringBuilder row = new StringBuilder();
                for (int i = 0; i < keys.Length; i++)
                {
                    if (i > 0) { head.Append(','); row.Append(','); }
                    head.Append(keys[i]);
                    row.Append(Csv(i < values.Length ? values[i] : ""));
                }
                w.WriteLine(head.ToString());
                w.WriteLine(row.ToString());
            }
        }
        catch (Exception e)
        {
            LastError = e.Message;
            Debug.LogError("[Experimento] Falha ao gravar os metadados: " + e);
        }
    }

    private static string DescribePlaylist(ExperimentConfig config)
    {
        if (config.playlist == null || config.playlist.Count == 0) return "";
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < config.playlist.Count; i++)
        {
            ExperimentTrack t = config.playlist[i];
            if (i > 0) sb.Append(" | ");
            sb.Append(t.songObjectName).Append(" (").Append(t.bpm).Append(" BPM, skip ")
              .Append(t.beatSkipFactor).Append(')');
            if (t.isReserve) sb.Append(" [reserva]");
        }
        return sb.ToString();
    }

    // ------------------------------------------------------------------
    // utilidades
    // ------------------------------------------------------------------

    private string Clocks()
    {
        long unixMs = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        return string.Concat(
            unixMs.ToString(CultureInfo.InvariantCulture), ",",
            F(AudioSettings.dspTime, 4), ",",
            F(SessionSeconds, 4));
    }

    /// <summary>
    /// Formata um numero com casas fixas. NaN vira campo vazio: e assim que se
    /// diz "nao se aplica" num CSV sem inventar um sentinela numerico que
    /// depois entra por engano em alguma media.
    /// </summary>
    private static string F(double value, int decimals)
    {
        if (double.IsNaN(value) || double.IsInfinity(value)) return "";
        return value.ToString("F" + decimals, CultureInfo.InvariantCulture);
    }

    private static string Csv(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        bool needsQuotes = value.IndexOf(',') >= 0 || value.IndexOf('"') >= 0 ||
                           value.IndexOf('\n') >= 0 || value.IndexOf('\r') >= 0;
        if (!needsQuotes) return value;
        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }

    private static string Sanitize(string value)
    {
        if (string.IsNullOrEmpty(value)) return "P00";
        StringBuilder sb = new StringBuilder(value.Length);
        foreach (char c in value)
        {
            if (char.IsLetterOrDigit(c) || c == '-' || c == '_') sb.Append(c);
        }
        return sb.Length > 0 ? sb.ToString() : "P00";
    }
}
