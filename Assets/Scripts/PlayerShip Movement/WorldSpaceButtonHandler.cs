using UnityEngine;
using UnityEngine.UIElements;

// Add namespace for PlayerShipController

namespace PlayerShip_Movement
{
    public class WorldSpaceButtonHandler : MonoBehaviour
    {
        // [SerializeField] private UIDocument worldSpaceUIDocument; // Reference to the UIDocument
        // [SerializeField] private Canvas canvas; // Reference to the World Space Canvas
        // [SerializeField] private PlayerShipController playerShipController; // Reference to PlayerShipController
        //
        // private void Awake()
        // {
        //     Debug.Log("WorldSpaceButtonHandler: Awake called.");
        //
        //     // Validate references
        //     if (worldSpaceUIDocument == null)
        //     {
        //         Debug.LogWarning("World Space UIDocument is not assigned.");
        //         return;
        //     }
        //
        //     if (canvas == null)
        //     {
        //         Debug.LogWarning("Canvas is not assigned.");
        //         return;
        //     }
        //
        //     if (playerShipController == null)
        //     {
        //         Debug.LogWarning("PlayerShipController is not assigned. Attempting to find in scene.");
        //         playerShipController = FindFirstObjectByType<PlayerShipController>(); // Updated method
        //         if (playerShipController == null)
        //         {
        //             Debug.LogWarning("PlayerShipController not found in scene.");
        //             return;
        //         }
        //     }
        //
        //     if (canvas.renderMode != RenderMode.WorldSpace)
        //     {
        //         Debug.LogWarning("Canvas is not in World Space mode. Updating.");
        //         canvas.renderMode = RenderMode.WorldSpace;
        //     }
        //
        //     // Get the root visual element
        //     VisualElement root = worldSpaceUIDocument.rootVisualElement;
        //     Debug.Log("WorldSpaceButtonHandler: Root visual element retrieved.");
        //
        //     // Query the button (adjust to match UXML name)
        //     Button myButton = root.Q<Button>("WorldSpaceButton"); // Update if name differs
        //     if (myButton != null)
        //     {
        //         Debug.Log("WorldSpaceButtonHandler: Button 'WorldSpaceButton' found.");
        //         // Register click event
        //         myButton.clicked += () =>
        //         {
        //             Debug.Log("World Space Button Clicked!");
        //             playerShipController.StartTween(); // Call StartTween
        //         };
        //     }
        //     else
        //     {
        //         Debug.LogWarning("Button 'WorldSpaceButton' not found in UI Document. Check UXML name.");
        //     }
        // }
    }
}