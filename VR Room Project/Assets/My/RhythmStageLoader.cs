using UnityEngine;
using System.Collections;

public class RhythmStageLoader : MonoBehaviour
{
    [Header("Cena")]
    public GameObject[] ItemsToDisable;
    public GameObject[] ItemsToEnable;
    [Header("Visual")]
    public CanvasGroup transitionCanvas;
    public float fadeDuration = 0.5f;
    [Header("Musica")]
    public GameObject songObject;

    public void LoadSong()
    {
        StartCoroutine(LoadSequence());
    }

    private IEnumerator LoadSequence()
    {
        transitionCanvas.gameObject.SetActive(true);
        transitionCanvas.blocksRaycasts = true;
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            transitionCanvas.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            yield return null;
        }
        transitionCanvas.alpha = 1f;
        AudioSource[] allAudio = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (var audio in allAudio) audio.Stop();
        if (Conductor.Instance != null) Conductor.Instance.StopAnySong();
        if (HealthManager.Instance != null) HealthManager.Instance.ResetHealth();
        if (GameScoreManager.Instance != null) GameScoreManager.Instance.ResetGame();
        foreach (var item in ItemsToDisable) if (item != null) item.SetActive(false);
        foreach (var item in ItemsToEnable) if (item != null) item.SetActive(true);
        if (songObject != null)
        {
            songObject.SetActive(true);
            var songAudio = songObject.GetComponentInChildren<AudioSource>();
            if (songAudio != null)
            {
                songAudio.Stop();
                songAudio.playOnAwake = false;
            }
        }
        yield return new WaitForEndOfFrame();
        if (GameFlowManager.Instance != null) GameFlowManager.Instance.StartGameSequence();
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            transitionCanvas.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            yield return null;
        }
        transitionCanvas.alpha = 0f;
        transitionCanvas.blocksRaycasts = false;
        transitionCanvas.gameObject.SetActive(false);
    }
}

/*
using System.Collections;
using System.Diagnostics;
using UnityEngine;

public class RhythmStageLoader : MonoBehaviour
{
    [Header("Cena")]
    public GameObject[] ItemsToDisable;
    public GameObject[] ItemsToEnable;
    [Header("Visual")]
    public CanvasGroup transitionCanvas;
    public float fadeDuration = 0.5f;
    public float waitBeforeCount = 0.5f;
    [Header("Musica")]
    public GameObject songObject; // audio

    public void LoadSong()
    {
        StartCoroutine(LoadSequence());
    }

    private IEnumerator LoadSequence()
    {
        float timer = 0f;
        transitionCanvas.gameObject.SetActive(true);
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            transitionCanvas.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            yield return null;
        }
        transitionCanvas.alpha = 1f;
        if (Conductor.Instance != null) Conductor.Instance.StopAnySong();
        if (HealthManager.Instance != null) HealthManager.Instance.ResetHealth();
        if (GameScoreManager.Instance != null) GameScoreManager.Instance.ResetGame();
        foreach (var item in ItemsToDisable) if (item != null) item.SetActive(false);
        foreach (var item in ItemsToEnable) if (item != null) item.SetActive(true);
        if (songObject != null) songObject.SetActive(true);
        yield return new WaitForSeconds(0.2f);
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            transitionCanvas.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            yield return null;
        }
        transitionCanvas.alpha = 0f;
        transitionCanvas.blocksRaycasts = false;
        yield return new WaitForSeconds(waitBeforeCount);
        if (GameFlowManager.Instance != null) GameFlowManager.Instance.StartGameSequence();
        //else UnityEngine.Debug.LogError("Faltando GameFlowManager na cena!");
    }
}
*/