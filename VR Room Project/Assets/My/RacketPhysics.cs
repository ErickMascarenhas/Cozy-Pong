using UnityEngine;

public class RacketPhysics : MonoBehaviour
{
    [Tooltip("Arraste aqui o objeto 'RacketAnchor' que você criou dentro da mão")]
    public Transform target;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.maxAngularVelocity = 100f;
        rb.mass = 1.0f;
        if (target != null)
        {
            transform.position = target.position;
            transform.rotation = target.rotation;
        }
    }

    void FixedUpdate()
    {
        if (target == null) return;
        rb.linearVelocity = (target.position - transform.position) / Time.fixedDeltaTime;
        Quaternion rotationDifference = target.rotation * Quaternion.Inverse(transform.rotation);

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