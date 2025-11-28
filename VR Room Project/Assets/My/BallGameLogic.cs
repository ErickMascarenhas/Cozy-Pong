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
    public float velocityHomeRunThreshold = 6.0f;
    public float velocityOkThreshold = 2.5f;
    private bool hasBeenHit = false;
    private Rigidbody rb;
    private bool hasHitRacket = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (GameScoreManager.Instance == null) return;

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
            if (!hasHitRacket) GameScoreManager.Instance.RegisterHit(HitType.Miss);

            DestroyBall();
        }
    }

    private void EvaluateRacketHit(Collision collision)
    {
        if (hasBeenHit) return;
        hasBeenHit = true;
        Invoke(nameof(ResetHitFlag), 0.5f);
        float hitSpeed = collision.relativeVelocity.magnitude;
        //UnityEngine.Debug.Log($"FORCA DA BATIDA: {hitSpeed:0.00} (Ok < {velocityOkThreshold} | Bad > {velocityHomeRunThreshold})");

        HitType result = HitType.Ok;

        if (hitSpeed > velocityHomeRunThreshold)
        {
            result = HitType.Bad;
        }
        else if (hitSpeed < velocityOkThreshold)
        {
            result = HitType.Ok;
        }
        else
        {
            result = HitType.Perfect;
        }

        GameScoreManager.Instance.RegisterHit(result);
    }

    private void ResetHitFlag() => hasBeenHit = false;

    private void DestroyBall()
    {
        Destroy(gameObject);
    }
}