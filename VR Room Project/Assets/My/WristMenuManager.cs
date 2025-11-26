using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class WristMenuManager : MonoBehaviour
{
    public static bool IsMenuOpen = false;

    [Header("Inputs")]
    public InputActionReference toggleMenuButton;
    public InputActionReference shortcutAction1;
    public InputActionReference shortcutAction2;

    [Header("Referencias")]
    public Transform targetHand;
    public Transform headCamera;

    [Header("Estados do Jogo")]
    public GameObject lobbyObject;
    public GameObject inGameObject;
    public GameObject deadObject;

    [Header("UI")]
    public GameObject menuCanvas;
    public GameObject lobbyPanel;
    public GameObject inGamePanel;
    public GameObject deadPanel;

    [Header("Visuais")]
    public Vector3 positionOffset = new Vector3(0, 0.1f, 0);
    public float smoothSpeed = 15f;

    [Space(10)]
    [Header("Eventos: LOBBY")]
    public UnityEvent onLobbyAction1;
    public UnityEvent onLobbyAction2;

    [Header("Eventos: IN-GAME")]
    public UnityEvent onInGameAction1;
    public UnityEvent onInGameAction2;

    [Header("Eventos: MORTO")]
    public UnityEvent onDeadAction1;
    public UnityEvent onDeadAction2;

    private void Start()
    {
        AudioListener.volume = 1f;
    }

    private void Awake()
    {
        IsMenuOpen = false;
        if (menuCanvas) menuCanvas.SetActive(false);
    }

    private void OnEnable()
    {
        if (toggleMenuButton)
        {
            toggleMenuButton.action.Enable();
            toggleMenuButton.action.performed += OnToggleMenu;
        }
        if (shortcutAction1) shortcutAction1.action.Enable();
        if (shortcutAction2) shortcutAction2.action.Enable();
    }

    private void OnDisable()
    {
        if (toggleMenuButton) toggleMenuButton.action.performed -= OnToggleMenu;
    }

    private void Update()
    {
        FollowHand();
        if (IsMenuOpen)
        {
            CheckShortcuts();
        }
    }

    private void FollowHand()
    {
        if (targetHand == null || headCamera == null) return;
        Vector3 targetPos = targetHand.TransformPoint(positionOffset);
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * smoothSpeed);
        Vector3 directionToHead = transform.position - headCamera.position;
        directionToHead.y = 0;
        if (directionToHead != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(directionToHead);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * smoothSpeed);
        }
    }

    private void OnToggleMenu(InputAction.CallbackContext context)
    {
        IsMenuOpen = !IsMenuOpen;
        if (menuCanvas) menuCanvas.SetActive(IsMenuOpen);
        if (IsMenuOpen)
        {
            if (targetHand != null) transform.position = targetHand.TransformPoint(positionOffset);
            UpdateContextContent();
        }
    }

    private void UpdateContextContent()
    {
        if (lobbyPanel) lobbyPanel.SetActive(false);
        if (inGamePanel) inGamePanel.SetActive(false);
        if (deadPanel) deadPanel.SetActive(false);

        if (lobbyObject != null && lobbyObject.activeSelf)
        {
            if (lobbyPanel) lobbyPanel.SetActive(true);
        }
        else if (deadObject != null && deadObject.activeSelf)
        {
            if (deadPanel) deadPanel.SetActive(true);
        }
        else if (inGameObject != null && inGameObject.activeSelf)
        {
            if (inGamePanel) inGamePanel.SetActive(true);
        }
    }

    private void CheckShortcuts()
    {
        if (shortcutAction1 != null && shortcutAction1.action.WasPressedThisFrame())
        {
            if (lobbyObject != null && lobbyObject.activeSelf)
            {
                onLobbyAction1.Invoke();
            }
            else if (inGameObject != null && inGameObject.activeSelf)
            {
                onInGameAction1.Invoke();
            }
            else if (deadObject != null && deadObject.activeSelf)
            {
                onDeadAction1.Invoke();
            }
        }

        if (shortcutAction2 != null && shortcutAction2.action.WasPressedThisFrame())
        {
            if (lobbyObject != null && lobbyObject.activeSelf)
            {
                onLobbyAction2.Invoke();
            }
            if (inGameObject != null && inGameObject.activeSelf)
            {
                onInGameAction2.Invoke();
            }
            else if (deadObject != null && deadObject.activeSelf)
            {
                onDeadAction2.Invoke();
            }
        }
    }
    public void QuitGame()
    {
        UnityEngine.Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void RestartObjectSequence(GameObject objToRestart)
    {
        StartCoroutine(RestartRoutine(objToRestart));
    }

    public void ToggleMute()
    {
        if (AudioListener.volume > 0)
        {
            AudioListener.volume = 0f;
        }
        else
        {
            AudioListener.volume = 1f;
        }
    }

    private IEnumerator RestartRoutine(GameObject obj)
    {
        if (obj != null)
        {
            obj.SetActive(false);
            yield return null;
            obj.SetActive(true);
        }
    }
}