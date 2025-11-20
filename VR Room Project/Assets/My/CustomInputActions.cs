using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class CustomInputActions : MonoBehaviour
{
    [Header("Input")]
    public InputActionReference customButton;

    [Header("Configurações de Interação")]
    public XRGrabInteractable objectToSummon;
    public XRBaseInteractor handInteractor;
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
        if (handInteractor.hasSelection)
        {
            interactionManager.SelectExit((IXRSelectInteractor)handInteractor, (IXRSelectInteractable)objectToSummon);
        }
    }

    void ButtonWasReleased(InputAction.CallbackContext context)
    {
        StartCoroutine(SummonAndGrabRoutine());
    }

    IEnumerator SummonAndGrabRoutine()
    {
        if (objectToSummon == null || handInteractor == null || interactionManager == null) yield break;
        Rigidbody rb = objectToSummon.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = false;
        }
        objectToSummon.transform.position = handInteractor.transform.position;
        objectToSummon.transform.rotation = handInteractor.transform.rotation;
        yield return new WaitForFixedUpdate();
        interactionManager.SelectEnter((IXRSelectInteractor)handInteractor, (IXRSelectInteractable)objectToSummon);
        objectToSummon.selectExited.AddListener(OnObjectReleased);
    }

    void OnObjectReleased(SelectExitEventArgs args)
    {
        Rigidbody rb = args.interactableObject.transform.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = true;
            rb.isKinematic = false;
        }
        objectToSummon.selectExited.RemoveListener(OnObjectReleased);
    }
}