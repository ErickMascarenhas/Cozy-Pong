using UnityEngine;

public class FreeGripManager : MonoBehaviour
{
    private RacketPhysics _currentRacketPhysics;
    private Rigidbody _currentRacketRb;
    private Transform _currentHandAnchor;
    private GameObject _currentHandModel;

    private bool _isAdjusting = false;

    public void SetCurrentReferences(GameObject racketObj, Transform handAnchor, GameObject handModel)
    {
        if (_isAdjusting)
        {
            ResetGrip();
        }
        _currentRacketPhysics = null;
        _currentRacketRb = null;

        if (racketObj != null)
        {
            _currentRacketPhysics = racketObj.GetComponent<RacketPhysics>();
            _currentRacketRb = racketObj.GetComponent<Rigidbody>();
        }

        _currentHandAnchor = handAnchor;
        _currentHandModel = handModel;
    }

    public void ToggleAdjustMode()
    {
        if (_currentRacketPhysics == null || _currentHandAnchor == null) return;
        if (!_isAdjusting)
        {
            StartAdjustment();
        }
        else
        {
            FinishAdjustment();
        }
    }

    public void ResetGrip()
    {
        if (_currentRacketPhysics == null) return;
        if (_isAdjusting)
        {
            _isAdjusting = false;

            if (_currentRacketRb)
            {
                _currentRacketRb.isKinematic = false;
            }

            if (_currentHandModel)
            {
                _currentHandModel.SetActive(false);
            }
        }

        _currentRacketPhysics.enabled = true;
        _currentRacketPhysics.ResetCustomOffset();
    }

    private void StartAdjustment()
    {
        _isAdjusting = true;
        if (_currentRacketPhysics) _currentRacketPhysics.enabled = false;
        if (_currentRacketRb)
        {
            _currentRacketRb.isKinematic = true;
            _currentRacketRb.linearVelocity = Vector3.zero;
            _currentRacketRb.angularVelocity = Vector3.zero;
        }
        if (_currentHandModel) _currentHandModel.SetActive(true);
    }

    private void FinishAdjustment()
    {
        _isAdjusting = false;

        if (_currentHandAnchor && _currentRacketRb)
        {
            Vector3 newPosOffset = _currentHandAnchor.InverseTransformPoint(_currentRacketRb.transform.position);
            Quaternion newRotOffset = Quaternion.Inverse(_currentHandAnchor.rotation) * _currentRacketRb.transform.rotation;

            if (_currentRacketPhysics)
                _currentRacketPhysics.SetCustomOffset(newPosOffset, newRotOffset);
        }
        if (_currentRacketRb) _currentRacketRb.isKinematic = false;
        if (_currentRacketPhysics) _currentRacketPhysics.enabled = true;
        if (_currentHandModel) _currentHandModel.SetActive(false);
    }
}