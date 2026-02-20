using System.Collections;
using _Project.Scripts.SceneManagement;
using Crest;
using UI.IMGUI;
using UnityEngine;

namespace UI.WorldMap.ObjectInteractions
{
    public class Type0AButtonController : MonoBehaviour
    {
        private OceanRenderer _currentOcean;
        [SerializeField] private int targetSceneGroupIndex = 3;
        
        private void Awake()
        {
            WorldSpaceInteractionsEventBus.Type0AObjButtonClicked += OnType0AButtonClicked;
            Debug.Log("Type0AButtonController: Awake called. Registering event.");
        }

        private void OnDestroy()
        {
            WorldSpaceInteractionsEventBus.Type0AObjButtonClicked -= OnType0AButtonClicked;
        }

        public void OnType0AButtonClicked()
        {
            if (IMGUILoadingOverlay.Instance != null)
            {
                IMGUILoadingOverlay.Instance.TriggerLoadingScreen();
            }
            
            Debug.Log(": Enter Type0AObj clicked - Loading Scene Group 3");
            
            _ = SceneLoader.Instance.BeginSceneTransition(targetSceneGroupIndex);
        }

        // private IEnumerator LoadSceneGroup3WithOceanTransition()
        // {
        //     // 1. Find current ocean and disable it
        //     _currentOcean = FindFirstObjectByType<OceanRenderer>();
        //     if (_currentOcean != null)
        //     {
        //         _currentOcean.gameObject.SetActive(false);
        //         Debug.Log("[Ocean] Disabled old ocean for Scene Group 3");
        //     }
        //
        //     // 2. Load your Scene Group 3 (replace with your actual scene name)
        //     // If SceneLoader has a coroutine version, use it here
        //     SceneLoader.Instance.LoadSpecificSceneGroup(3);
        //     
        //     // 3. Wait for new scene to load fully
        //     yield return new WaitForSeconds(0.1f); // Small delay for Crest init
        //     yield return null;
        //     yield return null;
        //
        //     // 4. Enable new ocean in loaded scene
        //     _currentOcean = FindFirstObjectByType<OceanRenderer>();
        //     if (_currentOcean != null)
        //     {
        //         _currentOcean.gameObject.SetActive(true);
        //         Debug.Log("[Ocean] Activated ocean in Scene Group 3");
        //     }
        //     else
        //     {
        //         Debug.LogWarning("[Ocean] No ocean found in new scene!");
        //     }
        // }
    }
}
