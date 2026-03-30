using UnityEngine;
using System.Collections.Generic;

public enum NoteType // tipos de nota
{
    Normal,
    Spin,
    Proibida
}

public enum Lane // "faixas" da mesa
{
    MuitoEsquerda = 0,
    Esquerda = 1,
    Meio = 2,
    Direita = 3,
    MuitoDireita = 4
}

[System.Serializable]
public class NoteData
{
    [Tooltip("Tempo em BEATS que determina quando o jogador deve rebater a bola (60/BPM = Tempo em segundos por BEAT)")]
    public float timeInBeats;
    [Tooltip("Onde a bola deve ser rebatida")]
    public Lane lane;
    [Tooltip("Tipo de comportamento da bola")]
    public NoteType noteType;
}

[CreateAssetMenu(fileName = "New Song Chart", menuName = "Rhythm Game/Song Chart")]
public class SongChart : ScriptableObject
{
    public string songName;
    public string artistName;
    public AudioClip audioClip;
    public float bpm;
    [Header("Lista de Notas")]
    public List<NoteData> notes;
}