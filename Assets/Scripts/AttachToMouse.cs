using UnityEngine;
using UnityEngine.UI;

public class ObjectPlacer : MonoBehaviour
{
    public GameObject prefabToInstantiate; // Assign the prefab in the Inspector
    public Button spawnButton; // Assign the UI button in the Inspector
    public LayerMask terrainLayer; // Set this in the Inspector to the terrain's layer
    private GameObject instantiatedObject; // Reference to the instantiated object
    private bool isDragging = false;

    void Start()
    {
        // Add listener to the button
        if (spawnButton != null)
        {
            spawnButton.onClick.AddListener(OnSpawnButtonClick);
        }
        else
        {
            Debug.LogError("Spawn button not assigned in the Inspector!");
        }
    }

    void Update()
    {
        if (isDragging && instantiatedObject != null)
        {
            // Move the object with the mouse
            MoveObjectWithMouse();

            // Check for right-click to place the object
            if (Input.GetMouseButtonDown(1)) // Right-click
            {
                PlaceObjectOnTerrain();
            }
        }
    }

    void OnSpawnButtonClick()
    {
        if (instantiatedObject == null) // Only spawn if no object is currently being dragged
        {
            // Get initial mouse position in world space
            Vector3 spawnPosition = GetMouseWorldPosition();
            if (spawnPosition != Vector3.zero) // Ensure a valid position was found
            {
                // Instantiate the object at the mouse position
                instantiatedObject = Instantiate(prefabToInstantiate, spawnPosition, Quaternion.identity);
                isDragging = true;
            }
            else
            {
                Debug.LogWarning("Could not determine spawn position. Ensure the terrain is on the correct layer.");
            }
        }
    }

    Vector3 GetMouseWorldPosition()
    {
        // Cast a ray from the camera through the mouse position
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, terrainLayer))
        {
            return hit.point; // Return the point where the ray hits the terrain
        }

        return Vector3.zero; // Return zero if no hit (invalid position)
    }

    void MoveObjectWithMouse()
    {
        // Update the object's position to follow the mouse
        Vector3 newPosition = GetMouseWorldPosition();
        if (newPosition != Vector3.zero)
        {
            instantiatedObject.transform.position = newPosition;
        }
    }

    void PlaceObjectOnTerrain()
    {
        Vector3 finalPosition = GetMouseWorldPosition();
        if (finalPosition != Vector3.zero)
        {
            // Place the object directly on the terrain at the mouse position
            instantiatedObject.transform.position = finalPosition;

            // Optional: Align the object to the terrain's surface normal
            RaycastHit hit;
            if (Physics.Raycast(finalPosition + Vector3.up * 1f, Vector3.down, out hit, 2f, terrainLayer))
            {
                instantiatedObject.transform.up = hit.normal; // Align to terrain surface
            }
        }

        // Stop dragging and clear the reference
        isDragging = false;
        instantiatedObject = null; // Allow spawning a new object
    }
}