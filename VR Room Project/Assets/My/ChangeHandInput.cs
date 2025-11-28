using UnityEngine;
using UnityEngine.InputSystem;

public class ChangeHandInput : MonoBehaviour
{
    [Header("References")]
    public GripManager gripManager;
    [Header("Inputs")]
    public InputActionReference inputLeft;
    public InputActionReference inputRight;

    private void OnEnable()
    {
        if (inputLeft != null && inputLeft.action != null)
        {
            inputLeft.action.Enable();
            inputLeft.action.performed += OnLeftInput;
        }
        if (inputRight != null && inputRight.action != null)
        {
            inputRight.action.Enable();
            inputRight.action.performed += OnRightInput;
        }
    }

    private void OnDisable()
    {
        if (inputLeft != null && inputLeft.action != null)
        {
            inputLeft.action.performed -= OnLeftInput;
            inputLeft.action.Disable();
        }
        if (inputRight != null && inputRight.action != null)
        {
            inputRight.action.performed -= OnRightInput;
            inputRight.action.Disable();
        }
    }

    private void OnLeftInput(InputAction.CallbackContext context)
    {
        if (WristMenuManager.IsMenuOpen) return;
        if (gripManager != null) gripManager.SetLeftHand();
    }

    private void OnRightInput(InputAction.CallbackContext context)
    {
        if (WristMenuManager.IsMenuOpen) return;
        if (gripManager != null) gripManager.SetRightHand();
    }
}