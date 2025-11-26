using UnityEngine;
using UnityEngine.InputSystem;


public class ChangeHandInput : MonoBehaviour
{
    [Tooltip("Input Hand Swap")]
    public InputActionReference customButton;
    [Tooltip("Check Lobby")]
    public GameObject lobby;
    [Tooltip("Racket Shakehand Left")]
    public GameObject racketShakehandLeft;
    [Tooltip("Racket Penhold Left")]
    public GameObject racketPenholdLeft;
    [Tooltip("Racket Shakehand Right")]
    public GameObject racketShakehandRight;
    [Tooltip("Racket Penhold Right")]
    public GameObject racketPenholdRight;
    [Tooltip("Left Ray Interactor")]
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor leftRayInteractor;
    [Tooltip("Left Hand Model")]
    public GameObject leftHandModel;
    [Tooltip("Right Ray Interactor")]
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor rightRayInteractor;
    [Tooltip("Right Hand Model")]
    public GameObject rightHandModel;

    private void OnEnable()
    {
        if (customButton != null && customButton.action != null)
        {
            customButton.action.Enable();
            customButton.action.performed += OnSwapButtonData;
        }
    }

    private void OnDisable()
    {
        if (customButton != null && customButton.action != null)
        {
            customButton.action.performed -= OnSwapButtonData;
            customButton.action.Disable();
        }
    }

    private void OnSwapButtonData(InputAction.CallbackContext context)
    {
        SwapHand();
    }

    public void SwapHand()
    {
        if (lobby.activeSelf)
        {
            return;
        }
        if (racketShakehandRight.activeSelf) // se shakehand right, então shakehand left
        {
            PerformSwap(
                currentRacket: racketShakehandRight,
                newRacket: racketShakehandLeft,
                currentHandRay: rightRayInteractor,
                currentHandModel: rightHandModel,
                newHandRay: leftRayInteractor,
                newHandModel: leftHandModel
            );
        }
        else if (racketPenholdRight.activeSelf) // se penhold right, então penhold left
        {
            PerformSwap(
                currentRacket: racketPenholdRight,
                newRacket: racketPenholdLeft,
                currentHandRay: rightRayInteractor,
                currentHandModel: rightHandModel,
                newHandRay: leftRayInteractor,
                newHandModel: leftHandModel
            );
        }
        else if (racketShakehandLeft.activeSelf)  // se shakehand left, então shakehand right
        {
            PerformSwap(
                currentRacket: racketShakehandLeft,
                newRacket: racketShakehandRight,
                currentHandRay: leftRayInteractor,
                currentHandModel: leftHandModel,
                newHandRay: rightRayInteractor,
                newHandModel: rightHandModel
            );
        }
        else if (racketPenholdLeft.activeSelf)  // se penhold left, então penhold right
        {
            PerformSwap(
                currentRacket: racketPenholdLeft,
                newRacket: racketPenholdRight,
                currentHandRay: leftRayInteractor,
                currentHandModel: leftHandModel,
                newHandRay: rightRayInteractor,
                newHandModel: rightHandModel
            );
        }
    }

    private void PerformSwap(GameObject currentRacket, GameObject newRacket, UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor currentHandRay, GameObject currentHandModel, UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor newHandRay, GameObject newHandModel)
    {
        currentRacket.SetActive(false); // desativa raquete anterior
        newRacket.SetActive(true); // ativa raquete nova
        if (newHandRay != null) newHandRay.enabled = false; // configura nova mao
        if (newHandModel != null) newHandModel.SetActive(false);
        if (currentHandModel != null) currentHandModel.SetActive(true);
        if (currentHandRay != null && lobby.activeSelf) currentHandRay.enabled = true; // reativa ray da mao livre se tiver no lobby
    }
}