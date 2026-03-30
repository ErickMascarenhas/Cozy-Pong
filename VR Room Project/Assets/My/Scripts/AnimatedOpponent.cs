using UnityEngine;

public class AnimatedOpponent : MonoBehaviour
{
    public Animator animator;
    public Transform spawnPoint;
    public GameObject ballPrefab;
    public int currentOpponentLane = 2;
    private RhythmBall activeBall;

    void Start()
    {
        if (Conductor.Instance != null) Conductor.Instance.OnSpawnNote += StartHitAnimation;
    }

    void OnDestroy()
    {
        if (Conductor.Instance != null) Conductor.Instance.OnSpawnNote -= StartHitAnimation;
    }

    public void ResetToServe()
    {
        activeBall = null;
    }

    void StartHitAnimation(NoteData note)
    {
        int direction = (int)note.lane >= 2 ? 1 : 0;
        animator.Play(direction == 1 ? "RacketHitToLeft" : "RacketHitToRight", 0, 0f);
        LaunchBall(note);
    }

    void LaunchBall(NoteData note)
    {
        float flightDuration = Conductor.Instance.spawnOffsetBeats * Conductor.Instance.secPerBeat;
        if (activeBall != null && activeBall.gameObject.activeInHierarchy)
        {
            activeBall.transform.position = spawnPoint.position;
            activeBall.Initialize(note, flightDuration, false);
        }
        else
        {
            GameObject ballObj = Instantiate(ballPrefab, spawnPoint.position, Quaternion.identity);
            activeBall = ballObj.GetComponent<RhythmBall>();
            activeBall.Initialize(note, flightDuration, true);
        }
    }
}