using Debug = UnityEngine.Debug;
using TGS;
using UnityEngine;
using UnityEngine.UI; // Added for CanvasGroup and Button

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
    // Serialized fields for the close button and CanvasGroup
    [SerializeField] private Button closeButton; // Close button on Build panel
    [SerializeField] private CanvasGroup buildMenuCanvasGroup; // CanvasGroup for Build panel
    [SerializeField] private CanvasGroup[] additionalUICanvasGroups; // New list for other UI elements to grey out
    #endregion

    enum UIState { Idle, Build, Options }
    private UIState currentState = UIState.Idle;

    void Start()
    {
        Debug.Assert(_tgs != null, "TerrainGridSystem is not assigned!");
        Debug.Assert(buildCamera != null, "BuildCamera is not assigned!");
        Debug.Assert(playerCamera != null, "PlayerCamera is not assigned!");
        Debug.Assert(closeButton != null, "CloseButton is not assigned!");
        Debug.Assert(buildMenuCanvasGroup != null, "BuildMenuCanvasGroup is not assigned!");
        // Check additional UI CanvasGroups
        foreach (var canvasGroup in additionalUICanvasGroups)
        {
            Debug.Assert(canvasGroup != null, $"Additional UI CanvasGroup is not assigned in {gameObject.name}!");
        }
        SetState(UIState.Idle);
        ActivateCamera(playerCamera, buildCamera, playerCameraInputController, buildCameraInputController);
        
        // Add listener to close button
        closeButton.onClick.AddListener(OpenOptions);
    }

    void SetState(UIState newState)
    {
        currentState = newState;

        // Modified to keep buildMenuInventory active in Options state
        buildMenuInventory.SetActive(newState == UIState.Build || newState == UIState.Options);
        optionsMenu.SetActive(newState == UIState.Options);
        buildButton.SetActive(newState == UIState.Idle);
        _tgs.enabled = newState == UIState.Build;
        _tgs.showCells = newState == UIState.Build;

        // Adjust Build panel interactivity and appearance
        if (newState == UIState.Options)
        {
            // Grey out and disable interactions on Build panel
            buildMenuCanvasGroup.alpha = 0.5f; // Semi-transparent to indicate disabled
            buildMenuCanvasGroup.interactable = false;
            buildMenuCanvasGroup.blocksRaycasts = false;

            // Grey out and disable interactions for additional UI elements
            foreach (var canvasGroup in additionalUICanvasGroups)
            {
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 0.5f;
                    canvasGroup.interactable = false;
                    canvasGroup.blocksRaycasts = false;
                }
            }
        }
        else
        {
            // Restore Build panel to normal
            buildMenuCanvasGroup.alpha = 1f;
            buildMenuCanvasGroup.interactable = true;
            buildMenuCanvasGroup.blocksRaycasts = true;

            // Restore additional UI elements to normal
            foreach (var canvasGroup in additionalUICanvasGroups)
            {
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 1f;
                    canvasGroup.interactable = true;
                    canvasGroup.blocksRaycasts = true;
                }
            }
        }

        ToggleCameras();
        Debug.Log($"MainMenuDataIOManager: State changed to {currentState}");
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

    // New method to return to Build state from Options
    public void ExitOptionsToBuild()
    {
        if (currentState == UIState.Options)
            SetState(UIState.Build);
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