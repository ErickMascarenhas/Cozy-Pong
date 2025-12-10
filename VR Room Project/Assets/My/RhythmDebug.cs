using System.Diagnostics;
using UnityEngine;

public class RhythmDebug : MonoBehaviour
{
    void Start()
    {
        Conductor.Instance.OnSpawnNote += HandleSpawn;
    }
    void HandleSpawn(NoteData note)
    {
        UnityEngine.Debug.Log($"DISPARO! Nota Tipo: {note.noteType} | Lane: {note.lane} | Beat Atual: {Conductor.Instance.songPositionInBeats}");
    }
}