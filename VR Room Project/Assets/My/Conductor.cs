using System.Diagnostics;
using UnityEngine;

public class Conductor : MonoBehaviour
{
    public static Conductor Instance;
    [Header("Configuracao")]
    public float bpm;
    public float firstBeatOffset;
    public float spawnOffsetBeats = 2.0f;
    [Header("Referencias")]
    public AudioSource musicSource;
    public SongChart currentChart;
    public float songPosition;
    public float songPositionInBeats;
    public float secPerBeat;
    public float dspSongTime;
    public bool isPlaying = false;
    private float pauseStartDspTime;
    private float accumulatedPauseTime = 0f;
    public delegate void SpawnNoteAction(NoteData note);
    public event SpawnNoteAction OnSpawnNote;
    private int nextNoteIndex = 0;

    void Awake()
    {
        Instance = this;
        secPerBeat = 60f / bpm;
    }

    void Start()
    {
        isPlaying = false;
        if (musicSource != null) musicSource.Stop();
    }

    public void PlaySong(SongChart chart)
    {
        currentChart = chart;
        bpm = chart.bpm;
        secPerBeat = 60f / bpm;
        nextNoteIndex = 0;
        accumulatedPauseTime = 0f;
        musicSource.Play();
        dspSongTime = (float)AudioSettings.dspTime;
        isPlaying = true;
    }

    public void PauseMusic()
    {
        if (isPlaying)
        {
            musicSource.Pause();
            isPlaying = false;
            pauseStartDspTime = (float)AudioSettings.dspTime;
            //UnityEngine.Debug.Log("Music PAUSED at " + songPosition);
        }
    }

    public void ResumeMusic()
    {
        if (!isPlaying)
        {
            float pauseDuration = (float)AudioSettings.dspTime - pauseStartDspTime;
            accumulatedPauseTime += pauseDuration;
            musicSource.Play();
            isPlaying = true;
            //UnityEngine.Debug.Log("Music RESUMED. Total Paused: " + accumulatedPauseTime);
        }
    }

    void Update()
    {
        if (!isPlaying) return;
        songPosition = (float)(AudioSettings.dspTime - dspSongTime - accumulatedPauseTime - firstBeatOffset);
        songPositionInBeats = songPosition / secPerBeat;
        if (currentChart != null && nextNoteIndex < currentChart.notes.Count)
        {
            NoteData nextNote = currentChart.notes[nextNoteIndex];
            if (nextNote.timeInBeats - spawnOffsetBeats <= songPositionInBeats)
            {
                if (OnSpawnNote != null)
                {
                    OnSpawnNote(nextNote);
                }
                nextNoteIndex++;
            }
        }
    }

    public NoteData GetNextNote()
    {
        if (currentChart != null && nextNoteIndex < currentChart.notes.Count)
        {
            return currentChart.notes[nextNoteIndex];
        }
        return null;
    }

    public void StopAnySong()
    {
        isPlaying = false;
        if (musicSource != null)
        {
            musicSource.Stop();
            musicSource.time = 0;
        }
    }
}