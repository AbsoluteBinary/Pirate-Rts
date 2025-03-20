using TGS;
using UnityEngine;

public class BuildModeControler : MonoBehaviour
{
    [SerializeField] private GameObject buildButton;
    [SerializeField] private GameObject buildMenuInventory;
    //[SerializeField] private GameObject buildMenuIO;
    [SerializeField] private Camera buildCamera;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameObject buildCameraInputContoller;
    [SerializeField] private GameObject playerCameraInputContoller;

    [SerializeField] TerrainGridSystem _tgs;
    
    // Called when Build mode button clicked
    public void EnterBuildModeIO()
    {
        
        buildMenuInventory.SetActive(true);
        ToggleCameras();
        _tgs.showCells = true;
        _tgs.enabled = true;
        buildButton.SetActive(false);
    }
    // Called when the build inventory ui close button is clicked
    public void ExitBuildModeIO()
    {
        //TODO : add check for build saved and no attached tiles or buildings ect on mouse ie "not in still got a object to be placed attached to mouse".
        ToggleCameras();
        buildMenuInventory.SetActive(false);
        _tgs.showCells = false;
        _tgs.enabled = false;
        buildButton.SetActive(true);
        
        
        
    }
    

    // Called when the scene starts
    void Start()
    {
        Debug.Log("God is Great");
        _tgs.showCells = false;
        _tgs.enabled = false;
        //_objectPlacer._tgs.gameObject.SetActive(false);
        //buildMenuInventory.SetActive(false);
        // Initially activate playercamera and deactivate buildcamera
        ActivateCamera(playerCamera, buildCamera, playerCameraInputContoller, buildCameraInputContoller);
        // buildCameraInputContoller.SetActive(false);
        // playerCameraInputContoller.SetActive(true);

    }

    // Called every frame
    // void Update()
    // {
    //     // Toggle cameras when the "T" key is pressed
    //     if (Input.GetKeyDown(KeyCode.T))
    //     {
    //         ToggleCameras();
    //         Debug.Log("Toggle camera");
    //     }
    // }

    // Public method to toggle cameras, can be called from other scripts or events
    public void ToggleCameras()
    {
        if (playerCamera.enabled)
        {
            // If playercamera is active, switch to buildcamera
            ActivateCamera(buildCamera, playerCamera, buildCameraInputContoller, playerCameraInputContoller);
        }
        else
        {
            // If buildcamera is active, switch to playercamera
            ActivateCamera(playerCamera, buildCamera, playerCameraInputContoller, buildCameraInputContoller);
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
