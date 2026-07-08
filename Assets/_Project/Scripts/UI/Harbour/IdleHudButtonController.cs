using _Project.Scripts.SceneManagement;
using _Project.Scripts.UI.IMGUI;
using _Project.Scripts.UI.WorldMap.HUDInteractions;
using Crest;
using UnityEngine;

namespace _Project.Scripts.UI.Harbour
{
    public class IdleHudButtonController : MonoBehaviour
    {
        private OceanRenderer _currentOcean;
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

        // private IEnumerator LoadSceneGroupOneWithOceanTransition()
        // {
        //     // 1. Find current ocean and disable it
        //     _currentOcean = FindObjectOfType<OceanRenderer>();
        //     if (_currentOcean != null)
        //     {
        //         _currentOcean.gameObject.SetActive(false);
        //         Debug.Log("[Ocean] Disabled old ocean for Scene Group 1");
        //     }
        //
        //     // 2. Load your Scene Group 1 (replace with your actual scene name)
        //     // If SceneLoader has a coroutine version, use it here
        //     SceneLoader.Instance.LoadSpecificSceneGroup(1);
        //     
        //     // 3. Wait for new scene to load fully
        //     yield return new WaitForSeconds(0.1f); // Small delay for Crest init
        //     yield return null;
        //     yield return null;
        //
        //     // 4. Enable new ocean in loaded scene
        //     _currentOcean = FindObjectOfType<OceanRenderer>();
        //     if (_currentOcean != null)
        //     {
        //         _currentOcean.gameObject.SetActive(true);
        //         Debug.Log("[Ocean] Activated ocean in Scene Group 1");
        //     }
        //     else
        //     {
        //         Debug.LogWarning("[Ocean] No ocean found in new scene!");
        //     }
        // }
    }
}
