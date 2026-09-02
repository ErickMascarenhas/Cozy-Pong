using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Transforma a lista de musicas do lobby em uma lista de condicoes e encadeia
/// a playlist da condicao escolhida, faixa a faixa, pela tela de conclusao.
///
/// Fluxo: o jogador escolhe uma condicao no lobby e vai direto para a primeira
/// faixa da playlist. Ao terminar, chega a tela de conclusao, onde o botao que
/// antes repetia a musica passa a levar a proxima faixa; voltar ao lobby
/// continua disponivel e encerra a playlist. Qualquer escolha feita no lobby
/// recomeca da primeira faixa.
///
/// Nada disso exige alteracao na cena. Os objetos de estado, os carregadores de
/// transicao e os botoes ja existentes sao localizados em tempo de execucao, e
/// as entradas da lista sao clonadas de uma entrada de musica, o que preserva o
/// leiaute e o estilo originais.
///
/// Este componente nao existe durante uma sessao experimental armada: la quem
/// conduz a exposicao e o <see cref="ExperimentSessionManager"/>, sem lobby e
/// sem tela de conclusao entre as faixas.
/// </summary>
public class PlaylistNavigator : MonoBehaviour
{
    public static PlaylistNavigator Instance { get; private set; }

    private static readonly ExperimentCondition[] MenuConditions =
    {
        ExperimentCondition.C1_CozyPongCalm,
        ExperimentCondition.C2_CozyPongLively,
        ExperimentCondition.C3_SoundtrackOnly,
        ExperimentCondition.C4_ExternalMeditation
    };

    // --- cena -------------------------------------------------------
    private GameObject _lobbyObject;
    private GameObject _inGameObject;
    private GameObject _deadObject;
    private GameObject _winObject;
    private Transform _songsRoot;
    private readonly Dictionary<string, ServeManager> _songs = new Dictionary<string, ServeManager>();
    private SongLoader _transitionTemplate;
    private SongLoader _winToLobbyLoader;
    private Transform _menuContent;

    // --- botoes da tela de conclusao --------------------------------
    private Button _advanceButton;
    private TextMeshProUGUI _advanceLabel;
    private string _advanceOriginalText;
    private Button _lobbyButton;

    // --- estado -----------------------------------------------------
    private ExperimentCondition _condition = ExperimentCondition.None;
    private int _index;
    private bool _running;
    private bool _busy;
    private bool _winWasActive;
    private bool _lobbyWasActive;
    private bool _ready;

    public bool IsRunning { get { return _running; } }
    public int TrackIndex { get { return _index; } }
    public ExperimentCondition Condition { get { return _condition; } }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        StartCoroutine(SetUp());
    }

    // ------------------------------------------------------------------
    // montagem
    // ------------------------------------------------------------------

    private IEnumerator SetUp()
    {
        yield return null;
        yield return null;

        if (!ResolveScene())
        {
            enabled = false;
            yield break;
        }

        BuildConditionMenu();
        _ready = true;
    }

    private bool ResolveScene()
    {
        WristMenuManager wrist = FindFirstObjectByType<WristMenuManager>(FindObjectsInactive.Include);
        if (wrist == null || wrist.inGameObject == null || wrist.lobbyObject == null)
        {
            Debug.LogWarning("[Playlist] WristMenuManager sem os objetos de estado atribuidos; " +
                             "o menu de condicoes nao sera montado.");
            return false;
        }

        _lobbyObject = wrist.lobbyObject;
        _inGameObject = wrist.inGameObject;
        _deadObject = wrist.deadObject;

        ServeManager[] managers = _inGameObject.GetComponentsInChildren<ServeManager>(true);
        for (int i = 0; i < managers.Length; i++)
        {
            string key = managers[i].transform.parent != null
                ? managers[i].transform.parent.name
                : managers[i].name;
            if (!_songs.ContainsKey(key)) _songs.Add(key, managers[i]);
            if (_songsRoot == null && managers[i].transform.parent != null)
                _songsRoot = managers[i].transform.parent.parent;
        }

        ResolveWinObject();
        ResolveLoaders();
        ResolveWinButtons();

        SongSelectionMenuUI entry = FirstSceneObject<SongSelectionMenuUI>();
        if (entry != null) _menuContent = entry.transform.parent;

        return _menuContent != null && _songs.Count > 0;
    }

    /// <summary>
    /// A tela de conclusao e o unico objeto de estado que contem a tela de
    /// resultados, entao basta subir a partir dela ate o irmao do lobby.
    /// </summary>
    private void ResolveWinObject()
    {
        ScoreResultUI result = FirstSceneObject<ScoreResultUI>();
        if (result == null || _lobbyObject == null || _lobbyObject.transform.parent == null) return;

        Transform t = result.transform;
        while (t.parent != null && t.parent != _lobbyObject.transform.parent) t = t.parent;
        if (t.parent == _lobbyObject.transform.parent) _winObject = t.gameObject;
    }

    /// <summary>
    /// Localiza os carregadores de transicao ja existentes na cena. Reaproveitar
    /// as listas deles e mais seguro que repetir aqui quais objetos ligar e
    /// desligar: se a cena mudar, a transicao acompanha.
    /// </summary>
    private void ResolveLoaders()
    {
        SongLoader[] loaders = Resources.FindObjectsOfTypeAll<SongLoader>();
        for (int i = 0; i < loaders.Length; i++)
        {
            SongLoader l = loaders[i];
            if (!l.gameObject.scene.IsValid()) continue;

            if (_transitionTemplate == null && l.songObject != null)
                _transitionTemplate = l;

            if (_winToLobbyLoader == null &&
                Contains(l.ItemsToDisable, _winObject) &&
                Contains(l.ItemsToEnable, _lobbyObject))
                _winToLobbyLoader = l;
        }
    }

    private void ResolveWinButtons()
    {
        if (_winObject == null) return;

        Button[] buttons = _winObject.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            SongLoader target = FirstPersistentLoader(buttons[i]);
            if (target == null) continue;
            if (target.parentToDisableChildren != null) _lobbyButton = buttons[i];
            else _advanceButton = buttons[i];
        }
        if (_advanceButton != null)
        {
            _advanceLabel = _advanceButton.GetComponentInChildren<TextMeshProUGUI>(true);
            if (_advanceLabel != null) _advanceOriginalText = _advanceLabel.text;
        }
    }

    private static SongLoader FirstPersistentLoader(Button button)
    {
        int n = button.onClick.GetPersistentEventCount();
        for (int i = 0; i < n; i++)
        {
            SongLoader l = button.onClick.GetPersistentTarget(i) as SongLoader;
            if (l != null) return l;
        }
        return null;
    }

    private static bool Contains(GameObject[] list, GameObject item)
    {
        if (list == null || item == null) return false;
        for (int i = 0; i < list.Length; i++) if (list[i] == item) return true;
        return false;
    }

    private static T FirstSceneObject<T>() where T : Component
    {
        T[] all = Resources.FindObjectsOfTypeAll<T>();
        for (int i = 0; i < all.Length; i++)
            if (all[i].gameObject.scene.IsValid()) return all[i];
        return null;
    }

    // ------------------------------------------------------------------
    // menu de condicoes
    // ------------------------------------------------------------------

    private void BuildConditionMenu()
    {
        if (_menuContent == null || _menuContent.childCount == 0) return;

        GameObject template = _menuContent.GetChild(0).gameObject;
        List<GameObject> originals = new List<GameObject>();
        for (int i = 0; i < _menuContent.childCount; i++)
            originals.Add(_menuContent.GetChild(i).gameObject);

        for (int i = 0; i < MenuConditions.Length; i++)
            CreateEntry(template, MenuConditions[i], i);

        // As musicas individuais saem da lista: no experimento a escolha e da
        // condicao, nunca da faixa.
        for (int i = 0; i < originals.Count; i++) originals[i].SetActive(false);

        RenamePanelTitle();
    }

    private void CreateEntry(GameObject template, ExperimentCondition condition, int order)
    {
        ExperimentConfig config = ExperimentConfigs.Get(condition);
        GameObject entry = Instantiate(template, _menuContent);
        entry.name = "Condition " + ExperimentSessionRequest.ToShortCode(condition);
        entry.transform.SetSiblingIndex(order);
        entry.SetActive(true);

        SongSelectionMenuUI stats = entry.GetComponent<SongSelectionMenuUI>();
        if (stats != null) Destroy(stats);

        Button button = entry.GetComponent<Button>();
        if (button != null)
        {
            DisablePersistentCalls(button);
            button.onClick.RemoveAllListeners();
            bool playable = config.playlist != null && config.playlist.Count > 0;
            if (playable)
            {
                ExperimentCondition captured = condition;
                button.onClick.AddListener(delegate { StartCondition(captured); });
            }
            button.interactable = playable;
        }

        SetChildText(entry, "Title", config.label);
        SetChildText(entry, "Author", DescribePlaylist(config));
        SetChildText(entry, "Score", "");
        SetChildText(entry, "Grade", "");
    }

    private static string DescribePlaylist(ExperimentConfig config)
    {
        if (config.playlist == null || config.playlist.Count == 0)
            return "conduzida fora deste aplicativo";

        int first = config.playlist[0].bpm;
        int last = config.playlist[config.playlist.Count - 1].bpm;
        int minutes = Mathf.RoundToInt(config.exposureSeconds / 60f);
        string activity = config.usesGameplay ? "faixas" : "faixas, so audio";
        return string.Format("{0} {1} - {2} a {3} BPM - {4} min",
                             config.playlist.Count, activity, first, last, minutes);
    }

    private static void DisablePersistentCalls(Button button)
    {
        int n = button.onClick.GetPersistentEventCount();
        for (int i = 0; i < n; i++)
            button.onClick.SetPersistentListenerState(i, UnityEventCallState.Off);
    }

    private static void EnablePersistentCalls(Button button)
    {
        int n = button.onClick.GetPersistentEventCount();
        for (int i = 0; i < n; i++)
            button.onClick.SetPersistentListenerState(i, UnityEventCallState.RuntimeOnly);
    }

    private static void SetChildText(GameObject root, string childName, string value)
    {
        Transform t = root.transform.Find(childName);
        if (t == null) return;
        TextMeshProUGUI label = t.GetComponent<TextMeshProUGUI>();
        if (label != null) label.text = value;
    }

    private void RenamePanelTitle()
    {
        Canvas canvas = _menuContent.GetComponentInParent<Canvas>();
        if (canvas == null) return;
        TextMeshProUGUI[] labels = canvas.GetComponentsInChildren<TextMeshProUGUI>(true);
        for (int i = 0; i < labels.Length; i++)
        {
            if (labels[i].transform.IsChildOf(_menuContent)) continue;
            string text = labels[i].text != null ? labels[i].text.Trim() : "";
            if (text.Equals("Songs", System.StringComparison.OrdinalIgnoreCase))
            {
                labels[i].text = "Conditions";
                return;
            }
        }
    }

    // ------------------------------------------------------------------
    // playlist
    // ------------------------------------------------------------------

    public void StartCondition(ExperimentCondition condition)
    {
        if (_busy || !_ready) return;

        ExperimentMode.BeginPractice(condition);
        if (ExperimentMode.Config == null) return;

        _condition = condition;
        _index = 0;
        _running = true;
        StartCoroutine(GoToTrack(0));
    }

    private IEnumerator GoToTrack(int index)
    {
        ExperimentConfig config = ExperimentMode.Config;
        if (config == null || config.playlist == null || index >= config.playlist.Count)
        {
            ReturnToLobby();
            yield break;
        }

        ExperimentTrack track = config.playlist[index];
        ServeManager serve;
        if (!_songs.TryGetValue(track.songObjectName, out serve) || serve == null)
        {
            Debug.LogError("[Playlist] Faixa ausente na cena: '" + track.songObjectName + "'.");
            ReturnToLobby();
            yield break;
        }

        _busy = true;
        _index = index;

        CanvasGroup fade = _transitionTemplate != null ? _transitionTemplate.transitionCanvas : null;
        float fadeDuration = _transitionTemplate != null ? _transitionTemplate.fadeDuration : 0.5f;
        float darkDuration = _transitionTemplate != null ? _transitionTemplate.stayDarkDuration : 1f;

        yield return Fade(fade, 0f, 1f, fadeDuration);

        if (_transitionTemplate != null && _transitionTemplate.ItemsToDisable != null)
            foreach (GameObject go in _transitionTemplate.ItemsToDisable)
                if (go != null) go.SetActive(false);

        if (_winObject != null) _winObject.SetActive(false);
        if (_deadObject != null) _deadObject.SetActive(false);

        DeactivateAllSongs();

        serve.ConfigureForExperiment(config, track);
        serve.enabled = config.usesGameplay;

        if (_transitionTemplate != null && _transitionTemplate.ItemsToEnable != null)
            foreach (GameObject go in _transitionTemplate.ItemsToEnable)
                if (go != null) go.SetActive(true);
        if (_inGameObject != null) _inGameObject.SetActive(true);

        GameObject songObject = serve.transform.parent != null
            ? serve.transform.parent.gameObject
            : serve.gameObject;
        songObject.SetActive(true);

        yield return new WaitForSeconds(darkDuration);
        yield return Fade(fade, 1f, 0f, fadeDuration);

        _busy = false;

        // Sem jogabilidade nao ha ServeManager para avisar do fim da faixa, e
        // tampouco faz sentido mostrar uma tela de resultados vazia: aqui o
        // encadeamento e feito direto.
        if (!config.usesGameplay) StartCoroutine(WatchAudioOnlyTrack(serve, songObject));
    }

    private IEnumerator WatchAudioOnlyTrack(ServeManager serve, GameObject songObject)
    {
        AudioSource source = null;
        Transform root = serve.transform.parent != null ? serve.transform.parent : serve.transform;
        AudioSource[] sources = root.GetComponentsInChildren<AudioSource>(true);
        if (sources.Length > 0) source = sources[0];
        if (source == null) yield break;

        float deadline = Time.time + 4f;
        while (!source.isPlaying && Time.time < deadline) yield return null;
        while (source != null && source.isPlaying && _running && songObject.activeInHierarchy) yield return null;

        if (!_running || !songObject.activeInHierarchy) yield break;
        Advance();
    }

    private void DeactivateAllSongs()
    {
        foreach (KeyValuePair<string, ServeManager> pair in _songs)
        {
            ServeManager sm = pair.Value;
            if (sm == null) continue;
            Transform root = sm.transform.parent != null ? sm.transform.parent : sm.transform;
            root.gameObject.SetActive(false);
        }
    }

    private static IEnumerator Fade(CanvasGroup group, float from, float to, float duration)
    {
        if (group == null || duration <= 0f) yield break;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            group.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        group.alpha = to;
    }

    /// <summary>Avanca para a proxima faixa, ou encerra se esta era a ultima.</summary>
    public void Advance()
    {
        if (_busy || !_running) return;
        ExperimentConfig config = ExperimentMode.Config;
        if (config == null || config.playlist == null) { ReturnToLobby(); return; }

        if (_index + 1 < config.playlist.Count) StartCoroutine(GoToTrack(_index + 1));
        else ReturnToLobby();
    }

    public void ReturnToLobby()
    {
        StopPlaylist();

        if (_winToLobbyLoader != null)
        {
            _winToLobbyLoader.LoadSong();
            return;
        }

        // Sem o carregador da cena, faz a volta manualmente para nao deixar o
        // jogador preso na tela de conclusao.
        StartCoroutine(ManualReturnToLobby());
    }

    private IEnumerator ManualReturnToLobby()
    {
        CanvasGroup fade = _transitionTemplate != null ? _transitionTemplate.transitionCanvas : null;
        float fadeDuration = _transitionTemplate != null ? _transitionTemplate.fadeDuration : 0.5f;

        yield return Fade(fade, 0f, 1f, fadeDuration);
        DeactivateAllSongs();
        if (_winObject != null) _winObject.SetActive(false);
        if (_deadObject != null) _deadObject.SetActive(false);
        if (_inGameObject != null) _inGameObject.SetActive(false);
        if (_lobbyObject != null) _lobbyObject.SetActive(true);
        yield return Fade(fade, 1f, 0f, fadeDuration);
    }

    private void StopPlaylist()
    {
        _running = false;
        _index = 0;
        _condition = ExperimentCondition.None;
        ExperimentMode.EndPractice();
        RestoreAdvanceButton();
    }

    // ------------------------------------------------------------------
    // tela de conclusao
    // ------------------------------------------------------------------

    private void Update()
    {
        if (!_ready) return;

        if (_winObject != null)
        {
            bool active = _winObject.activeInHierarchy;
            if (active && !_winWasActive) OnWinScreenOpened();
            _winWasActive = active;
        }

        if (_lobbyObject != null)
        {
            bool active = _lobbyObject.activeInHierarchy;
            if (active && !_lobbyWasActive && _running && !_busy) StopPlaylist();
            _lobbyWasActive = active;
        }
    }

    private void OnWinScreenOpened()
    {
        if (_advanceButton == null) return;

        if (!_running)
        {
            RestoreAdvanceButton();
            return;
        }

        ExperimentConfig config = ExperimentMode.Config;
        bool hasNext = config != null && config.playlist != null && _index + 1 < config.playlist.Count;

        DisablePersistentCalls(_advanceButton);
        _advanceButton.onClick.RemoveAllListeners();
        _advanceButton.onClick.AddListener(Advance);

        if (_advanceLabel != null)
        {
            _advanceLabel.text = hasNext
                ? "Next: " + config.playlist[_index + 1].songObjectName
                : "Finish playlist";
        }

        if (_lobbyButton != null)
        {
            _lobbyButton.onClick.RemoveAllListeners();
            _lobbyButton.onClick.AddListener(StopPlaylist);
        }
    }

    private void RestoreAdvanceButton()
    {
        if (_advanceButton != null)
        {
            _advanceButton.onClick.RemoveAllListeners();
            EnablePersistentCalls(_advanceButton);
            if (_advanceLabel != null && !string.IsNullOrEmpty(_advanceOriginalText))
                _advanceLabel.text = _advanceOriginalText;
        }
        if (_lobbyButton != null) _lobbyButton.onClick.RemoveAllListeners();
    }
}
