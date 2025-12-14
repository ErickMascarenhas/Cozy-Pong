using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance;
    [Header("UI")]
    public CanvasGroup blackScreenFader; // painel preto que cobre a tela
    public TextMeshProUGUI countdownText; // texto de contagem no meio da tela
    [Header("Efeito")]
    public AudioSource sfxSource;
    public AudioClip countdownSfx; // hi-hat
    [Header("Estado")]
    public bool isGameActive = false;
    public UnityEvent onPlayerDeath; // evento pra fazer quando morrer

    void Awake() { Instance = this; }

    void Start() { }

    public void StartGameSequence()
    {
        StopAllCoroutines();
        StartCoroutine(IntroSequenceRoutine());
    }

    IEnumerator IntroSequenceRoutine()
    {
        Conductor.Instance.musicSource.Stop();
        isGameActive = false;
        countdownText.gameObject.SetActive(true);
        blackScreenFader.alpha = 1f;
        float beatDuration = 60f / Conductor.Instance.currentChart.bpm;

        for (int i = 4; i > 0; i--)
        {
            countdownText.text = i.ToString();
            sfxSource.PlayOneShot(countdownSfx);
            yield return new WaitForSeconds(beatDuration);
        }
        countdownText.text = "";
        countdownText.gameObject.SetActive(false);
        blackScreenFader.alpha = 0f;

        Conductor.Instance.PlaySong(Conductor.Instance.currentChart);
        isGameActive = true;
    }

    public void HandleError()
    {
        if (!isGameActive) return;
        isGameActive = false;
        Conductor.Instance.PauseMusic();
        var balls = FindObjectsByType<RhythmBall>(FindObjectsSortMode.None);
        foreach (var b in balls) Destroy(b.gameObject);
        bool isDead = HealthManager.Instance.AddError();

        if (isDead)
        {
            GameOver();
        }
        else
        {
            StartCoroutine(ResumeSequence());
        }
    }

    IEnumerator ResumeSequence()
    {
        blackScreenFader.alpha = 0.7f;
        countdownText.gameObject.SetActive(true);

        float beatDuration = Conductor.Instance.secPerBeat;
        for (int i = 4; i > 0; i--)
        {
            countdownText.text = i.ToString();
            sfxSource.PlayOneShot(countdownSfx);
            yield return new WaitForSeconds(beatDuration);
        }
        countdownText.gameObject.SetActive(false);
        blackScreenFader.alpha = 0f;
        Conductor.Instance.ResumeMusic();
        isGameActive = true;
    }

    void GameOver()
    {
        onPlayerDeath.Invoke();
        //Debug.Log("GAME OVER");
    }
}