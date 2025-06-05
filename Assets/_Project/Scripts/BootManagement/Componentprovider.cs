// ComponentProvider.cs
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using _Project.Scripts.EventBus;

namespace _Project.Scripts.BootManagement
{
    public class ComponentProvider : MonoBehaviour
    {
        [SerializeField] private Image loadingBar;
        [SerializeField] private Canvas loadingCanvas;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private Camera loadingCamera;

        private void Awake()
        {
            if (loadingBar == null || loadingCanvas == null || progressText == null || loadingCamera == null)
            {
                Debug.LogError("One or more UI components are not assigned in ComponentProvider.");
                return;
            }

            var components = new SceneComponentsData
            {
                loadingBar = this.loadingBar,
                loadingCanvas = this.loadingCanvas,
                progressText = this.progressText,
                loadingCamera = this.loadingCamera
            };

            EventBus<SceneComponentsLoadedEvent>.Raise(new SceneComponentsLoadedEvent { components = components });
        }
    }

    [System.Serializable]
    public class SceneComponentsData
    {
        public Image loadingBar;
        public Canvas loadingCanvas;
        public TextMeshProUGUI progressText;
        public Camera loadingCamera;
    }
}