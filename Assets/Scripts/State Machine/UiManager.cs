using TGS;
using UnityEngine;

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
    
    // Enum for our states
    enum UIState { Idle, Build, Options }
    private UIState currentState = UIState.Idle;
    
    // Start with everything off
    void Start()
    {
        Debug.Log("UIManager Start: Initializing UI state to Idle");
        SetState(UIState.Idle); // Ensure we begin in Idle state
        Debug.Log($"UIManager Start: TGS enabled = {_tgs.enabled}, showCells = {_tgs.showCells}");
        _tgs.showCells = false;
        _tgs.enabled = false;
        
        Debug.Log("UIManager Start: Activating player camera");
        ActivateCamera(playerCamera, buildCamera, playerCameraInputController, buildCameraInputController);
    }

    // Core state-switching logic
    void SetState(UIState newState)
    {
        Debug.Log($"SetState: Attempting to switch to state {newState} from {currentState}");
        currentState = newState;
        buildMenuInventory.SetActive(currentState == UIState.Build);
        optionsMenu.SetActive(currentState == UIState.Options);
        
        if (currentState == UIState.Build)
        {
            Debug.Log("SetState: Entering Build mode");
            StartBuildModeLogic();
        }
        if (currentState == UIState.Options)
        {
            Debug.Log("SetState: Entering Options mode");
            StartOptionsModeLogic();
        }
        if (currentState == UIState.Idle)
        {
            Debug.Log("SetState: Entering Idle mode");
            StartIdleModeLogic();
        }
        // Add animation logic here if desired
        
        Debug.Log($"SetState: State changed to {currentState}");
    }

    #region State Logic
    // State Logic
    void StartBuildModeLogic()
    {
        Debug.Log("StartBuildModeLogic: Activating build menu and grid system");
        buildMenuInventory.SetActive(true);
        ToggleCameras();
        _tgs.showCells = true;
        _tgs.enabled = true;
        buildButton.SetActive(false);
        optionsMenu.SetActive(true);
        Debug.Log($"StartBuildModeLogic: TGS enabled = {_tgs.enabled}, showCells = {_tgs.showCells}, buildButton active = {buildButton.activeSelf}");
    }
    
    void StartOptionsModeLogic()
    {
        Debug.Log("StartOptionsModeLogic: Activating options menu and grid system");
        optionsMenu.SetActive(true);
        buildMenuInventory.SetActive(true);
        ToggleCameras();
        _tgs.showCells = true;
        _tgs.enabled = true;
        buildButton.SetActive(false);
        Debug.Log($"StartOptionsModeLogic: Cameras toggled, TGS enabled = {_tgs.enabled}, optionsMenu active = {optionsMenu.activeSelf}");
    }
    
    void StartIdleModeLogic()
    {
        Debug.Log("StartIdleModeLogic: Deactivating menus and grid system");
        optionsMenu.SetActive(false);
        //TODO : add check for build saved and no attached tiles or buildings ect on mouse ie "not in still got a object to be placed attached to mouse".
        ToggleCameras();
        buildMenuInventory.SetActive(false);
        _tgs.showCells = false;
        _tgs.enabled = false;
        buildButton.SetActive(true);
        Debug.Log($"StartIdleModeLogic: TGS disabled, buildButton active = {buildButton.activeSelf}");
    }
    
    #endregion
    
    #region Public Methods
    // Public methods for buttons to call
    public void EnterBuildMode()
    {
        Debug.Log($"EnterBuildMode: Current state = {currentState}");
        if (currentState != UIState.Build)
        {
            Debug.Log("EnterBuildMode: Switching to Build mode");
            SetState(UIState.Build);
        }
        SetState(UIState.Build); // Note: This line seems redundant as it's called regardless of the if condition
        Debug.Log("EnterBuildMode: Build mode set");
    }

    public void OpenOptions()
    {
        Debug.Log("OpenOptions: Switching to Options mode");
        SetState(UIState.Options);
        Debug.Log("OpenOptions: Options mode set");
    }

    public void ExitToIdle()
    {
        Debug.Log("ExitToIdle: Switching to Idle mode");
        SetState(UIState.Idle);
        Debug.Log("ExitToIdle: Idle mode set");
    }
    #endregion
    
    //Camera Switching
    
    // Public method to toggle cameras, can be called from other scripts or events
    public void ToggleCameras()
    {
        Debug.Log($"ToggleCameras: Player camera enabled = {playerCamera.enabled}, Build camera enabled = {buildCamera.enabled}");
        if (playerCamera.enabled)
        {
            Debug.Log("ToggleCameras: Switching to build camera");
            ActivateCamera(buildCamera, playerCamera, buildCameraInputController, playerCameraInputController);
        }
        else
        {
            Debug.Log("ToggleCameras: Switching to player camera");
            ActivateCamera(playerCamera, buildCamera, playerCameraInputController, buildCameraInputController);
        }
        Debug.Log($"ToggleCameras: After switch - Player camera enabled = {playerCamera.enabled}, Build camera enabled = {buildCamera.enabled}");
    }

    // Helper method to activate one camera and deactivate the other
    private void ActivateCamera(Camera camToActivate, Camera camToDeactivate, GameObject controllerToActivate, GameObject controllerToDeactivate)
    {
        Debug.Log($"ActivateCamera: Activating {camToActivate.name}, Deactivating {camToDeactivate.name}");
        
        // Enable the camera to activate
        camToActivate.enabled = true;
        // Enable its AudioListener if it exists
        AudioListener listenerActivate = camToActivate.GetComponent<AudioListener>();
        if (listenerActivate != null)
        {
            listenerActivate.enabled = true;
            Debug.Log($"ActivateCamera: Enabled AudioListener for {camToActivate.name}");
        }
        else
        {
            Debug.LogWarning($"ActivateCamera: No AudioListener found on {camToActivate.name}");
        }
        controllerToActivate.SetActive(true);
        Debug.Log($"ActivateCamera: Activated controller {controllerToActivate.name}");

        // Disable the camera to deactivate
        camToDeactivate.enabled = false;
        // Disable its AudioListener if it exists
        AudioListener listenerDeactivate = camToDeactivate.GetComponent<AudioListener>();
        if (listenerDeactivate != null)
        {
            listenerDeactivate.enabled = false;
            Debug.Log($"ActivateCamera: Disabled AudioListener for {camToDeactivate.name}");
        }
        else
        {
            Debug.LogWarning($"ActivateCamera: No AudioListener found on {camToDeactivate.name}");
        }
        controllerToDeactivate.SetActive(false);
        Debug.Log($"ActivateCamera: Deactivated controller {controllerToDeactivate.name}");
    }
}
}