using TGS;
using UnityEngine;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace State_Machine
{
public class UIManager : MonoBehaviour
{
    #region Reference Variables
    [SerializeField] private GameObject buildButton;
    [SerializeField] private GameObject buildMenuInventory;
    [SerializeField] private GameObject optionsMenu;
    [SerializeField] private GameObject buildCameraInputController;
    [SerializeField] private GameObject playerCameraInputController;
    [SerializeField] private TerrainGridSystem _tgs;
    [SerializeField] private Camera buildCamera;
    [SerializeField] private Camera playerCamera;
    #endregion

    enum UIState { Idle, Build, Options }
    private UIState currentState = UIState.Idle;

    void Start()
    {
        Debug.Assert(_tgs != null, "TerrainGridSystem is not assigned!");
        Debug.Assert(buildCamera != null, "BuildCamera is not assigned!");
        Debug.Assert(playerCamera != null, "PlayerCamera is not assigned!");
        SetState(UIState.Idle);
        ActivateCamera(playerCamera, buildCamera, playerCameraInputController, buildCameraInputController);
    }

    void SetState(UIState newState)
    {
        currentState = newState;
        buildMenuInventory.SetActive(newState == UIState.Build);
        optionsMenu.SetActive(newState == UIState.Options);
        buildButton.SetActive(newState == UIState.Idle);
        _tgs.enabled = newState == UIState.Build;
        _tgs.showCells = newState == UIState.Build;
        ToggleCameras();
        Debug.Log($"UIManager: State changed to {currentState}"); // Minimal log for state changes
    }

    #region Public Methods
    public void EnterBuildMode()
    {
        if (currentState != UIState.Build)
            SetState(UIState.Build);
    }

    public void OpenOptions()
    {
        SetState(UIState.Options);
    }

    public void ExitToIdle()
    {
        // TODO: Add check for build saved and no attached objects
        SetState(UIState.Idle);
    }
    #endregion

    public void ToggleCameras()
    {
        if (currentState == UIState.Build || currentState == UIState.Options)
            ActivateCamera(buildCamera, playerCamera, buildCameraInputController, playerCameraInputController);
        else
            ActivateCamera(playerCamera, buildCamera, playerCameraInputController, buildCameraInputController);
    }

    private void ActivateCamera(Camera camToActivate, Camera camToDeactivate, GameObject controllerToActivate, GameObject controllerToDeactivate)
    {
        camToActivate.enabled = true;
        var listenerActivate = camToActivate.GetComponent<AudioListener>();
        if (listenerActivate != null)
            listenerActivate.enabled = true;
        else
            Debug.LogWarning($"No AudioListener found on {camToActivate.name}");
        controllerToActivate.SetActive(true);

        camToDeactivate.enabled = false;
        var listenerDeactivate = camToDeactivate.GetComponent<AudioListener>();
        if (listenerDeactivate != null)
            listenerDeactivate.enabled = false;
        else
            Debug.LogWarning($"No AudioListener found on {camToDeactivate.name}");
        controllerToDeactivate.SetActive(false);
    }
}
}