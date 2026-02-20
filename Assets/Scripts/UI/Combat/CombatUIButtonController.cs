using System.Collections;
using _Project.Scripts.SceneManagement;
using Crest;
using UI.IMGUI;
using UI.WorldMap.HUDInteractions;
using UnityEngine;

namespace UI.Combat
{
    public class CombatUIButtonController : MonoBehaviour
    {
        [SerializeField] private int targetSceneGroupIndex = 1;

        private void Awake()
        {
            HUDMenuButtonsEventBus.HUDEnterWorldClicked += OnLaunchWorldButtonClicked;
            //Debug.Log("HUDController: Awake called. Registering event.");
        }

        private void OnDestroy()
        {
            HUDMenuButtonsEventBus.HUDEnterWorldClicked -= OnLaunchWorldButtonClicked;
        }

        public void OnLaunchWorldButtonClicked()
        {
            if (IMGUILoadingOverlay.Instance != null)
            {
                IMGUILoadingOverlay.Instance.TriggerLoadingScreen();
            } 
            
            // Debug.Log("HUDController: Enter Harbour clicked - Loading Scene Group 2");
            //StartCoroutine(LoadSceneGroupOneWithOceanTransition());
            
            _ = SceneLoader.Instance.BeginSceneTransition(targetSceneGroupIndex);
        }
    }
}
