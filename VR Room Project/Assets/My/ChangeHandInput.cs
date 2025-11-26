using UnityEngine;
using UnityEngine.InputSystem;


public class ChangeHandInput : MonoBehaviour
{
    [Tooltip("Input Hand Swap")]
    public InputActionReference customButton;
    [Tooltip("Check Lobby")]
    public GameObject lobby;
    [Tooltip("Racket Left")]
    public GameObject racketLeft;
    [Tooltip("Racket Right")]
    public GameObject racketRight;
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
        if (WristMenuManager.IsMenuOpen)
        {
            return;
        }
        if (racketRight.activeSelf) // se racket right, então racket left
        {
            PerformSwap(currentRacket: racketRight, newRacket: racketLeft, currentHandRay: rightRayInteractor, currentHandModel: rightHandModel, newHandRay: leftRayInteractor, newHandModel: leftHandModel);
        }
        else if (racketLeft.activeSelf) // senao faz o contrario
        {
            PerformSwap(currentRacket: racketLeft, newRacket: racketRight, currentHandRay: leftRayInteractor, currentHandModel: leftHandModel, newHandRay: rightRayInteractor, newHandModel: rightHandModel);
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