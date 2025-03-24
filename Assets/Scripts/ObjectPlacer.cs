using TGS;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ObjectPlacer : MonoBehaviour
{
    public GameObject prefabToInstantiate; // Assign the prefab in the Inspector
    public Button spawnButton; // Assign the UI button in the Inspector
    public LayerMask terrainLayer; // Set this in the Inspector to the terrain's layer
    private GameObject instantiatedObject; // Reference to the instantiated object
    private bool isAttached = false;
    [SerializeField] public Camera buildCamera;
    
    [SerializeField] TMP_Text ValueText;
    //[SerializeField] Button tileAButton;
    // Placement counter controls
    public int tempPlacementcnt;
    public int placementcnt;
    public int buildLimtcnt = 5;
    
    // Testing toggles
    public bool cellIO;
    //[SerializeField] TerrainGridSystem _tgs;
    
    
    private TerrainGridSystem tgs;
    int cellIndex;
    int buttonIndex;

    void Start()
    {
        ValueText.text = buildLimtcnt.ToString();
        tgs = TerrainGridSystem.instance;
        tgs.OnCellClick += PlaceObject;
        //_tgs.showCells = false;
        
        
        
        
        if (buildCamera == null)
        {
            //Debug.LogError("Build Camera not assigned in the Inspector!");
        }
        else
        {
            //Debug.LogError("Build Camera ready");
        }
        
        // Add listener to the button
        if (spawnButton != null)
        {
            spawnButton.onClick.AddListener(OnSpawnButtonClick);
        }
        else
        {
            //Debug.LogError("Spawn button not assigned in the Inspector!");
        }
    }
    
    void OnCellClick (TerrainGridSystem grid, int cell, int button) 
    {
        if (buttonIndex == 1) {
            print("Right clicked on cell #" + cellIndex);
        }												
    }

    void Update()
    {
        if (isAttached && instantiatedObject != null)
        {
            // Move the object with the mouse
            MoveObjectWithMouse();

            // Check for right-click to place the object
            if (Input.GetMouseButtonDown(1)) // Right-click
            {
                //PlaceObjectOnTerrain();
                PlaceObject(tgs, cellIndex, buttonIndex);
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
                isAttached = true;
            }
            else
            {
                //Debug.LogWarning("Could not determine spawn position. Ensure the terrain is on the correct layer.");
            }
        }
    }

    Vector3 GetMouseWorldPosition()
    {
        // Cast a ray from the camera through the mouse position
        Ray ray = buildCamera.ScreenPointToRay(Input.mousePosition);
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

    // void PlaceObjectOnTerrain()
    // {
    //     Vector3 finalPosition = GetMouseWorldPosition();
    //     if (finalPosition != Vector3.zero)
    //     {
    //         // Place the object directly on the terrain at the mouse position
    //         instantiatedObject.transform.position = finalPosition;
    //
    //         // Optional: Align the object to the terrain's surface normal
    //         RaycastHit hit;
    //         if (Physics.Raycast(finalPosition + Vector3.up * 1f, Vector3.down, out hit, 2f, terrainLayer))
    //         {
    //             instantiatedObject.transform.up = hit.normal; // Align to terrain surface
    //         }
    //     }
    //
    //     // Stop dragging and clear the reference
    //     isAttached = false;
    //     instantiatedObject = null; // Allow spawning a new object
    // }
    // Working currently new code below to replace above method eventualy to handle the placement of the tile object
    public void PlaceObject(TerrainGridSystem grid, int cell, int button)
    {
        if (isAttached)
        {
            
            if (button == 1)
            {
                placementcnt += 1;
                //pendingObject = null;
                instantiatedObject.transform.position = tgs.CellGetPosition(cell);
                instantiatedObject = null;
                tempPlacementcnt -= 1;
                buildLimtcnt -= 1;
                
                //tgs.CellSetTag(cellIndex, 1);
                //print("Cell Index # " + cellIndex + "Tag # " + tag);
                
                
                //_tgs.CellGetPosition(_cellIndex).tag = 1;

                if (buildLimtcnt == 0)
                {
                    spawnButton.interactable = false;
                    //tileAButtonImage.color = tempColor;
                    //tempColor.a = 30f;
                    //tileAButtonImage.color = tempColor;
                }
                ValueText.text = buildLimtcnt.ToString(); 

                //if (pendingObject = null)
                    //print("None selected");
            }
        }
    }
}