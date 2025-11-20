using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class BallThrowerScript : MonoBehaviour
{
    [Header("Configurações Principais")]
    public GameObject Ball;
    public Transform[] Points;
    public AudioSource MusicSource;
    public TextAsset RhythmDataFile;

    [Header("Ajustes")]
    public float DestroyBallAfter = 2f;

    private struct Note
    {
        public float time;
        public int spawnIndex;
        public Vector3 direction;
        public float speed;
    }

    private Queue<Note> songNotes = new Queue<Note>();
    private Note currentNote;
    private bool hasSongs = false;
    private bool isPlaying = false;

    void Start()
    {
        if (RhythmDataFile != null)
        {
            ParseRhythmData(RhythmDataFile.text);
        }

        if (songNotes.Count > 0)
        {
            currentNote = songNotes.Dequeue();
            hasSongs = true;
            if (!MusicSource.isPlaying) MusicSource.Play();
            isPlaying = true;
        }
    }

    void Update()
    {
        if (!isPlaying || !hasSongs) return;
        float currentTime = MusicSource.time; // AudioSettings.dspTime?
        if (currentTime >= currentNote.time)
        {
            SpawnBall(currentNote);
            if (songNotes.Count > 0)
            {
                currentNote = songNotes.Dequeue();
            }
        }
    }

    void SpawnBall(Note note)
    {
        if (note.spawnIndex >= Points.Length)
        {
            note.spawnIndex = 0;
        }
        Transform spawnPoint = Points[note.spawnIndex];
        GameObject newBall = Instantiate(Ball, spawnPoint.position, Quaternion.identity);
        Rigidbody rb = newBall.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.AddForce(note.direction.normalized * note.speed, ForceMode.VelocityChange);
        }
        Destroy(newBall, DestroyBallAfter);
    }

    void ParseRhythmData(string textData)
    {
        string[] lines = textData.Split('\n');

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            if (line.StartsWith("#")) continue; // para comentar com # no txt
            string[] data = line.Split(',');
            if (data.Length >= 6)
            {
                Note newNote = new Note();
                float msTime = float.Parse(data[0], CultureInfo.InvariantCulture);
                newNote.time = msTime / 1000f;
                newNote.spawnIndex = int.Parse(data[1]);
                float x = float.Parse(data[2], CultureInfo.InvariantCulture);
                float y = float.Parse(data[3], CultureInfo.InvariantCulture);
                float z = float.Parse(data[4], CultureInfo.InvariantCulture);
                newNote.direction = new Vector3(x, y, z);
                newNote.speed = float.Parse(data[5], CultureInfo.InvariantCulture);
                songNotes.Enqueue(newNote);
            }
        }
    }
}