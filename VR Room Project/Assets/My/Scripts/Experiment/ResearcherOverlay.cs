using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Painel do pesquisador, desenhado na tela do operador.
///
/// Nao aparece dentro do HMD: e a tela que o pesquisador acompanha, no editor
/// ou em uma build de computador. Mostra o andamento da sessao e oferece os
/// dois controles que o TCLE exige que existam, pausa e interrupcao imediata.
///
/// Em C3 e C4 tambem escurece a tela, para que o monitor nao vire um estimulo
/// visual nao previsto no protocolo.
///
/// Atalhos: P pausa e retoma, F10 interrompe a exposicao.
/// </summary>
public class ResearcherOverlay : MonoBehaviour
{
    private const int PanelWidth = 430;
    private const int PanelHeight = 250;

    private GUIStyle _panelStyle;
    private GUIStyle _titleStyle;
    private GUIStyle _lineStyle;
    private Texture2D _panelTexture;
    private Texture2D _blackTexture;
    private bool _stylesReady;

    private void Update()
    {
        if (!ExperimentMode.IsActive) return;
        ExperimentSessionManager session = ExperimentSessionManager.Instance;
        if (session == null) return;

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.pKey.wasPressedThisFrame)
            session.SetManualPause(!session.IsPaused);

        if (keyboard.f10Key.wasPressedThisFrame)
            session.RequestStop("interrompida_pelo_pesquisador");
    }

    private void OnGUI()
    {
        if (!ExperimentMode.IsActive) return;
        ExperimentSessionManager session = ExperimentSessionManager.Instance;
        if (session == null) return;

        ExperimentConfig config = ExperimentMode.Config;
        ExperimentSessionRequest request = ExperimentMode.Request;
        if (config == null || request == null) return;

        EnsureStyles();

        if (config.blankDisplay && !session.IsFinished)
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _blackTexture);

        Rect panel = new Rect(16, 16, PanelWidth, PanelHeight);
        GUI.Box(panel, GUIContent.none, _panelStyle);
        GUILayout.BeginArea(new Rect(panel.x + 14, panel.y + 12, panel.width - 28, panel.height - 24));

        GUILayout.Label("SESSAO EXPERIMENTAL", _titleStyle);
        GUILayout.Label(string.Format("{0}  -  sessao {1}  -  {2}",
            request.participantId, request.sessionNumber, config.label), _lineStyle);

        if (session.IsFinished)
        {
            GUILayout.Label("ENCERRADA. Copie os arquivos antes de fechar.", _titleStyle);
        }
        else if (session.IsRunning)
        {
            double remaining = session.ExposureRemaining;
            GUILayout.Label(string.Format("Decorrido {0}   Restante {1}{2}",
                Clock(session.ExposureElapsed), Clock(remaining),
                session.IsPaused ? "   [ PAUSADA ]" : ""), _titleStyle);
            GUILayout.Label("Faixa: " + (string.IsNullOrEmpty(session.CurrentTrackName)
                ? "(transicao)" : session.CurrentTrackName), _lineStyle);
        }
        else
        {
            GUILayout.Label("Preparando...", _lineStyle);
        }

        SessionLogger logger = session.Logger;
        if (logger != null)
        {
            GUILayout.Label(string.Format("Registro: {0}   |   notas: {1}",
                logger.IsOpen ? "aberto" : "FECHADO", logger.EventCount), _lineStyle);
            if (!string.IsNullOrEmpty(logger.LastError))
                GUILayout.Label("ERRO DE GRAVACAO: " + logger.LastError, _titleStyle);
        }

        if (session.MissingTracks > 0)
            GUILayout.Label("ATENCAO: " + session.MissingTracks +
                            " faixa(s) da playlist nao existem na cena.", _titleStyle);
        if (session.TrackRestarts > 0)
            GUILayout.Label("Reinicios de faixa: " + session.TrackRestarts, _lineStyle);

        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button(session.IsPaused ? "Retomar (P)" : "Pausar (P)", GUILayout.Height(30)))
            session.SetManualPause(!session.IsPaused);
        if (GUILayout.Button("INTERROMPER (F10)", GUILayout.Height(30)))
            session.RequestStop("interrompida_pelo_pesquisador");
        GUILayout.EndHorizontal();

        GUILayout.EndArea();
    }

    private static string Clock(double seconds)
    {
        if (seconds < 0) seconds = 0;
        int total = Mathf.FloorToInt((float)seconds);
        return string.Format("{0:00}:{1:00}", total / 60, total % 60);
    }

    private void EnsureStyles()
    {
        if (_stylesReady) return;

        _panelTexture = MakeTexture(new Color(0.05f, 0.05f, 0.07f, 0.92f));
        _blackTexture = MakeTexture(Color.black);

        _panelStyle = new GUIStyle(GUI.skin.box);
        _panelStyle.normal.background = _panelTexture;

        _titleStyle = new GUIStyle(GUI.skin.label);
        _titleStyle.fontSize = 16;
        _titleStyle.fontStyle = FontStyle.Bold;
        _titleStyle.normal.textColor = new Color(1f, 0.9f, 0.6f);

        _lineStyle = new GUIStyle(GUI.skin.label);
        _lineStyle.fontSize = 14;
        _lineStyle.normal.textColor = new Color(0.9f, 0.9f, 0.9f);

        _stylesReady = true;
    }

    private static Texture2D MakeTexture(Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        texture.hideFlags = HideFlags.HideAndDontSave;
        return texture;
    }

    private void OnDestroy()
    {
        if (_panelTexture != null) Destroy(_panelTexture);
        if (_blackTexture != null) Destroy(_blackTexture);
    }
}
