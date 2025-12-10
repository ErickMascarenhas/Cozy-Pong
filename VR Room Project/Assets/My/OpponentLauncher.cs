using UnityEngine;

public class OpponentLauncher : MonoBehaviour
{
    [Header("Referencias")]
    public Transform spawnPoint; // ponto de spawn da bola (onde fica mao do oponente)
    public GameObject ballPrefab; // prefab da bola

    void Start()
    {
        if (Conductor.Instance != null)
        {
            Conductor.Instance.OnSpawnNote += LaunchBall;
        }
    }

    void OnDestroy()
    {
        if (Conductor.Instance != null)
        {
            Conductor.Instance.OnSpawnNote -= LaunchBall;
        }
    }

    void LaunchBall(NoteData note)
    {
        Vector3 targetPos = LaneManager.Instance.GetLanePosition(note.lane); // identifica lane alvo
        GameObject ballObj = Instantiate(ballPrefab, spawnPoint.position, Quaternion.identity); // cria bola
        RhythmBall rBall = ballObj.GetComponent<RhythmBall>();
        rBall.Initialize(note);
        float flightDuration = Conductor.Instance.spawnOffsetBeats * Conductor.Instance.secPerBeat; // calcula fisica baseado no tempo de viagem e segundos por beat
        Vector3 velocity = CalculateVelocity(spawnPoint.position, targetPos, flightDuration);
        rBall.rb.linearVelocity = velocity; // aplica a velocidade
    }

    Vector3 CalculateVelocity(Vector3 start, Vector3 end, float time) // calcula velocidade inicial pra ir de um ponto a outro em tempo determinado
    {
        Vector3 displacement = end - start; // distancia vetorial
        Vector3 velocityY = Vector3.up * displacement.y; // velocidade horizontal constante (distancia / tempo)
        Vector3 velocityXZ = displacement;
        velocityXZ.y = 0; // calcula apenas plano horizontal
        Vector3 finalVelocityXZ = velocityXZ / time;
        float gravity = Physics.gravity.y; // velocidade vertical: DeltaY = Vi*t + 0.5*g*t^2 -> Vi = (DeltaY - 0.5*g*t^2) / t
        float finalVelocityY = (displacement.y - (0.5f * gravity * time * time)) / time;
        return finalVelocityXZ + Vector3.up * finalVelocityY;
    }
}