using UnityEngine;

public class AnimatedOpponent : MonoBehaviour
{
    [Header("Referencias")]
    public Animator animator;
    public Transform spawnPoint;
    public GameObject ballPrefab;

    [Header("Configuracao de Lanes (0 a 4)")]
    public int currentOpponentLane = 2;
    private RhythmBall activeBall;

    void Start()
    {
        if (Conductor.Instance != null)
        {
            Conductor.Instance.OnSpawnNote += StartHitAnimation;
        }
    }

    void OnDestroy()
    {
        if (Conductor.Instance != null)
        {
            Conductor.Instance.OnSpawnNote -= StartHitAnimation;
        }
    }

    void StartHitAnimation(NoteData note)
    {
        int X = currentOpponentLane;
        int Y = (int)note.lane;
        if (X <= Y)
        {
            animator.Play("RacketHitToRight", 0, 0f);
        }
        else
        {
            animator.Play("RacketHitToLeft", 0, 0f);
        }
        LaunchBall(note);
    }

    void LaunchBall(NoteData note)
    {
        Vector3 targetPos = LaneManager.Instance.GetPlayerLanePos((int)note.lane);
        float flightDuration = Conductor.Instance.spawnOffsetBeats * Conductor.Instance.secPerBeat;
        if (activeBall != null)
        {
            activeBall.transform.position = spawnPoint.position;
            activeBall.rb.linearVelocity = Vector3.zero;
            activeBall.rb.angularVelocity = Vector3.zero;
        }
        else
        {
            GameObject ballObj = Instantiate(ballPrefab, spawnPoint.position, Quaternion.identity);
            activeBall = ballObj.GetComponent<RhythmBall>();
        }
        activeBall.Initialize(note, flightDuration);
    }

    Vector3 CalculateVelocity(Vector3 start, Vector3 end, float time)
    {
        Vector3 displacement = end - start;
        Vector3 velocityY = Vector3.up * displacement.y;
        Vector3 velocityXZ = displacement;
        velocityXZ.y = 0;

        Vector3 finalVelocityXZ = velocityXZ / time;
        float gravity = Physics.gravity.y;
        float finalVelocityY = (displacement.y - (0.5f * gravity * time * time)) / time;

        return finalVelocityXZ + Vector3.up * finalVelocityY;
    }
}