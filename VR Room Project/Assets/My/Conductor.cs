using UnityEngine;
using System;

public class Conductor : MonoBehaviour
{
    public static Conductor Instance;
    [Header("Configuracao")]
    public SongChart currentChart; // "mapeamento" das notas
    public AudioSource musicSource;
    public float spawnOffsetBeats = 2.0f; // tempo antes do hit pra bola nascer
    [Header("Estado Atual")]
    public float songPosition; // tempo atual da musica em segundos
    public float songPositionInBeats; // tempo atual em beats
    public float secPerBeat; // segundos por beat
    public float dspSongTime; // momento de inicio da musica
    public event Action<NoteData> OnSpawnNote; // evento de spawn da nota
    private int nextNoteIndex = 0;
    private bool isPlaying = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (currentChart != null)
        {
            PlaySong(currentChart);
        }
    }

    public void PlaySong(SongChart chart)
    {
        currentChart = chart;
        secPerBeat = 60f / currentChart.bpm;
        musicSource.clip = currentChart.audioClip;
        nextNoteIndex = 0;
        songPosition = 0;
        dspSongTime = (float)AudioSettings.dspTime + 1.0f; // delay de loading pre play
        musicSource.PlayScheduled(dspSongTime);
        isPlaying = true;
    }

    void Update()
    {
        if (!isPlaying) return;
        songPosition = (float)(AudioSettings.dspTime - dspSongTime); // calcular tempo atual
        songPositionInBeats = songPosition / secPerBeat; // calcular beat atual
        if (nextNoteIndex < currentChart.notes.Count)
        {
            NoteData nextNote = currentChart.notes[nextNoteIndex];
            float spawnBeat = nextNote.timeInBeats - spawnOffsetBeats; // tempo pra spawnar eh tempo em beats - tempo de viagem da bola
            if (songPositionInBeats >= spawnBeat)
            {
                OnSpawnNote?.Invoke(nextNote); // lanca ela
                nextNoteIndex++;
            }
        }
    }

    public void PauseMusic()
    {
        musicSource.Pause();
        isPlaying = false;
    }

    public void ResumeMusic()
    {
        dspSongTime = (float)AudioSettings.dspTime - songPosition;
        musicSource.Play();
        isPlaying = true;
    }
}