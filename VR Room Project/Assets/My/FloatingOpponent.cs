using UnityEngine;

public class FloatingOpponent : MonoBehaviour
{
    [Header("Posicoes")]
    public Transform restPosition;
    public Transform serveOrigin;
    [Header("Configuracao")]
    public float moveSpeed = 15f;
    public float rotationSpeed = 20f;
    public float hitDistance = 0.5f;
    [Header("Ajustes Visuais")]
    public Vector3 hitRotationOffset;
    private Vector3 targetPosition;
    private Quaternion targetRotation;

    void Start()
    {
        if (restPosition != null)
        {
            targetPosition = restPosition.position;
            targetRotation = restPosition.rotation;
        }

        if (Conductor.Instance != null)
        {
            Conductor.Instance.OnSpawnNote += PerformHit;
        }
    }

    void OnDestroy()
    {
        if (Conductor.Instance != null)
        {
            Conductor.Instance.OnSpawnNote -= PerformHit;
        }
    }

    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * moveSpeed);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f && restPosition != null)
        {
            Vector3 idleFloat = Vector3.up * Mathf.Sin(Time.time * 2f) * 0.002f;
            transform.position += idleFloat;
        }
    }

    void PerformHit(NoteData note)
    {
        Vector3 targetLanePos = LaneManager.Instance.GetLanePosition(note.lane);
        Vector3 directionToTarget = (targetLanePos - serveOrigin.position).normalized;
        transform.position = serveOrigin.position;
        Quaternion lookRotation = Quaternion.LookRotation(directionToTarget);
        targetRotation = lookRotation * Quaternion.Euler(hitRotationOffset);
        targetPosition = serveOrigin.position + (directionToTarget * hitDistance);
        CancelInvoke("GoToRest");
        Invoke("GoToRest", 0.4f);
    }

    void GoToRest()
    {
        if (restPosition != null)
        {
            targetPosition = restPosition.position;
            targetRotation = restPosition.rotation;
        }
    }
}