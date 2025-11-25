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