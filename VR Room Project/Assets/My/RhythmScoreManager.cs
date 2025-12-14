using System.Diagnostics;
using UnityEngine;

public class RhythmScoreManager : MonoBehaviour
{
    public static RhythmScoreManager Instance;

    [Header("Tolerancia (em segundos)")]
    public float perfectWindow = 0.15f; // perfect
    public float hitWindow = 0.35f; // good/bad

    void Awake()
    {
        Instance = this;
    }

    public void ProcessHit(float noteTime, float impactTime, bool isProhibited)
    {
        if (isProhibited)
        {
            GameFlowManager.Instance.HandleError();
            return;
        }

        float timeDiff = Mathf.Abs(impactTime - noteTime);

        if (timeDiff <= perfectWindow)
        {
            //UnityEngine.Debug.Log($"PERFECT! (Diff: {timeDiff:F3}s)");
        }
        else if (timeDiff <= hitWindow)
        {
            //UnityEngine.Debug.Log($"HIT! (Diff: {timeDiff:F3}s)");
        }
        else
        {
            //UnityEngine.Debug.Log($"BAD TIMING! (Diff: {timeDiff:F3}s)");
        }

        NoteData nextNote = Conductor.Instance.GetNextNote();

        if (nextNote != null)
        {
            RhythmBall ball = FindFirstObjectByType<RhythmBall>();
            if (ball != null) ball.OnPlayerHit(nextNote);
        }
        else
        {
            //Destroy(FindFirstObjectByType<RhythmBall>().gameObject);
        }
    }

    public void ProcessMiss()
    {
        GameFlowManager.Instance.HandleError();
    }
}