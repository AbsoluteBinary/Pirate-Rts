using TGS;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlacementControl : MonoBehaviour
{
    [SerializeField] GameObject[] pooledObjects = new GameObject[3];
    private  GameObject pendingObject;
    private Vector3 currentMousePos;
    private RaycastHit _raycastHit;
    public Camera _camera;
    [SerializeField]
    private LayerMask _layerMask;
    private Cell cell;
    private Color tempColor;
    [SerializeField] Image tileAButtonImage;

    //[SerializeField] private int PoolSize;

    private bool isAttached = false;
    private TerrainGridSystem _tgs;
    int _cellIndex;
    int _buttonIndex;
    [SerializeField] TMP_Text ValueText;
    [SerializeField] Button tileAButton;
    // Placement counter controls
    public int tempPlacementcnt;
    public int placementcnt;
    public int buildLimtcnt = 5;
    
    
    void Start()
    {
        _camera = Camera.main;
        _tgs = TerrainGridSystem.instance;
        _tgs.OnCellClick += PlaceObject;
        // Button image get set
        tileAButtonImage = tileAButtonImage.GetComponent<Image>();
        //tileAButtonImage.color.a = 30f;
        _tgs.OnCellClick += OnCellClick;
    }
    
    void OnCellClick (TerrainGridSystem grid, int cellIndex, int buttonIndex) 
    {
			if (buttonIndex == 1) {
				print("Right clicked on cell #" + cellIndex);
			}												
    }

    // Update is called once per frame
    void Update()
    {
        if (pendingObject != null)
        {
            pendingObject.transform.position = currentMousePos;
        }
        // Update checking for mouse click (anywhere) then calling placeobject function
        // if tile is selected it is currently attached to mouse position on the ground
        if (Input.GetMouseButtonDown(1))
        {
            PlaceObject(_tgs, _cellIndex, _buttonIndex);
        }
        ValueText.text = buildLimtcnt.ToString(); 
        
        Vector3 camPos = _camera.transform.position;
    }

    private void FixedUpdate()
    {
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out _raycastHit, 1000, _layerMask))
        {
            currentMousePos = _raycastHit.point;
        }
    }

    private void PlaceObject(TerrainGridSystem tgs, int cellIndex, int buttonIndex)
    {
        if (isAttached)
        {
            if (_buttonIndex == 1)
            {
                placementcnt += 1;
                //pendingObject = null;
                pendingObject.transform.position = _tgs.CellGetPosition(_cellIndex);
                pendingObject = null;
                tempPlacementcnt -= 1;
                buildLimtcnt -= 1;
                
                _tgs.CellSetTag(_cellIndex, 1);
                print("Cell Index # " + _cellIndex + "Tag # " + tag);

                if (buildLimtcnt == 0)
                {
                    tileAButton.interactable = false;
                    tileAButtonImage.color = tempColor;
                    tempColor.a = 30f;
                    tileAButtonImage.color = tempColor;
                }

                if (pendingObject == null)
                    print("None selected");
            }
        }
        //GameObject go = pendingObject;
        //go.transform.position = _tgs.CellGetPosition(_cellIndex);
        
    }
    // If tile image (button) is clicked this function is Executed
    // instantiating the tile and attaching it to mouse on ground layer
    public void SelectObject(int index)
    {
        tempPlacementcnt += 1;
        pendingObject = Instantiate(pooledObjects[index], currentMousePos, transform.rotation);
        isAttached = true;
    }
    
    // Study
    // public void CellSetTag(Cell cell, int tag) {
    //         // remove previous tag register
    //         if (cellTagged.ContainsKey(cell.tag)) {
    //             cellTagged.Remove(cell.tag);
    //         }
    //         // override existing tag
    //         if (cellTagged.ContainsKey(tag)) {
    //             cellTagged.Remove(tag);
    //         }
    //         cellTagged.Add(tag, cell);
    //         cell.tag = tag;
    //     }
    
    
    
    
    
}