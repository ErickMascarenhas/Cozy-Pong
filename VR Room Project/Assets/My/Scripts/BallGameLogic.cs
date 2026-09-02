using UnityEngine;

public class BallGameLogic : MonoBehaviour
{
    [Tooltip("Tag da Raquete")]
    public string racketTag = "Racket";
    [Tooltip("Tag de objetos proibidos")]
    public string forbiddenTag = "Forbidden";
    [Tooltip("Tag para detectar Miss")]
    public string floorTag = "Floor";
    [Header("Efeitos Visuais")]
    public GameObject hitParticlePrefab;
    public bool useRacketColor = true;
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

        // No experimento os limiares vem da condicao, e nao do prefab: e o que
        // permite afirmar no Capitulo 3 que a configuracao animada julga com
        // mais rigor que a relaxante.
        if (ExperimentMode.UsesConditionConfig && ExperimentMode.Config != null)
        {
            ExperimentConfig config = ExperimentMode.Config;
            velocityHomeRunThreshold = config.velocityHomeRunThreshold;
            velocityOkThreshold = config.velocityOkThreshold;
            timingPerfectThreshold = config.timingPerfectThreshold;
            timingOkThreshold = config.timingOkThreshold;
        }
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
            // O erro de tempo julgado aqui e o mesmo e_i reportado no TCC
            // (Equacao 3.1): a distancia entre o instante da rebatida e a
            // batida mais proxima da musica. Antes o julgamento usava a
            // diferenca entre tempo de voo e tempo previsto, uma quantidade
            // parecida mas nao identica a que o texto declara medir.
            float beatError = -1f;
            if (guidedBall.Origin != null)
                beatError = guidedBall.Origin.DistanceToNearestBeat(guidedBall.Origin.TrackTime);

            timeDiff = beatError >= 0f
                ? beatError
                : Mathf.Abs(guidedBall._currentFlightTime - guidedBall._expectedFlightDuration);

            if (timeDiff <= timingPerfectThreshold) timingResult = HitType.Perfect;
            else if (timeDiff <= timingOkThreshold) timingResult = HitType.Ok;
            else timingResult = HitType.Bad;
        }
        HitType finalResult;
        if (speedResult == HitType.Bad || timingResult == HitType.Bad) finalResult = HitType.Bad;
        else if (speedResult == HitType.Ok || timingResult == HitType.Ok) finalResult = HitType.Ok;
        else finalResult = HitType.Perfect;
        // Durante o experimento o desfecho vai para o arquivo de eventos, e nao
        // para o console: sao centenas de notas por sessao.
        if (!ExperimentMode.IsActive)
            UnityEngine.Debug.Log($"FORCA: {hitSpeed:0.00} ({speedResult}) | TEMPO: {timeDiff:0.00}s ({timingResult}) -> HIT FINAL: {finalResult}");
        if (hitParticlePrefab != null)
        {
            Vector3 contactPoint = collision.contacts.Length > 0 ? collision.contacts[0].point : transform.position;
            GameObject particle = Instantiate(hitParticlePrefab, contactPoint, Quaternion.identity);
            ParticleSystem ps = particle.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                if (useRacketColor)
                {
                    RacketColorManager racketColors = collision.gameObject.GetComponent<RacketColorManager>();
                    if (racketColors == null) racketColors = collision.gameObject.GetComponentInParent<RacketColorManager>();
                    if (racketColors != null)
                    {
                        main.startColor = racketColors.bladeColor;
                    }
                    else
                    {
                        main.startColor = Color.white;
                    }
                }
                else
                {
                    if (finalResult == HitType.Perfect) main.startColor = Color.green;
                    else if (finalResult == HitType.Ok) main.startColor = Color.yellow;
                    else main.startColor = new Color(1f, 0.5f, 0f);
                }
            }
            Destroy(particle, 1f);
        }
        GameScoreManager.Instance.RegisterHit(finalResult);

        if (guidedBall != null && guidedBall.Origin != null)
            guidedBall.Origin.ReportNoteOutcome(guidedBall, finalResult, hitSpeed, true);
    }

    private void ResetHitFlag() => hasBeenHit = false;

    private void DestroyBall()
    {
        Destroy(gameObject);
    }
}