using Packages.ModularStrategyTopDownCameraController.Input;
using Packages.ModularStrategyTopDownCameraController.Scripts;
using UnityEngine;

namespace _Project.Scripts.Cam_System
{
    public class OrthographicZoomComponent : MonoBehaviour
    {
        [Header("Zoom Settings")]
        public float speed = 10f;
        public bool isInverted = false;

        [Header("Orthographic Limits")]
        [Tooltip("Smallest orthographic size (most zoomed in)")]
        public float minSize = 10f;

        [Tooltip("Largest orthographic size (most zoomed out)")]
        public float maxSize = 40f;

        private float currentZoom;
        private StrategyCameraController controller;

        private Camera camera
        {
            get { return controller.camera; }
        }

        private CameraInputs xinputs
        {
            get { return controller.xinputs; }
        }

        void Start()
        {
            if (controller == null)
                controller = GetComponent<StrategyCameraController>();

            currentZoom = camera.orthographic ? camera.orthographicSize : camera.fieldOfView;
        }

        private void Update()
        {
            Zoom(xinputs.zoom);
        }

        private void Zoom(float dir)
        {
            if (camera.orthographic)
            {
                float size = camera.orthographicSize - dir * speed * Time.deltaTime * (isInverted ? -1f : 1f);

                // Clamp with our own limits instead of the controller's
                size = Mathf.Clamp(size, minSize, maxSize);

                camera.orthographicSize = size;
            }
            else
            {
                float fov = camera.fieldOfView + dir * speed * Time.deltaTime * (isInverted ? -1f : 1f);
                fov = controller.ZoomFovConstaints(fov); // keep original for perspective
                camera.fieldOfView = fov;
            }
        }
    }
}