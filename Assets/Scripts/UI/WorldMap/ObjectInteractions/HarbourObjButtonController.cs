using System.Collections;
using _Project.Scripts.SceneManagement;
using Crest;
using UI.WorldMap.HUDInteractions;
using UnityEngine;

namespace UI.WorldMap.ObjectInteractions
{
    public class HarbourObjButtonController : MonoBehaviour
    {
        private OceanRenderer _currentOcean;

        private void Awake()
        {
            WorldSpaceInteractionsEventBus.HarbourButtonClicked += OnHarbourButtonClicked;
            Debug.Log("HUDController: Awake called. Registering event.");
        }

        private void OnDestroy()
        {
            WorldSpaceInteractionsEventBus.HarbourButtonClicked -= OnHarbourButtonClicked;
        }

        public void OnHarbourButtonClicked()
        {
            Debug.Log(": Enter Harbour clicked - Loading Scene Group 2");
            StartCoroutine(LoadSceneGroup2WithOceanTransition());
        }

        private IEnumerator LoadSceneGroup2WithOceanTransition()
        {
            // 1. Find current ocean and disable it
            _currentOcean = FindObjectOfType<OceanRenderer>();
            if (_currentOcean != null)
            {
                _currentOcean.gameObject.SetActive(false);
                Debug.Log("[Ocean] Disabled old ocean for Scene Group 2");
            }

            // 2. Load your Scene Group 2 (replace with your actual scene name)
            // If SceneLoader has a coroutine version, use it here
            SceneLoader.Instance.LoadSpecificSceneGroup(2);
            
            // 3. Wait for new scene to load fully
            yield return new WaitForSeconds(0.1f); // Small delay for Crest init
            yield return null;
            yield return null;

            // 4. Enable new ocean in loaded scene
            _currentOcean = FindObjectOfType<OceanRenderer>();
            if (_currentOcean != null)
            {
                _currentOcean.gameObject.SetActive(true);
                Debug.Log("[Ocean] Activated ocean in Scene Group 2");
            }
            else
            {
                Debug.LogWarning("[Ocean] No ocean found in new scene!");
            }
        }
    }
}
