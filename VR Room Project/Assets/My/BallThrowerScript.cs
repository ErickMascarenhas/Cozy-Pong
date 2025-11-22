using System.Collections.Generic;
using System.Diagnostics;
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

    private float chartOffset = 0; // variaveis do chart
    private float chartInterval = 0; // se 0, usa tempo do txt. se > 0, calcula automatico
    private float chartCeiling = 0;  // tempo maximo permitido
    private float defaultDirZ = 1;   // usa este valor fixo ate segunda ordem
    private bool useLetters = false; // modo de leitura

    private Queue<Note> songNotes = new Queue<Note>();
    private Note currentNote;
    private bool hasSongs = false;
    private bool hasActiveNote = false;
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
            hasActiveNote = true;
            hasSongs = true;

            if (!MusicSource.isPlaying) MusicSource.Play();
            isPlaying = true;
        }
    }

    void Update()
    {
        if (!isPlaying || !hasSongs || !hasActiveNote) return;

        float currentTime = MusicSource.time;

        if (chartCeiling > 0 && currentTime * 1000 > chartCeiling)
        {
            hasSongs = false;
            hasActiveNote = false;
            return;
        }

        if (currentTime >= currentNote.time)
        {
            SpawnBall(currentNote);

            if (songNotes.Count > 0)
            {
                currentNote = songNotes.Dequeue();
                hasActiveNote = true;
            }
            else
            {
                hasActiveNote = false;
                hasSongs = false;
            }
        }
    }

    void SpawnBall(Note note)
    {
        if (note.spawnIndex >= Points.Length || note.spawnIndex < 0)
        {
            note.spawnIndex = 2;
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
        bool headerParsed = false;
        float currentFixedTimeAccumulator = 0;

        foreach (string line in lines)
        {
            string cleanLine = line.Trim();
            if (string.IsNullOrWhiteSpace(cleanLine)) continue;
            if (cleanLine.StartsWith("#")) continue;
            if (!headerParsed)
            {
                ParseHeader(cleanLine);
                headerParsed = true;
                currentFixedTimeAccumulator = chartOffset;
                continue;
            }
            string[] data = cleanLine.Split(',');
            if (data.Length < 1) continue;

            Note newNote = new Note();
            if (chartInterval > 0)
            {
                newNote.time = currentFixedTimeAccumulator / 1000f;
                currentFixedTimeAccumulator += chartInterval;
            }
            else
            {
                float msTime = float.Parse(data[0], CultureInfo.InvariantCulture);
                newNote.time = (msTime + chartOffset) / 1000f;
            }
            if (useLetters)
            {
                int stringIndex = (chartInterval > 0) ? 0 : 1;
                if (data.Length > stringIndex)
                {
                    string code = data[stringIndex].Trim();
                    ResolveLetters(code, ref newNote);
                }
            }
            else
            {
                int startIndex = (chartInterval > 0) ? 0 : 1;
                if (data.Length >= startIndex + 4)
                {
                    newNote.spawnIndex = int.Parse(data[startIndex]);
                    float x = float.Parse(data[startIndex + 1], CultureInfo.InvariantCulture);
                    float y = float.Parse(data[startIndex + 2], CultureInfo.InvariantCulture);

                    float z;
                    float speed;
                    if (defaultDirZ != 0)
                    {
                        z = defaultDirZ;
                        speed = float.Parse(data[startIndex + 3], CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        z = float.Parse(data[startIndex + 3], CultureInfo.InvariantCulture);
                        speed = float.Parse(data[startIndex + 4], CultureInfo.InvariantCulture);
                    }

                    newNote.direction = new Vector3(x, y, z);
                    newNote.speed = speed;
                }
            }

            songNotes.Enqueue(newNote);
        }
    }

    void ParseHeader(string line)
    {
        string[] cols = line.Split(',');
        if (cols.Length >= 5)
        {
            chartOffset = float.Parse(cols[0], CultureInfo.InvariantCulture);
            chartInterval = float.Parse(cols[1], CultureInfo.InvariantCulture);
            chartCeiling = float.Parse(cols[2], CultureInfo.InvariantCulture);
            float dirZParam = float.Parse(cols[3], CultureInfo.InvariantCulture);
            defaultDirZ = (dirZParam == 0) ? 0 : dirZParam;
            if (defaultDirZ == 0 && dirZParam != 0) defaultDirZ = 1;
            string type = cols[4].Trim().ToLower();
            useLetters = (type == "letters");
        }
    }

    void ResolveLetters(string code, ref Note note)
    {
        if (code.Length < 3) return;

        char posChar = char.ToUpper(code[0]);
        char angleChar = char.ToUpper(code[1]);
        char speedChar = char.ToUpper(code[2]);
        char dirY = (code.Length > 3) ? char.ToUpper(code[3]) : 'M';

        switch (posChar) // posicao - ponto 0 a 4: L = esquerda(0), l = meio-esq(1), C = centro(2), r = meio-dir(3), R = direita(4)
        {
            case 'L': note.spawnIndex = 0; break;
            case 'l': note.spawnIndex = 1; break;
            case 'C': note.spawnIndex = 2; break;
            case 'r': note.spawnIndex = 3; break;
            case 'R': note.spawnIndex = 4; break;
            default: note.spawnIndex = 2; break; // se nao entender, joga no meio
        }

        float x = 0;
        float y = 0;
        float z = (defaultDirZ != 0) ? defaultDirZ : 1f;
        float spd = 5f;
        
        switch (angleChar) // direcaoX - dirX: L = muito esquerda(0.25), l = esquerda(0.15), S = reto(0), r = direita(-0.15), R = muito direita(-0.25)
        {
            case 'S': x = 0f; break;
            case 'l': x = 0.15f; break;
            case 'L': x = 0.25f; break;
            case 'r': x = -0.15f; break;
            case 'R': x = -0.25f; break;
            default: x = 0f; break;
        }
        switch (dirY) // direcaoY - dirY: D = muito baixo(-0.5), d = baixo(0.25), M = reto(0), u = cima(0.25), U = muito cima(0.5)
        {
            case 'D': y = -0.2f; break;
            case 'd': y = -0.1f; break;
            case 'M': y = 0f; break;
            case 'u': y = 0.1f; break;
            case 'U': y = 0.2f; break;
            default: y = 0f; break;
        }
        if (speedChar == 'S') // velocidade: S = lento(~4.5-4.8), N = normal/medio(~5.0-5.6), F = rapido(~6.2-6.4)
        {
            if (dirY == 'M')
            {
                if (angleChar == 'S') spd = 4.5f;
                else if (angleChar == 'l' || angleChar == 'R') spd = 4.8f;
                else if (angleChar == 'L' || angleChar == 'r') spd = 4.8f;
            }
            else if (dirY == 'U') { spd = 4.5f; } // muito cima, lento
            else if (dirY == 'u') { spd = 4.5f; } // cima, lento
            else if (dirY == 'd') { spd = 4.5f; } // baixo, lento
            else if (dirY == 'D') { spd = 4.5f; } // muito baixo, lento
        }
        else if (speedChar == 'N')
        {
            if (dirY == 'M')
            {
                if (angleChar == 'S') spd = 5.0f;
                else if (angleChar == 'l' || angleChar == 'R') spd = 5.6f;
                else if (angleChar == 'L' || angleChar == 'r') spd = 5.0f;
            }
            else if (dirY == 'U') { spd = 5.0f; } // muito cima, normal
            else if (dirY == 'u') { spd = 5.0f; } // cima, normal
            else if (dirY == 'd') { spd = 5.0f; } // baixo, normal
            else if (dirY == 'D') { spd = 5.0f; } // muito baixo, normal
        }
        else if (speedChar == 'F')
        {
            if (dirY == 'M')
            {
                if (angleChar == 'S') spd = 6.2f;
                else if (angleChar == 'l' || angleChar == 'R') spd = 6.4f;
                else if (angleChar == 'L' || angleChar == 'r') spd = 5.2f;
            }
            else if (dirY == 'U') { spd = 6.0f; } // muito cima, rapido
            else if (dirY == 'u') { spd = 6.0f; } // cima, rapido
            else if (dirY == 'd') { spd = 6.0f; } // baixo, rapido
            else if (dirY == 'D') { spd = 6.0f; } // muito baixo, rapido
        }
        note.direction = new Vector3(x, y, z);
        note.speed = spd;
    }
}