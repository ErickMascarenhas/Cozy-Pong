using UnityEngine;

public class BallGameLogic : MonoBehaviour
{
    [Tooltip("Tag da Raquete")]
    public string racketTag = "Racket";
    [Tooltip("Tag de objetos proibidos")]
    public string forbiddenTag = "Forbidden";
    [Tooltip("Tag para detectar Miss")]
    public string floorTag = "Floor";
    [Header("Parametros")]
    public float velocityHomeRunThreshold = 3.5f;
    public float velocityOkThreshold = 1.5f;
    public float timingPerfectThreshold = 0.2f;
    public float timingOkThreshold = 0.4f;
    private bool hasBeenHit = false;
    private Rigidbody rb;
    private bool hasHitRacket = false;
    private bool hasHitFloor = false;

    private ServeManager _serveManager;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        _serveManager = FindFirstObjectByType<ServeManager>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (GameScoreManager.Instance == null) return;
        if (_serveManager != null && _serveManager.isServingPaused) return;
        if (collision.gameObject.CompareTag(racketTag))
        {
            if (gameObject.CompareTag(forbiddenTag))
            {
                GameScoreManager.Instance.RegisterHit(HitType.Miss);
                DestroyBall();
                return;
            }
            if (hasHitRacket) return;

            hasHitRacket = true;
            EvaluateRacketHit(collision);
        }
        else if (collision.gameObject.CompareTag(floorTag) || collision.gameObject.name.ToLower().Contains("floor"))
        {
            if (hasHitFloor) return;
            hasHitFloor = true;
            if (!hasHitRacket) GameScoreManager.Instance.RegisterHit(HitType.Miss);
            UnityEngine.Debug.Log("Floor hit!");
            DestroyBall();
        }
    }

    private void EvaluateRacketHit(Collision collision)
    {
        if (hasBeenHit) return;
        hasBeenHit = true;
        Invoke(nameof(ResetHitFlag), 0.5f);
        float hitSpeed = 0f;
        if (collision.rigidbody != null) hitSpeed = collision.rigidbody.linearVelocity.magnitude;
        else hitSpeed = collision.relativeVelocity.magnitude;
        HitType speedResult = HitType.Perfect;
        if (hitSpeed > velocityHomeRunThreshold) speedResult = HitType.Bad;
        else if (hitSpeed < velocityOkThreshold) speedResult = HitType.Ok;
        HitType timingResult = HitType.Perfect;
        float timeDiff = 0f;
        GuidedBall guidedBall = GetComponent<GuidedBall>();
        if (guidedBall != null)
        {
            timeDiff = Mathf.Abs(guidedBall._currentFlightTime - guidedBall._expectedFlightDuration);
            if (timeDiff <= timingPerfectThreshold) timingResult = HitType.Perfect;
            else if (timeDiff <= timingOkThreshold) timingResult = HitType.Ok;
            else timingResult = HitType.Bad;
        }
        HitType finalResult;
        if (speedResult == HitType.Bad || timingResult == HitType.Bad) finalResult = HitType.Bad;
        else if (speedResult == HitType.Ok || timingResult == HitType.Ok) finalResult = HitType.Ok;
        else finalResult = HitType.Perfect;
        UnityEngine.Debug.Log($"FOR�A: {hitSpeed:0.00} ({speedResult}) | TEMPO: {timeDiff:0.00}s ({timingResult}) -> HIT FINAL: {finalResult}");
        GameScoreManager.Instance.RegisterHit(finalResult);
    }

    private void ResetHitFlag() => hasBeenHit = false;

    private void DestroyBall()
    {
        Destroy(gameObject);
    }
}