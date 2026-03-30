using UnityEngine;
using System.Collections;

public class SongLoader : MonoBehaviour
{
    [Header("Objetos das cenas")]
    public GameObject[] ItemsToDisable;
    public GameObject[] ItemsToEnable;

    [Header("Objeto dos managers das musicas")]
    public Transform parentToDisableChildren;

    [Header("Tela de loading e Musica")]
    public CanvasGroup transitionCanvas;
    public GameObject songObject;

    [Header("Settings")]
    public float fadeDuration = 0.5f; // tempo de fade
    public float stayDarkDuration = 1f; // tempo que fica escuro

    public void LoadSong()
    {
        StartCoroutine(TransitionSequence());
    }

    private IEnumerator TransitionSequence()
    {
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            transitionCanvas.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            yield return null;
        }
        transitionCanvas.alpha = 1f;

        for (int i = 0; i < ItemsToDisable.Length; i++)
        {
            if (ItemsToDisable[i] != null) ItemsToDisable[i].SetActive(false);
        }
        if (parentToDisableChildren != null) foreach (Transform child in parentToDisableChildren) child.gameObject.SetActive(false);
        for (int i = 0; i < ItemsToEnable.Length; i++)
        {
            if (ItemsToEnable[i] != null) ItemsToEnable[i].SetActive(true);
        }

        if (songObject != null) songObject.SetActive(true);
        
        yield return new WaitForSeconds(stayDarkDuration);
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            transitionCanvas.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            yield return null;
        }
        transitionCanvas.alpha = 0f;
    }
}