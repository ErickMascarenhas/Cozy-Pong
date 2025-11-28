using UnityEngine;

public class GripManager : MonoBehaviour
{
    [Header("FreeGripManager")]
    public FreeGripManager freeGripManager;

    [Header("Raquetes")]
    [Tooltip("Racket Left")]
    public GameObject racketLeft;
    [Tooltip("Racket Right")]
    public GameObject racketRight;

    [Header("Mao Esquerda")]
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor leftRayInteractor;
    public GameObject leftHandModel;
    [Tooltip("RacketAnchor")]
    public Transform leftHandAnchor;

    [Header("Mao Direita")]
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor rightRayInteractor;
    public GameObject rightHandModel;
    [Tooltip("RacketAnchor")]
    public Transform rightHandAnchor;

    private bool _isRightHand = true;

    private void Start()
    {
        _isRightHand = true;
        UpdateRacketState(forceUpdate: true);
    }

    public void SetLeftHand()
    {
        if (!_isRightHand) return;
        _isRightHand = false;
        UpdateRacketState();
    }

    public void SetRightHand()
    {
        if (_isRightHand) return;
        _isRightHand = true;
        UpdateRacketState();
    }

    private void UpdateRacketState(bool forceUpdate = false)
    {
        if (racketLeft) racketLeft.SetActive(false);
        if (racketRight) racketRight.SetActive(false);

        GameObject currentRacket = null;
        Transform currentAnchor = null;
        GameObject currentModelToVis = null;

        if (_isRightHand)
        {
            currentRacket = racketRight;
            currentAnchor = rightHandAnchor;
            currentModelToVis = rightHandModel;
            ToggleHand(rightRayInteractor, rightHandModel, false);
            ToggleHand(leftRayInteractor, leftHandModel, true);
        }
        else
        {
            currentRacket = racketLeft;
            currentAnchor = leftHandAnchor;
            currentModelToVis = leftHandModel;
            ToggleHand(leftRayInteractor, leftHandModel, false);
            ToggleHand(rightRayInteractor, rightHandModel, true);
        }
        if (currentRacket != null) currentRacket.SetActive(true);
        if (freeGripManager != null) freeGripManager.SetCurrentReferences(currentRacket, currentAnchor, currentModelToVis);
    }

    private void ToggleHand(UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor ray, GameObject model, bool state)
    {
        if (ray != null) ray.enabled = state;
        if (model != null) model.SetActive(state);
    }
}