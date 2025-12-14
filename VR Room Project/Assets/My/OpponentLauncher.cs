using UnityEngine;

public class OpponentLauncher : MonoBehaviour
{
    [Header("Referências")]
    public Transform spawnPoint;
    public GameObject ballPrefab;
    private RhythmBall activeBall;

    void Start()
    {
        if (Conductor.Instance != null) Conductor.Instance.OnSpawnNote += LaunchBall;
    }

    void OnDestroy()
    {
        if (Conductor.Instance != null) Conductor.Instance.OnSpawnNote -= LaunchBall;
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
}