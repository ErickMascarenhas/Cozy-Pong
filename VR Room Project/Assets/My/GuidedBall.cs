using UnityEngine;

public class GuidedBall : MonoBehaviour
{
    private int _bounceCount = 0;
    private Transform _targetBounce2;
    private Vector3 _targetAirHit;
    private Vector3 _returnTarget;
    private Rigidbody _rb;
    private bool _hasBeenHitByPlayer = false;
    private float _hNet;
    private float _hArc;

    public void InitializeServe(Transform bounce2, Vector3 airHit, Vector3 returnSpot, float hNet, float hArc)
    {
        _targetBounce2 = bounce2;
        _targetAirHit = airHit;
        _returnTarget = returnSpot;
        _hNet = hNet;
        _hArc = hArc;
        _rb = GetComponent<Rigidbody>();
        _bounceCount = 0;
        _hasBeenHitByPlayer = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!_hasBeenHitByPlayer && collision.gameObject.CompareTag("Table"))
        {
            _bounceCount++;
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            if (_bounceCount == 1)
            {
                LaunchToPoint(_targetBounce2.position, _hNet);
            }
            else if (_bounceCount == 2)
            {
                LaunchToPoint(_targetAirHit, _hArc);
            }
        }
        else if (collision.gameObject.CompareTag("Racket") || collision.gameObject.CompareTag("Player"))
        {
            _hasBeenHitByPlayer = true;
            LaunchToPoint(_returnTarget, 0.5f);
        }
    }

    private void LaunchToPoint(Vector3 targetPos, float heightPeak)
    {
        Vector3 velocity = CalculateParabolaVelocity(transform.position, targetPos, heightPeak);
        _rb.linearVelocity = velocity;
    }

    private Vector3 CalculateParabolaVelocity(Vector3 start, Vector3 end, float height)
    {
        float gravity = Mathf.Abs(Physics.gravity.y);
        float maxHeight = Mathf.Max(start.y, end.y) + height;
        float h1 = maxHeight - start.y;
        float h2 = maxHeight - end.y;
        if (h1 < 0) h1 = 0.01f;
        if (h2 < 0) h2 = 0.01f;
        float time = Mathf.Sqrt(2 * h1 / gravity) + Mathf.Sqrt(2 * h2 / gravity);
        Vector3 velocityXZ = new Vector3(end.x - start.x, 0, end.z - start.z) / time;
        Vector3 velocityY = Vector3.up * Mathf.Sqrt(2 * gravity * h1);
        return velocityXZ + velocityY;
    }
}