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
        [SerializeField] private Camera buildCamera;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private GameObject buildCameraInputController;
        [SerializeField] private GameObject playerCameraInputController;
        [SerializeField] private TerrainGridSystem _tgs;

        #endregion
        
        
        // Enum for our states
        enum UIState { Idle, Build, Options }
        private UIState currentState = UIState.Idle;
        

        // Start with everything off
        void Start()
        {
            SetState(UIState.Idle); // Ensure we begin in Idle state
            //Debug.Log("To God be the Glory");
            _tgs.showCells = false;
            _tgs.enabled = false;
            
            ActivateCamera(playerCamera, buildCamera, playerCameraInputController, buildCameraInputController);
        }

        // Core state-switching logic
        void SetState(UIState newState)
        {
            currentState = newState;
            buildMenuInventory.SetActive(currentState == UIState.Build);
            optionsMenu.SetActive(currentState == UIState.Options);
            
            if (currentState == UIState.Build) StartBuildModeLogic();
            if (currentState == UIState.Options) StartOptionsModeLogic(); 
            if (currentState == UIState.Idle) StartIdleModeLogic();
            // Add animation logic here if desired
            
            // Optional: Log state for debugging
            Debug.Log("Current State: " + currentState);
        }

        #region State Logic
        // State Logic
        void StartBuildModeLogic()
        {
            buildMenuInventory.SetActive(true);
            //ToggleCameras();
            _tgs.showCells = true;
            _tgs.enabled = true;
            buildButton.SetActive(false);
            optionsMenu.SetActive(true);
        }
        
        void StartOptionsModeLogic()
        {
            optionsMenu.SetActive(true);
            buildMenuInventory.SetActive(true);
            ToggleCameras();
            _tgs.showCells = true;
            _tgs.enabled = true;
            buildButton.SetActive(false);
        }
        void StartIdleModeLogic()
        {
            optionsMenu.SetActive(false);
            //TODO : add check for build saved and no attached tiles or buildings ect on mouse ie "not in still got a object to be placed attached to mouse".
            ToggleCameras();
            buildMenuInventory.SetActive(false);
            _tgs.showCells = false;
            _tgs.enabled = false;
            buildButton.SetActive(true);
        }
        
        #endregion
        
        
        #region Public Methods
        // Public methods for buttons to call
        public void EnterBuildMode()
        {
            if (currentState != UIState.Build) SetState(UIState.Build);
            SetState(UIState.Build);
        }

        public void OpenOptions()
        {
            SetState(UIState.Options);
        }

        public void ExitToIdle()
        {
            SetState(UIState.Idle);
        }
        #endregion
        
     //Camera Switching
     
     // Public method to toggle cameras, can be called from other scripts or events
     public void ToggleCameras()
     {
         if (playerCamera.enabled)
         {
             // If playercamera is active, switch to buildcamera
             ActivateCamera(buildCamera, playerCamera, buildCameraInputController, playerCameraInputController);
         }
         else
         {
             // If buildcamera is active, switch to playercamera
             ActivateCamera(playerCamera, buildCamera, playerCameraInputController, buildCameraInputController);
         }
     }

     // Helper method to activate one camera and deactivate the other
     private void ActivateCamera(Camera camToActivate, Camera camToDeactivate, GameObject controllerToActivate, GameObject controllerToDeactivate)
     {
         // Enable the camera to activate
         camToActivate.enabled = true;
         // Enable its AudioListener if it exists
         AudioListener listenerActivate = camToActivate.GetComponent<AudioListener>();
         if (listenerActivate != null) listenerActivate.enabled = true;
         controllerToActivate.SetActive(true);

         // Disable the camera to deactivate
         camToDeactivate.enabled = false;
         // Disable its AudioListener if it exists
         AudioListener listenerDeactivate = camToDeactivate.GetComponent<AudioListener>();
         if (listenerDeactivate != null) listenerDeactivate.enabled = false;
         controllerToDeactivate.SetActive(false);
     }
        
        
    }
}
