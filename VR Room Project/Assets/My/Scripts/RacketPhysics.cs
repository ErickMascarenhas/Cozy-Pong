using UnityEngine;

public class RacketPhysics : MonoBehaviour
{
    [Tooltip("RacketAnchor")]
    public Transform target;
    [Header("Forca")]
    [Tooltip("Velocidade maxima permitida")]
    public float maxVelocity = 10.0f;
    [Tooltip("Suavizacao. Valor maior = raquete mais 'pesada' e menos explosiva. Padrao = 1.0")]
    [Range(1.0f, 5.0f)]
    public float smoothing = 1.0f;
    private Rigidbody rb;
    private Vector3 _customPosOffset = Vector3.zero;
    private Quaternion _customRotOffset = Quaternion.identity;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.maxAngularVelocity = 150f;
        rb.mass = 1.0f;
        if (target != null)
        {
            transform.position = target.position;
            transform.rotation = target.rotation;
        }
    }

    public void SetCustomOffset(Vector3 localPos, Quaternion localRot)
    {
        _customPosOffset = localPos;
        _customRotOffset = localRot;
    }

    public void ResetCustomOffset()
    {
        _customPosOffset = Vector3.zero;
        _customRotOffset = Quaternion.identity;
    }

    void FixedUpdate()
    {
        if (target == null) return;
        Vector3 targetPosition = target.TransformPoint(_customPosOffset);
        Quaternion targetRotation = target.rotation * _customRotOffset;
        Vector3 neededVelocity = (targetPosition - transform.position) / (Time.fixedDeltaTime * smoothing);
        if (neededVelocity.magnitude > maxVelocity)
        {
            neededVelocity = Vector3.ClampMagnitude(neededVelocity, maxVelocity);
        }
        rb.linearVelocity = neededVelocity;
        Quaternion rotationDifference = targetRotation * Quaternion.Inverse(transform.rotation);
        rotationDifference.ToAngleAxis(out float angleInDegree, out Vector3 rotationAxis);

        if (angleInDegree > 180f) angleInDegree -= 360f;

        if (Mathf.Abs(angleInDegree) > Mathf.Epsilon)
        {
            Vector3 rotationDifferenceInDegree = angleInDegree * rotationAxis;
            rb.angularVelocity = (rotationDifferenceInDegree * Mathf.Deg2Rad / Time.fixedDeltaTime);
        }
        else
        {
            rb.angularVelocity = Vector3.zero;
        }
    }
}