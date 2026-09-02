using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Conduz uma sessao experimental do inicio ao fim.
///
/// Substitui o fluxo normal do jogo: nao ha lobby, nem escolha de musica, nem
/// tela de vitoria ou de derrota. A exposicao dura exatamente o tempo previsto
/// no protocolo, encadeando a playlist da condicao, e termina por cronometro,
/// nunca por fim de faixa.
///
/// Todo instante relevante vai para o <see cref="SessionLogger"/>, que e o que
/// permite alinhar depois os dados do jogo com os registros de VFC e EDA.
/// </summary>
[DefaultExecutionOrder(-50)]
public class ExperimentSessionManager : MonoBehaviour
{
    public static ExperimentSessionManager Instance { get; private set; }

    private const float TrackStartTimeoutSeconds = 4f;

    private ExperimentConfig _config;
    private ExperimentSessionRequest _request;
    private SessionLogger _logger;
    private FrameRateMonitor _frames;

    private GameObject _lobbyObject;
    private GameObject _inGameObject;
    private GameObject _deadObject;
    private readonly Dictionary<string, ServeManager> _songs = new Dictionary<string, ServeManager>();
    private AudioMixer _mixer;

    private double _exposureStart;
    private double _pausedAccumulated;
    private double _pauseStartedAt;
    private double _finalElapsed;

    private bool _paused;
    private bool _manualPause;
    private bool _stopRequested;
    private bool _restartRequested;
    private string _endReason = "duracao_completa";

    private int _trackRestarts;
    private int _playlistLoops;
    private int _missingTracks;

    // ------------------------------------------------------------------
    // estado exposto para a interface do pesquisador
    // ------------------------------------------------------------------

    public bool IsRunning { get; private set; }
    public bool IsFinished { get; private set; }
    public bool IsPaused { get { return _paused; } }
    public string CurrentTrackName { get; private set; }
    public int TrackRestarts { get { return _trackRestarts; } }
    public int MissingTracks { get { return _missingTracks; } }
    public ExperimentConfig Config { get { return _config; } }
    public SessionLogger Logger { get { return _logger; } }

    public double ExposureElapsed
    {
        get
        {
            if (!IsRunning) return _finalElapsed;
            double paused = _pausedAccumulated;
            if (_paused) paused += Time.realtimeSinceStartupAsDouble - _pauseStartedAt;
            return Time.realtimeSinceStartupAsDouble - _exposureStart - paused;
        }
    }

    public double ExposureRemaining
    {
        get { return _config != null ? Mathf.Max(0f, (float)(_config.exposureSeconds - ExposureElapsed)) : 0.0; }
    }

    // ------------------------------------------------------------------
    // ciclo de vida
    // ------------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        _logger = GetComponent<SessionLogger>();
        _frames = GetComponent<FrameRateMonitor>();
    }

    private void Start()
    {
        if (!ExperimentMode.IsActive)
        {
            enabled = false;
            return;
        }
        StartCoroutine(RunSession());
    }

    private IEnumerator RunSession()
    {
        // Espera a cena terminar de acordar antes de mexer nela.
        yield return null;
        yield return null;

        _config = ExperimentMode.Config;
        _request = ExperimentMode.Request;

        ResolveSceneObjects();
        ResolveMixer();

        if (_logger != null) _logger.Begin(_request, _config);
        Marker("SESSION_START", _config.label);
        Marker("BUILD", Application.productName + " " + Application.version + " / Unity " + Application.unityVersion);
        Marker("SEED", ExperimentMode.Seed.ToString());

        LockAudio();
        PrepareStateObjects();
        ApplyVisualPreset();

        if (_frames != null) _frames.BeginSampling();

        _exposureStart = Time.realtimeSinceStartupAsDouble;
        IsRunning = true;
        Marker("EXPOSURE_START", "alvo " + _config.exposureSeconds.ToString("F0") + " s");

        if (_config.usesSoundtrack && _config.playlist != null && _config.playlist.Count > 0)
            yield return PlayPlaylist();
        else
            yield return WaitOutTimer();

        _finalElapsed = ExposureElapsed;
        IsRunning = false;
        if (_frames != null) _frames.EndSampling();

        Marker("EXPOSURE_END", _endReason + " | " + _finalElapsed.ToString("F2") + " s");

        Cleanup();
        Marker("SESSION_END", "");

        if (_logger != null)
        {
            _logger.WriteMeta(_request, _config, _frames, _endReason,
                              _finalElapsed, _trackRestarts, _playlistLoops);
            _logger.Close();
        }

        IsFinished = true;
        Debug.Log("[Experimento] Sessao concluida (" + _endReason + "). Arquivos em: " +
                  (_logger != null ? _logger.Directory : "(sem registro)"));
    }

    // ------------------------------------------------------------------
    // resolucao da cena
    // ------------------------------------------------------------------

    private void ResolveSceneObjects()
    {
        WristMenuManager wrist = FindFirstObjectByType<WristMenuManager>(FindObjectsInactive.Include);
        if (wrist != null)
        {
            _lobbyObject = wrist.lobbyObject;
            _inGameObject = wrist.inGameObject;
            _deadObject = wrist.deadObject;
        }

        if (_inGameObject == null)
        {
            Debug.LogError("[Experimento] Nao encontrei o objeto de estado do jogo. " +
                           "Verifique se o WristMenuManager tem lobbyObject, inGameObject e " +
                           "deadObject atribuidos.");
            return;
        }

        ServeManager[] managers = _inGameObject.GetComponentsInChildren<ServeManager>(true);
        for (int i = 0; i < managers.Length; i++)
        {
            ServeManager sm = managers[i];
            string key = sm.transform.parent != null ? sm.transform.parent.name : sm.name;
            if (!_songs.ContainsKey(key)) _songs.Add(key, sm);
        }
        Debug.Log("[Experimento] " + _songs.Count + " musicas localizadas na cena.");
    }

    private void ResolveMixer()
    {
        AudioSettingsManager settings = FindFirstObjectByType<AudioSettingsManager>(FindObjectsInactive.Include);
        if (settings != null && settings.mainMixer != null)
        {
            _mixer = settings.mainMixer;
            return;
        }
        // Segunda tentativa: pelo grupo de mixagem de qualquer musica.
        foreach (KeyValuePair<string, ServeManager> pair in _songs)
        {
            AudioSource src = FindMusicSource(pair.Value);
            if (src != null && src.outputAudioMixerGroup != null)
            {
                _mixer = src.outputAudioMixerGroup.audioMixer;
                return;
            }
        }
        Debug.LogWarning("[Experimento] AudioMixer nao localizado; o volume nao pode ser travado.");
    }

    private static AudioSource FindMusicSource(ServeManager serve)
    {
        if (serve == null) return null;
        Transform songRoot = serve.transform.parent != null ? serve.transform.parent : serve.transform;
        AudioSource[] sources = songRoot.GetComponentsInChildren<AudioSource>(true);
        return sources.Length > 0 ? sources[0] : null;
    }

    // ------------------------------------------------------------------
    // preparo
    // ------------------------------------------------------------------

    private void LockAudio()
    {
        // Um unico caminho de audio: o mixer. AudioListener fica em ganho
        // unitario e os controles de volume ficam suspensos durante a sessao.
        AudioListener.volume = 1f;
        AudioListener.pause = false;
        SetMixerDb("MasterVolume", _config.masterVolumeDb);
        SetMixerDb("MusicVolume", _config.musicVolumeDb);
        SetMixerDb("SFXVolume", _config.sfxVolumeDb);
        Marker("AUDIO_LOCKED", string.Format("master {0} dB, music {1} dB, sfx {2} dB",
            _config.masterVolumeDb, _config.musicVolumeDb, _config.sfxVolumeDb));
    }

    private void SetMixerDb(string parameter, float db)
    {
        if (_mixer == null) return;
        _mixer.SetFloat(parameter, db);
    }

    private void PrepareStateObjects()
    {
        bool needsInGame = _config.usesGameplay || _config.usesSoundtrack;

        if (_deadObject != null) _deadObject.SetActive(false);
        if (_inGameObject != null) _inGameObject.SetActive(needsInGame);
        if (_lobbyObject != null) _lobbyObject.SetActive(!needsInGame);

        DeactivateAllSongs();
    }

    private void DeactivateAllSongs()
    {
        foreach (KeyValuePair<string, ServeManager> pair in _songs)
        {
            ServeManager sm = pair.Value;
            if (sm == null) continue;
            Transform songRoot = sm.transform.parent != null ? sm.transform.parent : sm.transform;
            songRoot.gameObject.SetActive(false);
        }
    }

    private void ApplyVisualPreset()
    {
        if (_inGameObject == null) return;
        AudioReactLight[] lights = _inGameObject.GetComponentsInChildren<AudioReactLight>(true);
        for (int i = 0; i < lights.Length; i++)
        {
            lights[i].cycleHue = _config.lightHueCycle;
            lights[i].fixedColor = _config.lightColor;
            lights[i].minIntensity = _config.lightMinIntensity;
            lights[i].maxIntensity = _config.lightMaxIntensity;
        }
        if (lights.Length > 0)
            Marker("VISUAL_PRESET", lights.Length + " luzes ajustadas, cicloDeMatiz=" + _config.lightHueCycle);
    }

    // ------------------------------------------------------------------
    // execucao
    // ------------------------------------------------------------------

    private IEnumerator PlayPlaylist()
    {
        int index = 0;
        while (!_stopRequested && ExposureElapsed < _config.exposureSeconds)
        {
            int count = _config.playlist.Count;
            if (count == 0) break;

            int position;
            if (index < count)
            {
                position = index;
            }
            else if (_config.repeatLastTrackWhenShort)
            {
                // Mantem a exposicao no ponto mais intenso do arco em vez de
                // voltar ao andamento inicial.
                position = count - 1;
                _playlistLoops++;
                Marker("TRACK_REPEAT_LAST", "repeticao " + _playlistLoops + " de " +
                       _config.playlist[position].songObjectName);
            }
            else
            {
                position = index % count;
                if (position == 0)
                {
                    _playlistLoops++;
                    Marker("PLAYLIST_LOOP", "volta " + _playlistLoops +
                           " - a playlist acabou antes do cronometro");
                }
            }

            yield return PlayTrack(_config.playlist[position]);
            index++;
        }
        if (_stopRequested && _endReason == "duracao_completa") _endReason = "interrompida";
    }

    private IEnumerator PlayTrack(ExperimentTrack track)
    {
        ServeManager serve;
        if (!_songs.TryGetValue(track.songObjectName, out serve) || serve == null)
        {
            _missingTracks++;
            Marker("TRACK_MISSING", track.songObjectName);
            Debug.LogError("[Experimento] Faixa ausente na cena: '" + track.songObjectName + "'.");
            yield return null;
            yield break;
        }

        GameObject songObject = serve.transform.parent != null
            ? serve.transform.parent.gameObject
            : serve.gameObject;

        serve.ConfigureForExperiment(_config, track);
        serve.enabled = _config.usesGameplay;

        CurrentTrackName = track.songObjectName;
        songObject.SetActive(true);

        Marker("TRACK_START", string.Format("{0} | {1} BPM | skip {2} | {3:F1} bolas/min",
            track.songObjectName, track.bpm, track.beatSkipFactor, track.NominalBallsPerMinute));

        AudioSource source = FindMusicSource(serve);
        yield return StartCoroutine(WaitForPlayback(source));

        while (!_stopRequested && ExposureElapsed < _config.exposureSeconds)
        {
            UpdatePauseState();
            ApplyEndFade();

            if (_restartRequested)
            {
                _restartRequested = false;
                _trackRestarts++;
                Marker("TRACK_RESTART", track.songObjectName + " | reinicio " + _trackRestarts);
                songObject.SetActive(false);
                yield return null;
                serve.ConfigureForExperiment(_config, track);
                serve.enabled = _config.usesGameplay;
                songObject.SetActive(true);
                source = FindMusicSource(serve);
                yield return StartCoroutine(WaitForPlayback(source));
                continue;
            }

            if (source == null) break;
            if (!source.isPlaying && !_paused) break; // faixa chegou ao fim

            yield return null;
        }

        Marker("TRACK_END", track.songObjectName);
        songObject.SetActive(false);
        CurrentTrackName = "";
    }

    private IEnumerator WaitForPlayback(AudioSource source)
    {
        if (source == null) yield break;
        double deadline = Time.realtimeSinceStartupAsDouble + TrackStartTimeoutSeconds;
        while (!source.isPlaying && Time.realtimeSinceStartupAsDouble < deadline && !_stopRequested)
            yield return null;

        if (!source.isPlaying && !_stopRequested)
        {
            source.Play();
            Marker("TRACK_FORCED_PLAY", source.clip != null ? source.clip.name : "(sem clipe)");
        }
    }

    private IEnumerator WaitOutTimer()
    {
        // C4 e a familiarizacao nao reproduzem playlist: aqui o aplicativo e
        // apenas o cronometro e a fonte de marcadores da condicao.
        while (!_stopRequested && ExposureElapsed < _config.exposureSeconds)
        {
            UpdatePauseState();
            yield return null;
        }
        if (_stopRequested && _endReason == "duracao_completa") _endReason = "interrompida";
    }

    private void ApplyEndFade()
    {
        if (_config.endFadeSeconds <= 0f || _mixer == null) return;
        double remaining = _config.exposureSeconds - ExposureElapsed;
        if (remaining > _config.endFadeSeconds) return;
        float k = Mathf.Clamp01((float)(remaining / _config.endFadeSeconds));
        SetMixerDb("MasterVolume", Mathf.Lerp(-80f, _config.masterVolumeDb, k));
    }

    private void UpdatePauseState()
    {
        bool wanted = _manualPause || WristMenuManager.IsMenuOpen;
        if (wanted == _paused) return;

        if (wanted)
        {
            _paused = true;
            _pauseStartedAt = Time.realtimeSinceStartupAsDouble;
            PauseCurrentAudio(true);
            Marker("PAUSE_START", _manualPause ? "pesquisador" : "menu do participante");
        }
        else
        {
            _paused = false;
            _pausedAccumulated += Time.realtimeSinceStartupAsDouble - _pauseStartedAt;
            PauseCurrentAudio(false);
            Marker("PAUSE_END", "pausa acumulada " + _pausedAccumulated.ToString("F2") + " s");
        }
    }

    private void PauseCurrentAudio(bool pause)
    {
        if (string.IsNullOrEmpty(CurrentTrackName)) return;
        ServeManager serve;
        if (!_songs.TryGetValue(CurrentTrackName, out serve)) return;
        AudioSource src = FindMusicSource(serve);
        if (src == null) return;
        if (pause) src.Pause();
        else src.UnPause();
    }

    private void Cleanup()
    {
        DeactivateAllSongs();
        SetMixerDb("MasterVolume", _config.masterVolumeDb);
        if (_deadObject != null) _deadObject.SetActive(false);
        if (_inGameObject != null) _inGameObject.SetActive(false);
        if (_lobbyObject != null) _lobbyObject.SetActive(true);
    }

    // ------------------------------------------------------------------
    // controles do pesquisador e retorno do jogo
    // ------------------------------------------------------------------

    /// <summary>
    /// Interrompe a exposicao imediatamente. Garantia prevista na Secao 6 do TCLE.
    /// </summary>
    public void RequestStop(string reason)
    {
        if (_stopRequested) return;
        _stopRequested = true;
        _endReason = string.IsNullOrEmpty(reason) ? "interrompida" : reason;
        Marker("INTERRUPTION", _endReason);
        Debug.LogWarning("[Experimento] Interrupcao solicitada: " + _endReason);
    }

    public void SetManualPause(bool paused)
    {
        _manualPause = paused;
    }

    /// <summary>
    /// Chamado pelo GameScoreManager quando o participante esgota as caixas de erro.
    /// A faixa recomeca e o cronometro continua correndo, para que a exposicao
    /// mantenha a mesma duracao nas quatro condicoes.
    /// </summary>
    public void NotifyPlayerFailed()
    {
        if (!IsRunning) return;
        if (!_config.restartTrackOnFailure) return;
        _restartRequested = true;
    }

    private void Marker(string name, string detail)
    {
        if (_logger != null) _logger.Marker(name, detail);
    }
}
