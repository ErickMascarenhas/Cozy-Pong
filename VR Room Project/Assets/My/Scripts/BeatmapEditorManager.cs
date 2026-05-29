using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[System.Serializable]
public class NoteData : System.IComparable<NoteData>
{
    public float time;
    public int type;
    public int CompareTo(NoteData other)
    {
        return time.CompareTo(other.time);
    }
}

public class BeatmapEditorManager : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;
    public string songName = "MinhaMusica";
    [Header("Controles")]
    public Slider timeSlider;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI statusText;
    [Header("Visualizacao")]
    public RectTransform trackContainer;
    public GameObject notePrefab;
    public float scrollSpeed = 500f;
    [Header("Mapeamento")]
    public int currentNoteType = 0;
    public List<NoteData> savedNotes = new List<NoteData>();

    private class EditorNote
    {
        public NoteData data;
        public GameObject visualObj;
        public RectTransform rect;
    }
    private List<EditorNote> visualNotes = new List<EditorNote>();

    private bool isPlaying = false;

    private void Start()
    {
        if (timeSlider != null)
        {
            timeSlider.minValue = 0;
            if (audioSource.clip != null) timeSlider.maxValue = audioSource.clip.length;
            timeSlider.onValueChanged.AddListener(OnSliderChanged);
        }
        UpdateStatus($"Tipo Selecionado: {currentNoteType} | Pressione ENTER para tocar. ESPAÇO para marcar.");
    }

    private void Update()
    { // BOTOES: ENTER [PAUSAR/TOCAR], ESPACO [MARCAR], CTRL+S [SALVAR] E CTRL+L [ABRIR BEATMAP]
        if (Keyboard.current == null) return;
        CheckNumberInputs();
        if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame)
        {
            TogglePlayPause();
        }
        if (audioSource.isPlaying)
        {
            timeSlider.SetValueWithoutNotify(audioSource.time);
            UpdateTimerText();
        }
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            RecordHit(audioSource.time);
        }
        if (Keyboard.current.sKey.wasPressedThisFrame && Keyboard.current.ctrlKey.isPressed)
        {
            SaveBeatmap();
        }
        if (Keyboard.current.lKey.wasPressedThisFrame && Keyboard.current.ctrlKey.isPressed)
        {
            LoadBeatmap();
        }
        UpdateVisualNotes();
    }

    private void CheckNumberInputs()
    {
        if (Keyboard.current.digit0Key.wasPressedThisFrame || Keyboard.current.numpad0Key.wasPressedThisFrame) ChangeNoteType(0);
        if (Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.numpad1Key.wasPressedThisFrame) ChangeNoteType(1);
        if (Keyboard.current.digit2Key.wasPressedThisFrame || Keyboard.current.numpad2Key.wasPressedThisFrame) ChangeNoteType(2);
        if (Keyboard.current.digit3Key.wasPressedThisFrame || Keyboard.current.numpad3Key.wasPressedThisFrame) ChangeNoteType(3);
        if (Keyboard.current.digit4Key.wasPressedThisFrame || Keyboard.current.numpad4Key.wasPressedThisFrame) ChangeNoteType(4);
        if (Keyboard.current.digit5Key.wasPressedThisFrame || Keyboard.current.numpad5Key.wasPressedThisFrame) ChangeNoteType(5);
        if (Keyboard.current.digit6Key.wasPressedThisFrame || Keyboard.current.numpad6Key.wasPressedThisFrame) ChangeNoteType(6);
        if (Keyboard.current.digit7Key.wasPressedThisFrame || Keyboard.current.numpad7Key.wasPressedThisFrame) ChangeNoteType(7);
        if (Keyboard.current.digit8Key.wasPressedThisFrame || Keyboard.current.numpad8Key.wasPressedThisFrame) ChangeNoteType(8);
        if (Keyboard.current.digit9Key.wasPressedThisFrame || Keyboard.current.numpad9Key.wasPressedThisFrame) ChangeNoteType(9);
    }

    private void ChangeNoteType(int type)
    {
        currentNoteType = type;
        UpdateStatus($"Tipo de nota alterado para: [{currentNoteType}] | Total de Notas: {savedNotes.Count}");
    }

    public void TogglePlayPause()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Pause();
            UpdateStatus("Pausado. Tipo atual: [{currentNoteType}]");
        }
        else
        {
            audioSource.Play();
            UpdateStatus("Tocando... Tipo atual: [{currentNoteType}]");
        }
    }

    private void OnSliderChanged(float value)
    {
        audioSource.time = value;
        UpdateTimerText();
        UpdateVisualNotes();
    }

    private void UpdateTimerText()
    {
        if (timeText != null && audioSource.clip != null)
        {
            timeText.text = $"{audioSource.time:F2}s / {audioSource.clip.length:F2}s";
        }
    }

    private void RecordHit(float time)
    {
        NoteData newNote = new NoteData { time = time, type = currentNoteType };
        savedNotes.Add(newNote);
        savedNotes.Sort();
        SpawnVisualNote(newNote);
        UpdateStatus($"Nota Tipo [{currentNoteType}] em {time:F3}s | Total: {savedNotes.Count}");
    }

    private void SpawnVisualNote(NoteData noteData)
    {
        GameObject newNoteObj = Instantiate(notePrefab, trackContainer, false);
        RectTransform rect = newNoteObj.GetComponent<RectTransform>();
        TextMeshProUGUI noteText = newNoteObj.GetComponentInChildren<TextMeshProUGUI>();
        if (noteText != null)
        {
            noteText.text = noteData.type.ToString();
        }
        Button btn = newNoteObj.GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(() => RemoveNote(noteData, newNoteObj));
        }
        visualNotes.Add(new EditorNote { data = noteData, visualObj = newNoteObj, rect = rect });
    }

    private void RemoveNote(NoteData noteData, GameObject noteObj)
    {
        savedNotes.Remove(noteData);
        visualNotes.RemoveAll(n => n.visualObj == noteObj);
        Destroy(noteObj);
        UpdateStatus($"Nota apagada. Total: {savedNotes.Count}");
    }

    private void UpdateVisualNotes()
    {
        float currentTime = audioSource.time;
        foreach (var note in visualNotes)
        {
            if (note.rect != null)
            {
                float yPos = (note.data.time - currentTime) * scrollSpeed;
                note.rect.anchoredPosition = new Vector2(0, yPos);
            }
        }
    }

    public void SaveBeatmap()
    {
        string path = Application.dataPath + "/Beatmaps/";
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        string filePath = path + songName + ".txt";
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            foreach (NoteData n in savedNotes)
            {
                float ms = n.time * 1000f;
                writer.WriteLine($"{Mathf.RoundToInt(ms)}, {n.type}");
            }
        }
        UpdateStatus($"SALVO COM SUCESSO EM: {filePath}");
    }

    public void LoadBeatmap()
    {
        string path = Application.dataPath + "/Beatmaps/" + songName + ".txt";
        if (!File.Exists(path))
        {
            UpdateStatus($"ARQUIVO NÃO ENCONTRADO: {path}");
            return;
        }
        foreach (var note in visualNotes) Destroy(note.visualObj);
        visualNotes.Clear();
        savedNotes.Clear();
        string[] lines = File.ReadAllLines(path);
        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            string[] parts = line.Split(',');
            if (parts.Length >= 2)
            {
                if (float.TryParse(parts[0].Trim(), out float ms) && int.TryParse(parts[1].Trim(), out int type))
                {
                    NoteData loadedNote = new NoteData { time = ms / 1000f, type = type };
                    savedNotes.Add(loadedNote);
                }
            }
        }
        savedNotes.Sort();
        foreach (var note in savedNotes)
        {
            SpawnVisualNote(note);
        }
        UpdateStatus($"BEATMAP CARREGADO! {savedNotes.Count} notas encontradas.");
    }

    private void UpdateStatus(string msg)
    {
        if (statusText != null) statusText.text = msg;
        Debug.Log(msg);
    }
}