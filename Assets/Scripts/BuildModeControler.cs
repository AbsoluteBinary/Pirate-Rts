using UnityEngine;

public class BuildModeControler : MonoBehaviour
{
    [SerializeField] private GameObject buildButton;
    [SerializeField] private GameObject buildMenuInventory;
    //[SerializeField] private GameObject buildMenuIO;
    
    public void ToggleBuildMenu()
    {
        buildButton.SetActive(!buildButton.activeSelf);
        buildMenuInventory.SetActive(!buildMenuInventory.activeSelf);
    }
}
