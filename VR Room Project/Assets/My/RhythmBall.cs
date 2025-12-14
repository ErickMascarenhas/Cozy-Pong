using System.Diagnostics;
using UnityEngine;

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
    public float magnusPower = 0.01f;
    private bool isPlayerTurn = false;
    private bool waitingForBounce = false;
    public bool isForbidden = false;
    private bool applyMagnusEffect = false;
    private bool hasBeenHit = false;
    private Vector3 finalAirTarget;
    private float timeToAirTarget;

    public void Initialize(NoteData note, float totalTravelTime)
    {
        currentNote = note;
        rb = GetComponent<Rigidbody>();

        isPlayerTurn = false;
        isForbidden = false;
        applyMagnusEffect = false;
        hasBeenHit = false;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        float timeToTable = totalTravelTime * 0.6f;
        timeToAirTarget = totalTravelTime * 0.4f;

        Vector3 tableTarget = LaneManager.Instance.GetPlayerLanePos((int)note.lane);
        finalAirTarget = LaneManager.Instance.GetPlayerAirPos((int)note.lane);
        rb.linearVelocity = CalculateVelocityByTime(transform.position, tableTarget, timeToTable);
        waitingForBounce = true;
        if (indicatorPrefab != null && !isForbidden)
        {
            currentIndicator = Instantiate(indicatorPrefab, finalAirTarget, Quaternion.identity);
            HitIndicator indicatorScript = currentIndicator.GetComponent<HitIndicator>();
            if (indicatorScript != null)
            {
                Vector3 opponentCenter = LaneManager.Instance.GetOpponentTablePos(2);
                indicatorScript.Initialize(totalTravelTime, opponentCenter, OnIndicatorExpired);
            }
        }

        ConfigureBallType();
    }

    void OnIndicatorExpired()
    {
        if (!hasBeenHit && !isPlayerTurn && !isForbidden)
        {
            //Debug.Log("MISS");
            RhythmScoreManager.Instance.ProcessMiss();
            Destroy(gameObject);
        }
        else if (!hasBeenHit && isForbidden)
        {
            //Debug.Log("bola proibida evitada");
            Destroy(gameObject);
        }
    }

    void ConfigureBallType()
    {
        switch (currentNote.noteType)
        {
            case NoteType.Spin:
                float spinDirection = 0f;
                if ((int)currentNote.lane < 2) spinDirection = 1f; // se para esquerda, curva para direita
                else if ((int)currentNote.lane > 2) spinDirection = -1f; // se para direita, curva para esquerda
                else spinDirection = -1f; // se no meio, curva para esquerda
                rb.AddTorque(Vector3.up * 100f * spinDirection, ForceMode.Impulse);
                trail.startColor = Color.cyan;
                ballRenderer.material = normalMat;
                applyMagnusEffect = true;
                break;
            case NoteType.Proibida:
                isForbidden = true;
                ballRenderer.material = forbiddenMat;
                trail.startColor = Color.red;
                break;
            default:
                ballRenderer.material = normalMat;
                trail.startColor = Color.white;
                break;
        }
    }

    void FixedUpdate()
    {
        if (applyMagnusEffect && !isPlayerTurn)
        {
            Vector3 magnusForce = Vector3.Cross(rb.angularVelocity, rb.linearVelocity) * magnusPower;
            rb.AddForce(magnusForce);
        }
    }

    public void OnPlayerHit(NoteData nextNote)
    {
        if (isPlayerTurn) return;
        hasBeenHit = true;
        isPlayerTurn = true;
        applyMagnusEffect = false;
        waitingForBounce = false;
        if (currentIndicator != null) Destroy(currentIndicator);
        float nextNoteTime = nextNote.timeInBeats * Conductor.Instance.secPerBeat;
        float currentTime = Conductor.Instance.songPositionInBeats * Conductor.Instance.secPerBeat;
        float totalDuration = Mathf.Max(nextNoteTime - currentTime, 0.4f);

        float timeToTable = totalDuration * 0.6f;
        timeToAirTarget = totalDuration * 0.4f;

        Vector3 tableTarget = LaneManager.Instance.GetOpponentTablePos((int)nextNote.lane);
        finalAirTarget = LaneManager.Instance.GetOpponentAirPos((int)nextNote.lane);

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.linearVelocity = CalculateVelocityByTime(transform.position, tableTarget, timeToTable);

        waitingForBounce = true;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (waitingForBounce && collision.gameObject.CompareTag("Table"))
        {
            waitingForBounce = false;
            rb.linearVelocity = Vector3.zero;
            rb.linearVelocity = CalculateVelocityByTime(transform.position, finalAirTarget, timeToAirTarget);
            return;
        }

        if (collision.gameObject.CompareTag("Racket"))
        {
            float noteTime = currentNote.timeInBeats * Conductor.Instance.secPerBeat;
            float hitTime = Conductor.Instance.songPositionInBeats * Conductor.Instance.secPerBeat;
            RhythmScoreManager.Instance.ProcessHit(noteTime, hitTime, isForbidden);
            if (currentIndicator != null) Destroy(currentIndicator);
        }
        else if (collision.gameObject.CompareTag("Floor"))
        {
            if (!hasBeenHit && !isPlayerTurn && !isForbidden)
            {
                RhythmScoreManager.Instance.ProcessMiss();
                if (currentIndicator != null) Destroy(currentIndicator);
            }

            Destroy(gameObject, 0.1f);
        }
    }

    Vector3 CalculateVelocityByTime(Vector3 start, Vector3 end, float time)
    {
        Vector3 displacement = end - start;
        Vector3 gravity = Physics.gravity;
        float vy = (displacement.y - (0.5f * gravity.y * time * time)) / time;
        float vx = displacement.x / time;
        float vz = displacement.z / time;
        return new Vector3(vx, vy, vz);
    }

    void OnDestroy()
    {
        if (currentIndicator != null) Destroy(currentIndicator);
    }
}