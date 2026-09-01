using UnityEngine;

public class GuidedBall : MonoBehaviour
{
    private int _bounceCount = 0;
    private Transform _targetBounce2;
    private Vector3 _targetAirHit;
    private Vector3 _returnTarget;
    private Rigidbody _rb;
    public bool _hasBeenHitByPlayer = false;
    private float _hNet;
    private float _hArc;
    private Vector3 _savedVelocity;
    private TrailRenderer _trail;
    private float _originalTrailTime;
    private bool _isPaused = false;
    public float _expectedFlightDuration;
    public float _currentFlightTime;
    private Collider _col;

    // --- contexto da nota, preenchido pelo ServeManager ao servir ----
    // Guardado na propria bola porque e ela que sobrevive ate a rebatida.
    [System.NonSerialized] public ServeManager Origin;
    [System.NonSerialized] public int NoteIndex = -1;
    [System.NonSerialized] public int BallType;
    [System.NonSerialized] public int SpawnIndex = -1;
    [System.NonSerialized] public int TargetIndex = -1;
    /// <summary>Instante da batida que originou esta bola, em segundos dentro da faixa.</summary>
    [System.NonSerialized] public float BeatTimeSeconds;
    /// <summary>Instante em que a bola foi lancada, em segundos dentro da faixa.</summary>
    [System.NonSerialized] public float LaunchTimeSeconds;

    public void InitializeServe(Transform bounce2, Vector3 airHit, Vector3 returnSpot, float hNet, float hArc, float flightDuration)
    {
        _targetBounce2 = bounce2;
        _targetAirHit = airHit;
        _returnTarget = returnSpot;
        _hNet = hNet;
        _hArc = hArc;
        _expectedFlightDuration = flightDuration;
        _currentFlightTime = 0f;
        _isPaused = false;
        _rb = GetComponent<Rigidbody>();
        _bounceCount = 0;
        _hasBeenHitByPlayer = false;
    }

    private void Update()
    {
        if (_isPaused || _hasBeenHitByPlayer) return;
        _currentFlightTime += Time.deltaTime;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!_hasBeenHitByPlayer && collision.gameObject.CompareTag("Table"))
        {
            _bounceCount++;
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            if (_bounceCount == 1)
            {
                LaunchToPoint(_targetBounce2.position, _hNet);
            }
            else if (_bounceCount == 2)
            {
                LaunchToPoint(_targetAirHit, _hArc);
            }
        }
        else if (collision.gameObject.CompareTag("Racket") || collision.gameObject.CompareTag("Player"))
        {
            // Apenas a fisica do retorno. A classificacao e a pontuacao da rebatida
            // sao responsabilidade exclusiva de BallGameLogic, que recebe a mesma
            // colisao: registrar aqui tambem contava cada rebatida duas vezes, com
            // limiares diferentes dos usados la.
            _hasBeenHitByPlayer = true;
            LaunchToPoint(_returnTarget, 0.5f);
        }
    }

    public void PauseBall()
    {
        _isPaused = true;
        if (_rb == null) _rb = GetComponent<Rigidbody>();
        if (_trail == null) _trail = GetComponentInChildren<TrailRenderer>();
        if (_col == null) _col = GetComponent<Collider>();

        if (_rb != null)
        {
            _savedVelocity = _rb.linearVelocity;
            _rb.isKinematic = true;
        }

        if (_trail != null)
        {
            _originalTrailTime = _trail.time;
            _trail.time = Mathf.Infinity;
        }

        if (_col != null) _col.enabled = false;
    }

    public void ResumeBall()
    {
        _isPaused = false;
        if (_rb != null)
        {
            _rb.isKinematic = false;
            _rb.linearVelocity = _savedVelocity;
            _rb.WakeUp();
        }
        if (_trail != null) _trail.time = _originalTrailTime;
        if (_col != null) _col.enabled = true;
    }

    private void LaunchToPoint(Vector3 targetPos, float heightPeak)
    {
        Vector3 velocity = CalculateParabolaVelocity(transform.position, targetPos, heightPeak);
        _rb.linearVelocity = velocity;
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