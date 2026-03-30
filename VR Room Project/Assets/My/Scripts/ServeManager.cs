using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Events;

public class ServeManager : MonoBehaviour
{
    [Header("Marcacoes")]
    public TextAsset beatMapFile;
    public float globalOffset = 0.0f;

    [Header("Referencias")]
    public Transform objectsContainer;
    public Transform tableHeightReference;
    public CannonLauncher cannonVisual;

    [Header("Musica")]
    [HideInInspector] public string songID;

    [Header("Cenario")]
    public Material[] skyboxes;
    public Transform tableParent;
    public Transform floorParent;
    public int skyboxIndex = 0;
    public int tableIndex = 0;
    public int floorIndex = 0;
    private Material _originalSkybox;

    [Header("Modo de Vida (HP vs Erros)")]
    public bool useErrorBoxes = false;
    public GameObject healthUIParent;
    public GameObject errorBoxesUIParent;

    [Header("Pontos")]
    public Transform[] spawnPoints; 
    public Transform[] targetAirPoints; 

    [Header("Pontos de retorno")]
    public Transform returnLeft;
    public Transform returnRight;

    [Header("Configuracao")]
    [Tooltip("0: Padrao, 1: Proibida, 2: Spin")]
    public GameObject[] ballPrefabs = new GameObject[3];
    [Tooltip("4 = Pega 1 e ignora proximas 3")]
    public int beatSkipFactor = 4;
    public GameObject indicatorPrefab;
    private float ballLifeTime = 1.72f; // public?
    private float arcHeight = 0.25f; // public?
    [Tooltip("Se True, preve tempo pra lancar bola, se False lanca no tempo exato do Txt")]
    public bool usePredictiveTiming = true;

    [Header("Vitoria")]
    private AudioSource musicSource;
    private GameObject requiredActiveObject;
    public float timeToWaitAfterMusic = 2.0f;
    public UnityEvent onMusicComplete;

    [Header("Estado")]
    public bool isServingPaused = false;
    public float startDelay = 0.0f;
    private float accumulatedPauseTime = 0f;
    private float pauseStartTime = 0f;
    private float _lastShootTime = -999f;
    private bool _hasAimedCurrentServe = false;

    private struct RawBeat
    {
        public float Time;
        public int Type;
    }

    private struct ScheduledServe
    {
        public float HitTime;
        public float LaunchTime;
        public int BallType;
        public Transform StartPoint;
        public Transform EndPoint;
        public Vector3 Bounce1Pos;
        public Vector3 Bounce2Pos;
        public float FlightDuration;
    }

    private Queue<RawBeat> _rawBeatTimes = new Queue<RawBeat>();
    private ScheduledServe? _nextServe = null;
    private float _sessionStartTime;
    private bool _sessionStarted = false;
    private Coroutine _musicMonitorCoroutine;

    private void Awake()
    {
        songID = transform.parent != null ? transform.parent.gameObject.name : gameObject.name;
        if (transform.parent != null)
        {
            requiredActiveObject = transform.parent.gameObject;
            foreach (Transform child in transform.parent)
            {
                if (child != transform)
                {
                    AudioSource audio = child.GetComponent<AudioSource>();
                    if (audio != null)
                    {
                        musicSource = audio;
                        break;
                    }
                }
            }
        }
        else UnityEngine.Debug.LogWarning("ServeManager: No parent!");
    }

    private void OnEnable()
    {
        _originalSkybox = RenderSettings.skybox;
        SetupEnvironment();
        AudioWaveRing waveRing = FindFirstObjectByType<AudioWaveRing>();
        if (waveRing != null) waveRing.musicSource = musicSource;
        AudioReactLight[] reactLights = FindObjectsByType<AudioReactLight>(FindObjectsSortMode.None);
        foreach (AudioReactLight l in reactLights) l.musicSource = musicSource;
        if (healthUIParent) healthUIParent.SetActive(!useErrorBoxes);
        if (errorBoxesUIParent) errorBoxesUIParent.SetActive(useErrorBoxes);
        if (GameScoreManager.Instance != null)
        {
            GameScoreManager.Instance.currentSongID = songID;
            GameScoreManager.Instance.usingErrorBoxes = useErrorBoxes;
            GameScoreManager.Instance.ResetGame();
        }
        CleanupAllObjects();
        LoadBeatMap();
        accumulatedPauseTime = 0f;
        _lastShootTime = -999f;
        _hasAimedCurrentServe = false;
        _sessionStartTime = Time.time - startDelay;
        _sessionStarted = true;
        isServingPaused = false;
        SkipPastBalls();
        if (musicSource != null) _musicMonitorCoroutine = StartCoroutine(MonitorMusicEnd());
    }

    private void OnDisable()
    {
        if (healthUIParent) healthUIParent.SetActive(false);
        if (errorBoxesUIParent) errorBoxesUIParent.SetActive(false);
        if (GameScoreManager.Instance != null) GameScoreManager.Instance.usingErrorBoxes = false;
        CleanupAllObjects();
        _sessionStarted = false;
        _nextServe = null;
        if (_musicMonitorCoroutine != null)
        {
            StopCoroutine(_musicMonitorCoroutine);
            _musicMonitorCoroutine = null;
        }
        if (_originalSkybox != null)
        {
            RenderSettings.skybox = _originalSkybox;
            DynamicGI.UpdateEnvironment();
        }
        if (cannonVisual != null) cannonVisual.Retract();
    }

    private void SetupEnvironment()
    {
        if (skyboxes != null && skyboxes.Length > 0)
        {
            int safeSkyboxIndex = Mathf.Clamp(skyboxIndex, 0, skyboxes.Length - 1);
            RenderSettings.skybox = skyboxes[safeSkyboxIndex];
            DynamicGI.UpdateEnvironment();
        }
        ActivateChildByIndex(tableParent, tableIndex);
        ActivateChildByIndex(floorParent, floorIndex);
    }

    private void ActivateChildByIndex(Transform parent, int index)
    {
        if (parent == null) return;
        for (int i = 0; i < parent.childCount; i++) parent.GetChild(i).gameObject.SetActive(i == index);
    }

    //public void PauseGame()
    //{
    //    if (isServingPaused || !_sessionStarted) return;
    //    isServingPaused = true;
    //    pauseStartTime = Time.time;
    //    if (musicSource != null && musicSource.isPlaying) musicSource.Pause();
    //}

    //public void ResumeGame()
    //{
    //    if (!isServingPaused || !_sessionStarted) return;
    //    isServingPaused = false;
    //    accumulatedPauseTime += (Time.time - pauseStartTime);
    //    if (musicSource != null) musicSource.Play();
    //}

    public void PauseGame()
    {
        if (isServingPaused || !_sessionStarted) return;
        isServingPaused = true;
        pauseStartTime = Time.time;
        if (musicSource != null && musicSource.isPlaying) musicSource.Pause();
        GuidedBall[] balls = FindObjectsByType<GuidedBall>(FindObjectsSortMode.None);
        foreach (GuidedBall b in balls) b.PauseBall();
        HitIndicator[] indicators = FindObjectsByType<HitIndicator>(FindObjectsSortMode.None);
        foreach (HitIndicator ind in indicators) ind.PauseIndicator();
    }

    public void ResumeGame()
    {
        if (!isServingPaused || !_sessionStarted) return;
        isServingPaused = false;
        accumulatedPauseTime += (Time.time - pauseStartTime);
        if (musicSource != null) musicSource.Play();
        GuidedBall[] balls = FindObjectsByType<GuidedBall>(FindObjectsSortMode.None);
        foreach (GuidedBall b in balls) b.ResumeBall();
        HitIndicator[] indicators = FindObjectsByType<HitIndicator>(FindObjectsSortMode.None);
        foreach (HitIndicator ind in indicators) ind.ResumeIndicator();
    }

    private void LoadBeatMap()
    {
        _rawBeatTimes.Clear();
        if (beatMapFile == null) return;

        string[] lines = beatMapFile.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        //List<float> times = new List<float>();
        int validBeatCount = 0;

        foreach (string line in lines)
        {
            string cleanLine = line.Trim();
            if (string.IsNullOrEmpty(cleanLine) || cleanLine.StartsWith("#") || cleanLine.StartsWith("BPM") || cleanLine.StartsWith("NOTAS")) continue;
            //string[] parts = line.Split(new char[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            string[] parts = cleanLine.Split(',');
            if (parts.Length > 0)
            {
                string timerString = parts[0].Trim();
                if (float.TryParse(timerString, NumberStyles.Any, CultureInfo.InvariantCulture, out float ms))
                {
                    if (validBeatCount % beatSkipFactor == 0)
                    {
                        float hitTime = ms / 1000.0f;
                        int bType = 0;
                        if (parts.Length > 1 && int.TryParse(parts[1].Trim(), out int parsedType)) bType = Mathf.Clamp(parsedType, 0, 2);
                        _rawBeatTimes.Enqueue(new RawBeat { Time = hitTime, Type = bType });
                    }
                    //times.Add(timeInSeconds);
                    validBeatCount++;
                }
            }
        }
        //times.Sort();
        //foreach (float t in times) _rawBeatTimes.Enqueue(t);
    }

    private void SkipPastBalls()
    {
        while (_rawBeatTimes.Count > 0)
        {
            RawBeat nextHitTime = _rawBeatTimes.Peek();
            ScheduledServe simServe = CalculatePhysicsForBeat(nextHitTime);
            if (simServe.LaunchTime + globalOffset < startDelay - 0.1f)
            {
                _rawBeatTimes.Dequeue();
            }
            else
            {
                _nextServe = simServe;
                _hasAimedCurrentServe = false;
                _rawBeatTimes.Dequeue();
                break; 
            }
        }
    }

    private void Update()
    {
        if (isServingPaused || !_sessionStarted) return;

        if (_nextServe == null && _rawBeatTimes.Count > 0)
        {
            RawBeat hitTime = _rawBeatTimes.Dequeue();
            _nextServe = CalculatePhysicsForBeat(hitTime);
            _hasAimedCurrentServe = false;
        }

        if (_nextServe.HasValue)
        {
            float timeSinceStart = Time.time - _sessionStartTime - accumulatedPauseTime;
            if (!_hasAimedCurrentServe && cannonVisual != null)
            {
                bool isTimeToAim = timeSinceStart >= (_nextServe.Value.LaunchTime + globalOffset - 4f); // 4s antes da bola
                bool isAfterRecoilDelay = timeSinceStart >= (_lastShootTime + 0.5f); // 0.5s depois de atirar
                if (isTimeToAim && isAfterRecoilDelay)
                {
                    Vector3 vel = CalculateParabolaVelocity(_nextServe.Value.StartPoint.position, _nextServe.Value.Bounce1Pos, 0.1f);
                    cannonVisual.AimAt(_nextServe.Value.StartPoint, vel);
                    _hasAimedCurrentServe = true;
                }
            }
            if (timeSinceStart >= _nextServe.Value.LaunchTime + globalOffset)
            {
                PerformServe(_nextServe.Value);
                _lastShootTime = timeSinceStart;
                _nextServe = null;
            }
        }
    }

    private ScheduledServe CalculatePhysicsForBeat(RawBeat hitTime)
    {
        Transform startT = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
        Transform endT = targetAirPoints[UnityEngine.Random.Range(0, targetAirPoints.Length)];
        Vector3 startPos = startT.position;
        Vector3 endPos = endT.position;

        float tableY = tableHeightReference.position.y;
        Vector3 bounce1Pos = Vector3.Lerp(startPos, endPos, 0.4f); // 0.35f?
        bounce1Pos.y = tableY;
        Vector3 bounce2Pos = Vector3.Lerp(startPos, endPos, 0.75f);
        bounce2Pos.y = tableY;
        float durationArc1 = GetArcTime(startPos, bounce1Pos, 0.1f);
        float durationArc2 = GetArcTime(bounce1Pos, bounce2Pos, arcHeight);
        float durationArc3 = GetArcTime(bounce2Pos, endPos, arcHeight);
        float totalFlightTime = durationArc1 + durationArc2 + durationArc3;

        float calculatedLaunchTime = usePredictiveTiming ? hitTime.Time - totalFlightTime : hitTime.Time;

        return new ScheduledServe
        {
            HitTime = hitTime.Time,
            BallType = hitTime.Type,
            FlightDuration = totalFlightTime,
            LaunchTime = calculatedLaunchTime,
            StartPoint = startT,
            EndPoint = endT,
            Bounce1Pos = bounce1Pos,
            Bounce2Pos = bounce2Pos
        };
    }

    private void PerformServe(ScheduledServe serveData)
    {
        Vector3 bounce2Pos = Vector3.Lerp(serveData.StartPoint.position, serveData.EndPoint.position, 0.8f); // remover isso?
        GameObject b2Obj = new GameObject("TempRef");
        b2Obj.transform.position = serveData.Bounce2Pos;
        if (objectsContainer) b2Obj.transform.SetParent(objectsContainer);
        Destroy(b2Obj, ballLifeTime);

        GameObject prefabToUse = ballPrefabs[0];
        if (serveData.BallType < ballPrefabs.Length && ballPrefabs[serveData.BallType] != null) prefabToUse = ballPrefabs[serveData.BallType];

        GameObject ball = Instantiate(prefabToUse, serveData.StartPoint.position, Quaternion.identity);
        if (objectsContainer) ball.transform.SetParent(objectsContainer);
        StartCoroutine(WatchBallLifespan(ball, ballLifeTime));

        GuidedBall sb = ball.GetComponent<GuidedBall>();
        if (sb == null) sb = ball.AddComponent<GuidedBall>();
        
        Vector3 returnSpot = (UnityEngine.Random.value > 0.5f) ? returnRight.position : returnLeft.position;
        sb.InitializeServe(b2Obj.transform, serveData.EndPoint.position, returnSpot, arcHeight, arcHeight, serveData.FlightDuration);

        Rigidbody rb = ball.GetComponent<Rigidbody>();
        rb.linearVelocity = CalculateParabolaVelocity(serveData.StartPoint.position, serveData.Bounce1Pos, 0.1f);

        if (indicatorPrefab != null)
        {
            GameObject ind = Instantiate(indicatorPrefab, serveData.EndPoint.position, Quaternion.identity);
            if (objectsContainer) ind.transform.SetParent(objectsContainer);
            ind.transform.LookAt(Camera.main.transform);
            HitIndicator ti = ind.GetComponent<HitIndicator>();
            if (ti) ti.Initialize(serveData.FlightDuration, returnSpot, null);
        }

        if (cannonVisual != null) cannonVisual.Shoot();
    }

    private IEnumerator WatchBallLifespan(GameObject ball, float timeLimit)
    {
        float timer = 0f;
        while (timer < timeLimit)
        {
            if (!isServingPaused) timer += Time.deltaTime;
            yield return null;
        }

        if (ball != null)
        {
            GuidedBall gb = ball.GetComponent<GuidedBall>();
            if (gb != null && !gb._hasBeenHitByPlayer && GameScoreManager.Instance != null) GameScoreManager.Instance.RegisterHit(HitType.Miss);
            Destroy(ball);
        }
    }

    private IEnumerator MonitorMusicEnd()
    {
        yield return null;
        while (!musicSource.isPlaying && !isServingPaused) yield return null;
        while (musicSource.isPlaying || isServingPaused) yield return null;
        yield return new WaitForSeconds(timeToWaitAfterMusic);
        if (requiredActiveObject != null && requiredActiveObject.activeSelf) onMusicComplete?.Invoke();
    }

    public void CleanupAllObjects()
    {
        if (objectsContainer != null)
            foreach (Transform child in objectsContainer) Destroy(child.gameObject);
    }

    private float GetArcTime(Vector3 start, Vector3 end, float heightPlus)
    {
        float g = Mathf.Abs(Physics.gravity.y);
        float yMax = Mathf.Max(start.y, end.y) + heightPlus;
        return Mathf.Sqrt(2 * (yMax - start.y) / g) + Mathf.Sqrt(2 * (yMax - end.y) / g);
    }

    private Vector3 CalculateParabolaVelocity(Vector3 start, Vector3 end, float height)
    {
        float g = Mathf.Abs(Physics.gravity.y);
        float distXZ = Vector2.Distance(new Vector2(start.x, start.z), new Vector2(end.x, end.z));
        float yMax = Mathf.Max(start.y, end.y) + height;
        float tUp = Mathf.Sqrt(2 * (yMax - start.y) / g);
        float tDown = Mathf.Sqrt(2 * (yMax - end.y) / g);
        float totalTime = tUp + tDown;

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(2 * g * (yMax - start.y));
        Vector3 velocityXZ = (new Vector3(end.x - start.x, 0, end.z - start.z) / totalTime);

        return velocityXZ + velocityY;
    }
}