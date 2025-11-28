/*
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class ServeManager : MonoBehaviour
{
    [Header("Limpeza")]
    public Transform objectsContainer;
    [Header("Pontos de spawn, alvo e altura da mesa")]
    public Transform[] spawnPoints;
    public Transform[] targetAirPoints;
    public Transform tableHeightReference; // objeto fixo vazio posicionado na altura da superficie da mesa
    [Header("Pontos de Retorno na mesa inimiga")]
    public Transform returnLeft;
    public Transform returnRight;
    [Header("Configuracao")]
    public GameObject ballPrefab;
    public GameObject indicatorPrefab;
    [Header("Tempo e Pausa")]
    public float loopInterval = 3.0f;
    public bool isServingPaused = false;
    [Header("Controle de Sessao")]
    public float startDelay = 2.0f;
    public float sessionDuration = 60.0f; // 0 ou -1 para infinito
    public float ballLifeTime = 3.0f;
    private float _nextServeTime;
    private float _sessionEndTime;
    private bool _sessionFinished = false;

    private void OnEnable()
    {
        CleanupAllObjects();
        _sessionFinished = false;
        _nextServeTime = Time.time + startDelay;
        if (sessionDuration > 0)
        {
            _sessionEndTime = Time.time + startDelay + sessionDuration;
        }
        else
        {
            _sessionEndTime = float.MaxValue;
        }
    }

    private void OnDisable()
    {
        CleanupAllObjects();
    }

    public void CleanupAllObjects()
    {
        if (objectsContainer != null)
        {
            foreach (Transform child in objectsContainer)
            {
                Destroy(child.gameObject);
            }
        }
    }

    private void Update()
    {
        if (isServingPaused || _sessionFinished) return;
        if (sessionDuration > 0 && Time.time >= _sessionEndTime)
        {
            _sessionFinished = true;
            return;
        }
        if (Time.time >= _nextServeTime)
        {
            PerformRandomServe();
            _nextServeTime = Time.time + loopInterval;
        }
    }

    void PerformRandomServe()
    {
        if (spawnPoints.Length == 0 || targetAirPoints.Length == 0) return;
        Transform start = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
        Transform end = targetAirPoints[UnityEngine.Random.Range(0, targetAirPoints.Length)];
        float tableY = tableHeightReference.position.y;
        Vector3 bounce1Pos = Vector3.Lerp(start.position, end.position, 0.35f);
        bounce1Pos.y = tableY;
        Vector3 bounce2Pos = Vector3.Lerp(start.position, end.position, 0.75f);
        bounce2Pos.y = tableY;
        GameObject b2Obj = new GameObject("TempBounce2");
        b2Obj.transform.position = bounce2Pos;
        if (objectsContainer != null) b2Obj.transform.SetParent(objectsContainer);
        Destroy(b2Obj, ballLifeTime);
        Vector3 returnSpot = (UnityEngine.Random.value > 0.5f) ? returnRight.position : returnLeft.position;
        float speedFactor = Mathf.Clamp(loopInterval / 4.0f, 0.5f, 1.5f);
        float hNet = 0.4f * speedFactor;
        float hArc = 0.4f * speedFactor;
        GameObject ball = Instantiate(ballPrefab, start.position, Quaternion.identity);
        if (objectsContainer != null) ball.transform.SetParent(objectsContainer);
        Destroy(ball, ballLifeTime);
        GuidedBall sb = ball.GetComponent<GuidedBall>();
        if (sb == null) sb = ball.AddComponent<GuidedBall>();
        sb.InitializeServe(b2Obj.transform, end.position, returnSpot, hNet, hArc);
        Rigidbody rb = ball.GetComponent<Rigidbody>();
        rb.linearVelocity = CalculateParabolaVelocity(start.position, bounce1Pos, 0.1f);
        if (indicatorPrefab != null)
        {
            float tTotal = EstimateTotalTime(start.position, bounce1Pos, bounce2Pos, end.position, 0.1f, hNet, hArc);
            GameObject ind = Instantiate(indicatorPrefab, end.position, Quaternion.identity);
            if (objectsContainer != null) ind.transform.SetParent(objectsContainer);
            ind.transform.LookAt(Camera.main.transform);
            ind.transform.Rotate(0, 180, 0);
            TargetIndicator ti = ind.GetComponent<TargetIndicator>();
            ti.Initialize(tTotal, returnSpot);
        }
    }

    private float EstimateTotalTime(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, float h1, float h2, float h3)
    {
        return GetArcTime(p1, p2, h1) + GetArcTime(p2, p3, h2) + GetArcTime(p3, p4, h3);
    }
    private float GetArcTime(Vector3 start, Vector3 end, float heightPlus)
    {
        float g = Mathf.Abs(Physics.gravity.y);
        float maxHeight = Mathf.Max(start.y, end.y) + heightPlus;
        return Mathf.Sqrt(2 * (maxHeight - start.y) / g) + Mathf.Sqrt(2 * (maxHeight - end.y) / g);
    }
    private Vector3 CalculateParabolaVelocity(Vector3 start, Vector3 end, float height)
    {
        float gravity = Mathf.Abs(Physics.gravity.y);
        float maxHeight = Mathf.Max(start.y, end.y) + height;
        float h1 = maxHeight - start.y;
        float h2 = maxHeight - end.y;
        if (h1 < 0) h1 = 0.01f;
        if (h2 < 0) h2 = 0.01f;
        float time = Mathf.Sqrt(2 * h1 / gravity) + Mathf.Sqrt(2 * h2 / gravity);
        Vector3 velocityXZ = new Vector3(end.x - start.x, 0, end.z - start.z) / time;
        Vector3 velocityY = Vector3.up * Mathf.Sqrt(2 * gravity * h1);
        return velocityXZ + velocityY;
    }
}
*/


///*
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class ServeManager : MonoBehaviour
{
    [Header("Marcacoes")]
    public TextAsset beatMapFile;
    public float globalOffset = 0.0f;

    [Header("Referencias")]
    public Transform objectsContainer;
    public Transform tableHeightReference;

    [Header("Pontos")]
    public Transform[] spawnPoints; 
    public Transform[] targetAirPoints; 

    [Header("Pontos de retorno")]
    public Transform returnLeft;
    public Transform returnRight;

    [Header("Configuracao")]
    public GameObject ballPrefab;
    public GameObject indicatorPrefab;
    public float ballLifeTime = 4.0f;
    [Tooltip("Altura maxima do arco")]
    public float arcHeight = 0.4f;

    [Tooltip("Se True, preve tempo pra lancar bola, se False lanca no tempo exato do Txt")]
    public bool usePredictiveTiming = true;

    [Header("Estado")]
    public bool isServingPaused = false;
    public float startDelay = 0.0f;
    private struct ScheduledServe
    {
        public float HitTime;
        public float LaunchTime;
        public Transform StartPoint;
        public Transform EndPoint;
        public Vector3 BouncePos;
        public float FlightDuration;
    }

    private Queue<float> _rawBeatTimes = new Queue<float>();
    private ScheduledServe? _nextServe = null;
    private float _sessionStartTime;
    private bool _sessionStarted = false;

    private void OnEnable()
    {
        if (GameScoreManager.Instance != null) GameScoreManager.Instance.ResetGame();
        CleanupAllObjects();
        LoadBeatMap();
        _sessionStartTime = Time.time - startDelay;
        _sessionStarted = true;
        SkipPastBalls();
    }

    private void OnDisable()
    {
        CleanupAllObjects();
        _sessionStarted = false;
        _nextServe = null;
    }

    private void LoadBeatMap()
    {
        _rawBeatTimes.Clear();
        if (beatMapFile == null) return;

        string[] lines = beatMapFile.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        List<float> times = new List<float>();

        foreach (string line in lines)
        {
            if (float.TryParse(line, NumberStyles.Any, CultureInfo.InvariantCulture, out float ms))
            {
                float timeInSeconds = ms / 1000.0f;
                times.Add(timeInSeconds);
            }
        }
        times.Sort();
        foreach (float t in times) _rawBeatTimes.Enqueue(t);
    }

    private void SkipPastBalls()
    {
        while (_rawBeatTimes.Count > 0)
        {
            float nextHitTime = _rawBeatTimes.Peek();
            ScheduledServe simServe = CalculatePhysicsForBeat(nextHitTime);
            if (simServe.LaunchTime + globalOffset < startDelay - 0.1f)
            {
                _rawBeatTimes.Dequeue();
            }
            else
            {
                _nextServe = simServe;
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
            float hitTime = _rawBeatTimes.Dequeue();
            _nextServe = CalculatePhysicsForBeat(hitTime);
        }

        if (_nextServe.HasValue)
        {
            float timeSinceStart = Time.time - _sessionStartTime;

            if (timeSinceStart >= _nextServe.Value.LaunchTime + globalOffset)
            {
                PerformServe(_nextServe.Value);
                _nextServe = null;
            }
        }
    }

    private ScheduledServe CalculatePhysicsForBeat(float hitTime)
    {
        Transform startT = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
        Transform endT = targetAirPoints[UnityEngine.Random.Range(0, targetAirPoints.Length)];
        Vector3 startPos = startT.position;
        Vector3 endPos = endT.position;

        float tableY = tableHeightReference.position.y;
        Vector3 bouncePos = Vector3.Lerp(startPos, endPos, 0.4f);
        bouncePos.y = tableY;

        float durationArc1 = GetArcTime(startPos, bouncePos, 0.1f);
        float durationArc2 = GetArcTime(bouncePos, endPos, arcHeight);
        float totalFlightTime = durationArc1 + durationArc2;

        float calculatedLaunchTime;
        if (usePredictiveTiming) calculatedLaunchTime = hitTime - totalFlightTime;
        else calculatedLaunchTime = hitTime;

        return new ScheduledServe
        {
            HitTime = hitTime,
            FlightDuration = totalFlightTime,
            LaunchTime = calculatedLaunchTime,
            StartPoint = startT,
            EndPoint = endT,
            BouncePos = bouncePos
        };
    }

    private void PerformServe(ScheduledServe serveData)
    {
        Vector3 bounce2Pos = Vector3.Lerp(serveData.StartPoint.position, serveData.EndPoint.position, 0.8f);
        GameObject b2Obj = new GameObject("TempRef");
        b2Obj.transform.position = bounce2Pos;
        if (objectsContainer) b2Obj.transform.SetParent(objectsContainer);
        Destroy(b2Obj, ballLifeTime);

        GameObject ball = Instantiate(ballPrefab, serveData.StartPoint.position, Quaternion.identity);
        if (objectsContainer) ball.transform.SetParent(objectsContainer);
        Destroy(ball, ballLifeTime);

        GuidedBall sb = ball.GetComponent<GuidedBall>();
        if (sb == null) sb = ball.AddComponent<GuidedBall>();
        
        Vector3 returnSpot = (UnityEngine.Random.value > 0.5f) ? returnRight.position : returnLeft.position;
        sb.InitializeServe(b2Obj.transform, serveData.EndPoint.position, returnSpot, arcHeight, arcHeight);

        Rigidbody rb = ball.GetComponent<Rigidbody>();
        rb.linearVelocity = CalculateParabolaVelocity(serveData.StartPoint.position, serveData.BouncePos, 0.1f);

        if (indicatorPrefab != null)
        {
            GameObject ind = Instantiate(indicatorPrefab, serveData.EndPoint.position, Quaternion.identity);
            if (objectsContainer) ind.transform.SetParent(objectsContainer);
            ind.transform.LookAt(Camera.main.transform);
            TargetIndicator ti = ind.GetComponent<TargetIndicator>();
            if (ti) ti.Initialize(serveData.FlightDuration, returnSpot);
        }
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