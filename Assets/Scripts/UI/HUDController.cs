using _Project.Scripts.SceneManagement;
using Managers.World_Map.HUDInteractions;
using UnityEngine;

namespace UI
{
    public class HUDController : MonoBehaviour
    {
        private void Awake()
        {
            // Register for button click event
            
            HUDMenuButtonsEventBus.HUDEnterBaseClicked += OnMarkerButtonClicked;
            Debug.Log("HUDController: Awake called. Registering event.");
        }

        public void OnMarkerButtonClicked()
        {
            SceneLoader.Instance.LoadSpecificSceneGroup(2);
        }
    }
}
