using System.Collections;
using _Project.Scripts.SceneManagement;
using Crest;
using Managers.World_Map.HUDInteractions;
using UnityEngine;

namespace UI
{
    public class HUDButtonController : MonoBehaviour
    {
        private OceanRenderer _currentOcean;

        private void Awake()
        {
            HUDMenuButtonsEventBus.HUDEnterBaseClicked += OnMarkerButtonClicked;
            Debug.Log("HUDController: Awake called. Registering event.");
        }

        private void OnDestroy()
        {
            HUDMenuButtonsEventBus.HUDEnterBaseClicked -= OnMarkerButtonClicked;
        }

        public void OnMarkerButtonClicked()
        {
            Debug.Log("HUDController: Enter Harbour clicked - Loading Scene Group 2");
            StartCoroutine(LoadSceneGroup2WithOceanTransition());
        }

        private IEnumerator LoadSceneGroup2WithOceanTransition()
        {
            // 1. Disable old ocean
            _currentOcean = FindObjectOfType<OceanRenderer>();
            if (_currentOcean != null)
            {
                _currentOcean.gameObject.SetActive(false);
                Debug.Log("[Ocean] Disabled old ocean");
            }

            // 2. Load new scene
            SceneLoader.Instance.LoadSpecificSceneGroup(2);

            // 3. Wait longer for scene + Crest to fully load
            yield return new WaitForSeconds(0.5f);  // Increased delay
            yield return null;
            yield return null;
            yield return null;  // Extra frames for Crest init

            // 4. Enable new ocean
            _currentOcean = FindObjectOfType<OceanRenderer>();
            if (_currentOcean != null)
            {
                _currentOcean.gameObject.SetActive(true);
                Debug.Log("[Ocean] Activated new ocean in Harbour");
            }
            else
            {
                Debug.LogError("[Ocean] NO OCEAN FOUND IN HARBOUR SCENE!");
            }
        }
    }
}
