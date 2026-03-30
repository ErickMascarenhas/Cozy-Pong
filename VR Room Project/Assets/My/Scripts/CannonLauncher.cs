using UnityEngine;

public class CannonLauncher : MonoBehaviour
{
    [Header("Configuracoes")]
    public float smoothSpeed = 8f; // tempo para mover e mirar
    public float recoilForce = 0.5f;

    private Vector3 _idlePosition;
    private Quaternion _idleRotation;
    private Vector3 _targetPosition;
    private Quaternion _targetRotation;
    private float _currentRecoil = 0f;
    private bool _isActive = false;

    private void Start()
    {
        _idlePosition = transform.position;
        _idleRotation = transform.rotation;
        _targetPosition = _idlePosition;
        _targetRotation = _idleRotation;
    }

    private void Update()
    {
        Vector3 finalPosition = _targetPosition - (transform.forward * _currentRecoil); // recuo
        transform.position = Vector3.Lerp(transform.position, finalPosition, Time.deltaTime * smoothSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, _targetRotation, Time.deltaTime * smoothSpeed); // move e mira
        if (_currentRecoil > 0) _currentRecoil = Mathf.Lerp(_currentRecoil, 0, Time.deltaTime * 15f); // "desrecua"
    }

    public void AimAt(Transform spawnPoint, Vector3 initialVelocity)
    {
        _isActive = true;
        _targetPosition = spawnPoint.position; // ball pos
        if (initialVelocity != Vector3.zero) _targetRotation = Quaternion.LookRotation(initialVelocity); // rotacao
    }

    public void Shoot()
    {
        _currentRecoil = recoilForce;
    }

    public void Retract()
    {
        if (!_isActive) return;
        _isActive = false;
        _targetPosition = _idlePosition;
        _targetRotation = _idleRotation;
    }
}