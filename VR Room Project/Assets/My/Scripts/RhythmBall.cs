using UnityEngine;
using System;

public class RhythmBall : MonoBehaviour
{
    [Header("Dados")]
    public NoteData currentNote;
    public Rigidbody rb;
    public TrailRenderer trail;
    [Header("Indicador")]
    public GameObject indicatorPrefab;
    private GameObject currentIndicator;
    [Header("Configuracao")]
    public Renderer ballRenderer;
    public Material normalMat;
    public Material forbiddenMat;
    [Header("Fisica")]
    public float magnusPower = 0.01f;
    private bool isForbidden = false;
    private bool customGravityActive = false;
    private float myGravity = 9.81f;
    private bool applyMagnusEffect = false;
    private bool isPlayerTurn = false;
    private bool waitingForBounce = false;
    private Vector3 nextTarget;
    private float nextDuration;
    private float startTime;
    private Action onCompletePath;

    public void Initialize(NoteData note, float duration, bool isServe)
    {
        currentNote = note;
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        isPlayerTurn = false;
        applyMagnusEffect = false;
        isForbidden = (note.noteType == NoteType.Proibida);
        waitingForBounce = false;
        ConfigureVisuals();
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        if (currentNote.noteType == NoteType.Spin)
        {
            float spinDirection = ((int)currentNote.lane < 2) ? 1f : -1f;
            rb.AddTorque(Vector3.up * 100f * spinDirection, ForceMode.Impulse);
            trail.startColor = Color.cyan;
            applyMagnusEffect = true;
        }
        if (isServe) StartServeRoutine(duration);
        else StartRallyRoutine(duration);
    }

    void StartServeRoutine(float totalTime)
    {
        float step1Time = totalTime * 0.2f;
        Vector3 target1 = LaneManager.Instance.GetOpponentTablePos(2);
        waitingForBounce = true;
        LaunchTo(target1, step1Time, () =>
        {
            float step2Time = totalTime * 0.4f;
            Vector3 target2 = LaneManager.Instance.GetPlayerLanePos((int)currentNote.lane);
            waitingForBounce = true;
            LaunchTo(target2, step2Time, () =>
            {
                OnTableBounce();
                waitingForBounce = false;
                float step3Time = totalTime * 0.4f;
                Vector3 target3 = LaneManager.Instance.GetPlayerAirPos((int)currentNote.lane);
                CreateIndicator(step3Time);
                LaunchTo(target3, step3Time, null);
            });
        });
    }

    void StartRallyRoutine(float totalTime)
    {
        float timeToTable = totalTime * 0.6f;
        waitingForBounce = true;
        Vector3 targetTable = LaneManager.Instance.GetPlayerLanePos((int)currentNote.lane);
        LaunchTo(targetTable, timeToTable, () =>
        {
            OnTableBounce();
            waitingForBounce = false;
            float timeToAir = totalTime * 0.4f;
            Vector3 targetAir = LaneManager.Instance.GetPlayerAirPos((int)currentNote.lane);
            CreateIndicator(timeToAir);
            LaunchTo(targetAir, timeToAir, null);
        });
    }

    void LaunchTo(Vector3 target, float duration, Action onComplete)
    {
        float fixedArcHeight = 0.35f; // ajuste da altura fixa do arco da bola (pra pular rede)
        startTime = Time.time;
        nextDuration = duration;
        nextTarget = target;
        onCompletePath = onComplete;
        BallPhysics.CalculateTrajectory(transform.position, target, duration, fixedArcHeight, out Vector3 vel, out float grav);
        rb.linearVelocity = vel;
        myGravity = grav;
        customGravityActive = true;
    }

    void FixedUpdate()
    {
        if (customGravityActive)
        {
            rb.AddForce(Vector3.down * myGravity, ForceMode.Acceleration);
            if (applyMagnusEffect && !isPlayerTurn)
            {
                Vector3 magnusForce = Vector3.Cross(rb.angularVelocity, rb.linearVelocity) * magnusPower;
                rb.AddForce(magnusForce);
            }
            if (Time.time >= startTime + nextDuration)
            {
                transform.position = nextTarget;
                customGravityActive = false;
                rb.linearVelocity = Vector3.zero;
                if (onCompletePath != null)
                {
                    Action callback = onCompletePath;
                    onCompletePath = null;
                    callback.Invoke();
                }
            }
        }
    }

    public void OnPlayerHit(NoteData nextNote)
    {
        if (isPlayerTurn) return;
        isPlayerTurn = true;
        waitingForBounce = false;
        if (currentIndicator != null) Destroy(currentIndicator);
        float nextNoteTime = nextNote.timeInBeats * Conductor.Instance.secPerBeat;
        float currentTime = Conductor.Instance.songPositionInBeats * Conductor.Instance.secPerBeat;
        float returnDuration = Mathf.Max(nextNoteTime - currentTime, 0.2f);
        Vector3 opponentHandPos = LaneManager.Instance.GetOpponentAirPos((int)nextNote.lane);
        applyMagnusEffect = false;
        LaunchTo(opponentHandPos, returnDuration, null);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Table"))
        {
            if (waitingForBounce) return;
            else
            {
                if (!isForbidden) RhythmScoreManager.Instance.ProcessMiss();
                if (currentIndicator != null) Destroy(currentIndicator);
                Destroy(gameObject);
            }
        }
        else if (collision.gameObject.CompareTag("Racket"))
        {
            float noteTime = currentNote.timeInBeats * Conductor.Instance.secPerBeat;
            float hitTime = Conductor.Instance.songPositionInBeats * Conductor.Instance.secPerBeat;
            RhythmScoreManager.Instance.ProcessHit(noteTime, hitTime, isForbidden);
            if (currentIndicator != null) Destroy(currentIndicator);
        }
        else if (collision.gameObject.CompareTag("Floor"))
        {
            if (!isPlayerTurn && !isForbidden) RhythmScoreManager.Instance.ProcessMiss();
            if (currentIndicator != null) Destroy(currentIndicator);
            Destroy(gameObject, 0.2f);
        }
    }

    void OnTableBounce()
    {
    }

    void CreateIndicator(float duration)
    {
        if (indicatorPrefab != null && !isForbidden)
        {
            currentIndicator = Instantiate(indicatorPrefab, LaneManager.Instance.GetPlayerAirPos((int)currentNote.lane), Quaternion.identity);
            HitIndicator indicatorScript = currentIndicator.GetComponent<HitIndicator>();
            if (indicatorScript != null)
            {
                Vector3 opponentCenter = LaneManager.Instance.GetOpponentTablePos(2);
                indicatorScript.Initialize(duration, opponentCenter, OnIndicatorExpired);
            }
        }
    }

    void OnIndicatorExpired()
    {
        if (!isPlayerTurn && !isForbidden)
        {
            RhythmScoreManager.Instance.ProcessMiss();
            Destroy(gameObject);
        }
        else if (isForbidden) Destroy(gameObject);
    }

    void ConfigureVisuals()
    {
        if (isForbidden)
        {
            ballRenderer.material = forbiddenMat;
            trail.startColor = Color.red;
        }
        else
        {
            ballRenderer.material = normalMat;
            trail.startColor = Color.white;
        }
    }

    void OnDestroy()
    {
        if (currentIndicator != null) Destroy(currentIndicator);
    }
}