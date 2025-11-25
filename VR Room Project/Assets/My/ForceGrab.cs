using System.Diagnostics;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ForceGrab : MonoBehaviour
{
    [Tooltip("Item a ser agarrado")]
    public UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable itemToGrab;

    [Tooltip("Interactor do controle")]
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor interactor;

    public void GrabItem()
    {
        UnityEngine.Debug.Log("Tentando agarrar item...");
        if (itemToGrab == null) UnityEngine.Debug.LogError("ItemToGrab é nulo!");
        if (interactor == null) UnityEngine.Debug.LogError("Interactor é nulo!");

        if (itemToGrab != null && interactor != null)
        {

            if (interactor.interactionManager == null)
            {
                return;
            }
            interactor.interactionManager.SelectEnter((UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor)interactor, (UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable)itemToGrab);
            UnityEngine.Debug.Log("Item agarrado com sucesso!");
        }
    }

    public void ReleaseItem()
    {
        if (itemToGrab != null && interactor != null)
        {
            interactor.interactionManager.SelectExit((UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor)interactor, (UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable)itemToGrab);
        }
    }
}