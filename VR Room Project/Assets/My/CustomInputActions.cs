using System.Diagnostics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class CustomInputActions : MonoBehaviour
{
    [Header("Input")]
    public InputActionReference customButton;

    [Header("Configurações de Interação")]
    public UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable objectToSummon;
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor handInteractor;
    public XRInteractionManager interactionManager;

    void Start()
    {
        if (customButton != null)
        {
            customButton.action.started += ButtonWasPressed;
            customButton.action.canceled += ButtonWasReleased;
        }
    }

    void OnDestroy()
    {
        if (customButton != null)
        {
            customButton.action.started -= ButtonWasPressed;
            customButton.action.canceled -= ButtonWasReleased;
        }
    }

    void ButtonWasPressed(InputAction.CallbackContext context)
    {
        
    }

    void ButtonWasReleased(InputAction.CallbackContext context)
    {
        SummonAndGrab();
    }

    void SummonAndGrab()
    {
        interactionManager.SelectExit((UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor) handInteractor, (UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable) objectToSummon);
        Rigidbody rb = objectToSummon.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        objectToSummon.transform.position = handInteractor.transform.position;
        objectToSummon.transform.rotation = handInteractor.transform.rotation;
        interactionManager.SelectEnter((UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor) handInteractor, (UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable) objectToSummon);
    }
}