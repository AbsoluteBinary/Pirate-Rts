using _Project.Scripts.SceneManagement;
using _Project.Scripts.UI.IMGUI;
using UnityEngine;

namespace _Project.Scripts.UI.WorldMap.HUDInteractions
{
    public class HUDButtonController : MonoBehaviour
    {
        //private OceanRenderer _currentOcean;
        [SerializeField] private int targetSceneGroupIndex = 2; // e.g., Harbour = 2

        [SerializeField] private int battleSceneGroupIndex = 3;
        
        
        
        private void Awake()
        {
            HUDMenuButtonsEventBus.HUDEnterBaseClicked += OnMarkerButtonClicked;
            //Debug.Log("HUDController: Awake called. Registering event.");
            HUDMenuButtonsEventBus.HUDEnterBattleClicked += OnEnterBattleClicked;
        }

        private void OnDestroy()
        {
            HUDMenuButtonsEventBus.HUDEnterBaseClicked -= OnMarkerButtonClicked;
            HUDMenuButtonsEventBus.HUDEnterBattleClicked -= OnEnterBattleClicked;
        }
        
        private void OnEnterBattleClicked()
        {
            if (IMGUILoadingOverlay.Instance != null)
                IMGUILoadingOverlay.Instance.TriggerLoadingScreen();

            _ = SceneLoader.Instance.BeginSceneTransition(battleSceneGroupIndex);
        }

        public void OnMarkerButtonClicked()
        {
            if (IMGUILoadingOverlay.Instance != null)
            {
                IMGUILoadingOverlay.Instance.TriggerLoadingScreen();
            }
            
            //Debug.Log("HUDController: Entering Harbour...");

            // SMOOTH PROFESSIONAL TRANSITION
            _ = SceneLoader.Instance.BeginSceneTransition(targetSceneGroupIndex);
        }
        
    }
}
