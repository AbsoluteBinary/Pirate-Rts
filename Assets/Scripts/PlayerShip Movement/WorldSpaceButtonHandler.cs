using UnityEngine;
using UnityEngine.UIElements;

namespace PlayerShip_Movement
{
    public class WorldSpaceButtonHandler : MonoBehaviour
    {
        [SerializeField] private UIDocument worldSpaceUIDocument; // Reference to the UIDocument
        [SerializeField] private Canvas canvas; // Reference to the World Space Canvas

        private void Awake()
        {
            // Log to confirm script execution
            Debug.Log("WorldSpaceButtonHandler: Awake called.");

            // Validate references
            if (worldSpaceUIDocument == null)
            {
                Debug.LogWarning("World Space UIDocument is not assigned in WorldSpaceButtonHandler.");
                return;
            }

            if (canvas == null)
            {
                Debug.LogWarning("Canvas is not assigned in WorldSpaceButtonHandler.");
                return;
            }

            // Ensure Canvas is in World Space
            if (canvas.renderMode != RenderMode.WorldSpace)
            {
                Debug.LogWarning("Canvas is not in World Space mode. Updating.");
                canvas.renderMode = RenderMode.WorldSpace;
            }

            // Get the root visual element
            VisualElement root = worldSpaceUIDocument.rootVisualElement;
            Debug.Log("WorldSpaceButtonHandler: Root visual element retrieved.");

            // Query the button (update 'MyWorldSpaceButton' to match UXML)
            Button myButton = root.Q<Button>("MyWorldSpaceButton");
            if (myButton != null)
            {
                Debug.Log("WorldSpaceButtonHandler: Button 'MyWorldSpaceButton' found.");
                // Register click event
                myButton.clicked += () =>
                {
                    Debug.Log("World Space Button Clicked!");
                };
            }
            else
            {
                Debug.LogWarning("Button 'MyWorldSpaceButton' not found in UI Document. Check UXML name.");
            }
        }
    }
}