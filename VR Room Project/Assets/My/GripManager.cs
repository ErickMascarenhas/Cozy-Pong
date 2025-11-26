using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors; 

public class RacketGripManager : MonoBehaviour
{
    [Tooltip("Racket Shakehand Left")]
    public GameObject racketShakehandLeft;
    [Tooltip("Racket Penhold Left")]
    public GameObject racketPenholdLeft;
    [Tooltip("Racket Shakehand Right")]
    public GameObject racketShakehandRight;
    [Tooltip("Racket Penhold Right")]
    public GameObject racketPenholdRight;
    [Tooltip("Left Ray Interactor")]
    public XRRayInteractor leftRayInteractor;
    [Tooltip("Left Hand Model")]
    public GameObject leftHandModel;
    [Tooltip("Right Ray Interactor")]
    public XRRayInteractor rightRayInteractor;
    [Tooltip("Right Hand Model")]
    public GameObject rightHandModel;

    private bool _isRightHand = true;
    private bool _isShakehandStyle = true;

    private void Start()
    {
        _isRightHand = true; // padrao shakehand na direita
        _isShakehandStyle = true;
        UpdateRacketState();
    }

    public void SetShakehand()
    {
        _isShakehandStyle = true;
        UpdateRacketState();
    }
    public void SetPenhold()
    {
        _isShakehandStyle = false;
        UpdateRacketState();
    }
    public void SetLeftHand()
    {
        _isRightHand = false;
        UpdateRacketState();
    }
    public void SetRightHand()
    {
        _isRightHand = true;
        UpdateRacketState();
    }

    private void UpdateRacketState()
    {
        if (racketShakehandLeft) racketShakehandLeft.SetActive(false);
        if (racketPenholdLeft) racketPenholdLeft.SetActive(false);
        if (racketShakehandRight) racketShakehandRight.SetActive(false);
        if (racketPenholdRight) racketPenholdRight.SetActive(false);

        if (_isRightHand) // direita
        {
            if (_isShakehandStyle)
                racketShakehandRight.SetActive(true);
            else
                racketPenholdRight.SetActive(true);
        }
        else // esquerda
        {
            if (_isShakehandStyle)
                racketShakehandLeft.SetActive(true);
            else
                racketPenholdLeft.SetActive(true);
        }

        if (_isRightHand) // raquete na direita
        {
            ToggleHand(rightRayInteractor, rightHandModel, false);
            ToggleHand(leftRayInteractor, leftHandModel, true);
        }
        else // raquete na esquerda
        {
            ToggleHand(leftRayInteractor, leftHandModel, false);
            ToggleHand(rightRayInteractor, rightHandModel, true);
        }
    }
    private void ToggleHand(XRRayInteractor ray, GameObject model, bool state)
    {
        if (ray != null) ray.enabled = state;
        if (model != null) model.SetActive(state);
    }
}

/*
using System.Diagnostics;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class RacketGripManager : MonoBehaviour
{
    [Header("Referencias")]
    public UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    public Transform currentAttachPoint;
    [Tooltip("Shakehand Left")]
    public Transform shakehandLeftHandRef;
    [Tooltip("Penhold Left")]
    public Transform penholdLeftHandRef;
    [Tooltip("Shakehand Right")]
    public Transform shakehandRightHandRef;
    [Tooltip("Penhold Right")]
    public Transform penholdRightHandRef;

    private bool _isRightHand = true; // padrao: right hand shakehand grip
    private bool _isShakehandStyle = true;

    private void Start()
    {
        if (grabInteractable.attachTransform == null)
        {
            grabInteractable.attachTransform = currentAttachPoint;
        }
        UpdateAttachPoint();
    }

    public void SetShakehand()
    {
        _isShakehandStyle = true;
        UpdateAttachPoint();
    }
    public void SetPenhold()
    {
        _isShakehandStyle = false;
        UpdateAttachPoint();
    }

    public void SetLeftHand()
    {
        _isRightHand = false;
        UpdateAttachPoint();
    }
    public void SetRightHand()
    {
        _isRightHand = true;
        UpdateAttachPoint();
    }

    private void UpdateAttachPoint()
    {
        Transform targetTransform = null;
        if (_isRightHand)
        {
            if (_isShakehandStyle) targetTransform = shakehandRightHandRef;
            else targetTransform = penholdRightHandRef;
        }
        else // left hand
        {
            if (_isShakehandStyle) targetTransform = shakehandLeftHandRef;
            else targetTransform = penholdLeftHandRef;
        }

        if (targetTransform == null)
        {
            UnityEngine.Debug.LogWarning("GripManager: Null Reference");
            return;
        }
        currentAttachPoint.localPosition = targetTransform.localPosition;
        currentAttachPoint.localRotation = targetTransform.localRotation;
    }
}
*/