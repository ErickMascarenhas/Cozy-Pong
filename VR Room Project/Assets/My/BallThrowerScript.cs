using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class BallThrowerScript : MonoBehaviour
{
    [Header("--- Mapeamento de Letras (Chart) ---")]
    [Tooltip("Letras usadas para definir a POSICAO (0 a 4)")]
    public char KeyPos_Left = 'L';      // 0: muito esquerda
    public char KeyPos_MidLeft = 'l';   // 1: esquerda
    public char KeyPos_Center = 'C';    // 2: centro
    public char KeyPos_MidRight = 'r';  // 3: muito direita
    public char KeyPos_Right = 'R';     // 4: direita
    [Tooltip("Letras usadas para o ANGULO X (Curva)")]
    public char KeyAng_Straight = 'S';  // 0: reto
    public char KeyAng_Left = 'l';      // 0.15: esquerda
    public char KeyAng_HardLeft = 'L';  // 0.25: muito esquerda
    public char KeyAng_Right = 'r';     // -0.15: direita
    public char KeyAng_HardRight = 'R'; // -0.25: muito direita
    [Tooltip("Letras usadas para a ALTURA Y")]
    public char KeyHgt_Up = 'U';        // -0.5: muito cima
    public char KeyHgt_Top = 'u';       // -0.25: cima
    public char KeyHgt_Mid = 'M';       // 0: meio/reto
    public char KeyHgt_Bot = 'd';       // 0.25: baixo
    public char KeyHgt_Down = 'D';      // 0.5: muito baixo
    [Tooltip("Letras usadas para a VELOCIDADE")]
    public char KeySpd_Slow = 'S';      // lento
    public char KeySpd_Normal = 'N';    // normal
    public char KeySpd_Fast = 'F';      // rapido

    [Header("Configuracoes Principais")]
    public GameObject BallPrefab;
    public GameObject IndicatorPrefab;
    public Transform[] Points;
    public TextAsset RhythmDataFile;
    [Header("Audio")]
    public AudioSource MusicSource;
    public AudioSource SfxSource;
    public AudioClip SoundIndicator;
    public AudioClip SoundShoot;
    [Header("Ajustes de Tempo")]
    public float PreSpawnTime = 1.0f;
    public float DestroyBallAfter = 2f;
    private class Note
    {
        public float time;
        public int spawnIndex;
        public Vector3 direction;
        public float speed;
        public GameObject indicatorInstance;
    }
    private float chartOffset = 0;
    private float chartInterval = 0;
    private float chartCeiling = 0;
    private float defaultDirZ = 1;
    private bool useLetters = false;
    private Queue<Note> pendingNotes = new Queue<Note>();
    private List<Note> activeNotes = new List<Note>();
    private bool hasSongs = false;
    private bool isPlaying = false;

    void Start()
    {
        if (RhythmDataFile != null)
        {
            ParseRhythmData(RhythmDataFile.text);
        }
        if (pendingNotes.Count > 0)
        {
            hasSongs = true;
            if (!MusicSource.isPlaying) MusicSource.Play();
            isPlaying = true;
        }
    }

    void Update()
    {
        if (!isPlaying || (!hasSongs && activeNotes.Count == 0)) return;
        float currentTime = MusicSource.time;
        if (chartCeiling > 0 && currentTime * 1000 > chartCeiling)
        {
            isPlaying = false;
            return;
        }
        if (pendingNotes.Count > 0)
        {
            Note nextNote = pendingNotes.Peek();
            if (currentTime >= nextNote.time - PreSpawnTime)
            {
                Note note = pendingNotes.Dequeue();
                ShowIndicator(note);
                activeNotes.Add(note);
            }
        }
        else
        {
            hasSongs = false;
        }
        for (int i = activeNotes.Count - 1; i >= 0; i--)
        {
            Note activeNote = activeNotes[i];
            if (currentTime >= activeNote.time)
            {
                SpawnBall(activeNote);
                if (activeNote.indicatorInstance != null) Destroy(activeNote.indicatorInstance);
                activeNotes.RemoveAt(i);
            }
        }
    }

    void ShowIndicator(Note note)
    {
        if (IndicatorPrefab == null) return;
        if (note.spawnIndex >= Points.Length || note.spawnIndex < 0) note.spawnIndex = 2;
        Transform spawnPoint = Points[note.spawnIndex];
        GameObject indicator = Instantiate(IndicatorPrefab, spawnPoint.position, Quaternion.identity);
        if (note.direction != Vector3.zero)
        {
            indicator.transform.rotation = Quaternion.LookRotation(note.direction);
        }
        note.indicatorInstance = indicator;
        if (SfxSource != null && SoundIndicator != null) SfxSource.PlayOneShot(SoundIndicator);
    }

    void SpawnBall(Note note)
    {
        Transform spawnPoint = Points[note.spawnIndex];
        GameObject newBall = Instantiate(BallPrefab, spawnPoint.position, Quaternion.identity);
        Rigidbody rb = newBall.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.AddForce(note.direction.normalized * note.speed, ForceMode.VelocityChange);
        }
        if (SfxSource != null && SoundShoot != null) SfxSource.PlayOneShot(SoundShoot);
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
                    if (chartInterval > 0 && code.ToUpper() == "NOP")
                    {
                        continue;
                    }
                    ResolveLetters(code, newNote);
                }
            }
            else
            {
                int i = (chartInterval > 0) ? 0 : 1;
                if (data.Length >= i + 4)
                {
                    newNote.spawnIndex = int.Parse(data[i]);
                    float x = float.Parse(data[i + 1], CultureInfo.InvariantCulture);
                    float y = float.Parse(data[i + 2], CultureInfo.InvariantCulture);
                    float z = (defaultDirZ != 0) ? defaultDirZ : float.Parse(data[i + 3], CultureInfo.InvariantCulture);
                    float speed = (defaultDirZ != 0) ? float.Parse(data[i + 3], CultureInfo.InvariantCulture) : float.Parse(data[i + 4], CultureInfo.InvariantCulture);

                    newNote.direction = new Vector3(x, y, z);
                    newNote.speed = speed;
                }
            }
            pendingNotes.Enqueue(newNote);
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
            useLetters = (cols[4].Trim().ToLower() == "letters");
        }
    }

    void ResolveLetters(string code, Note note)
    {
        if (code.Length < 3) return;

        char posChar = char.ToUpper(code[0]);
        char angleChar = char.ToUpper(code[1]);
        char speedChar = char.ToUpper(code[2]);
        char heightChar = (code.Length > 3) ? char.ToUpper(code[3]) : KeyHgt_Mid;

        // posicao
        if (posChar == KeyPos_Left) note.spawnIndex = 0;
        else if (posChar == KeyPos_MidLeft) note.spawnIndex = 1;
        else if (posChar == KeyPos_Center) note.spawnIndex = 2;
        else if (posChar == KeyPos_MidRight) note.spawnIndex = 3;
        else if (posChar == KeyPos_Right) note.spawnIndex = 4;
        else note.spawnIndex = 2;

        // angulo X
        float x = 0f;
        if (angleChar == KeyAng_Straight) x = 0f;
        else if (angleChar == KeyAng_Left) x = 0.15f;
        else if (angleChar == KeyAng_HardLeft) x = 0.25f;
        else if (angleChar == KeyAng_Right) x = -0.15f;
        else if (angleChar == KeyAng_HardRight) x = -0.25f;

        // altura Y
        float y = 0f;
        if (heightChar == KeyHgt_Up) y = -0.5f;
        else if (heightChar == KeyHgt_Top) y = -0.25f;
        else if (heightChar == KeyHgt_Mid) y = 0f;
        else if (heightChar == KeyHgt_Bot) y = 0.25f;
        else if (heightChar == KeyHgt_Down) y = 0.5f;

        float z = (defaultDirZ != 0) ? defaultDirZ : 1f;
        float spd = 5f;

        if (speedChar == KeySpd_Slow)
        {
            if (heightChar == KeyHgt_Mid)
            {
                if (angleChar == KeyAng_Straight) spd = 4.5f; else spd = 4.8f;
            }
            else spd = 4.5f;
        }
        else if (speedChar == KeySpd_Normal)
        {
            if (heightChar == KeyHgt_Mid)
            {
                if (angleChar == KeyAng_Straight) spd = 5.0f;
                else if (angleChar == KeyAng_Left || angleChar == KeyAng_Right) spd = 5.6f;
                else spd = 5.0f;
            }
            else spd = 5.0f;
        }
        else if (speedChar == KeySpd_Fast)
        {
            if (heightChar == KeyHgt_Mid)
            {
                if (angleChar == KeyAng_Straight) spd = 6.2f;
                else if (angleChar == KeyAng_Left || angleChar == KeyAng_Right) spd = 6.4f;
                else spd = 5.2f;
            }
            else spd = 6.0f;
        }
        note.direction = new Vector3(x, y, z);
        note.speed = spd;
    }
}