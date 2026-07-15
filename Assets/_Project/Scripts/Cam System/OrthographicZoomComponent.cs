using Packages.ModularStrategyTopDownCameraController.Input;
using Packages.ModularStrategyTopDownCameraController.Scripts;
using UnityEngine;

namespace _Project.Scripts.Cam_System
{
    public class OrthographicZoomComponent : MonoBehaviour
    {
        public float speed = 10;
        public bool isInverted = false;
        private float currentZoom;
        private StrategyCameraController controller;
        private new Camera camera
        {
            get
            {
                return controller.camera;
            }
        }
        private CameraInputs xinputs
        {
            get { return controller.xinputs; }
        }

        void Start()
        {
            if (controller == null)
            {
                controller = GetComponent<StrategyCameraController>();
            }
            currentZoom = controller.camera.fieldOfView;
        }

        private void Update()
        {
            var zoomVal = xinputs.zoom;
            Zoom(zoomVal);
        }

        private void Zoom(float dir)
        {
            if (camera.orthographic)
            {
                // Orthographic camera (what you're using)
                float size = camera.orthographicSize - dir * speed * Time.deltaTime * (isInverted ? -1 : 1);
                size = controller.ZoomFovConstaints(size);   // still works because it just clamps
                camera.orthographicSize = size;
            }
            else
            {
                // Perspective camera (original behaviour)
                float fov = camera.fieldOfView + dir * speed * Time.deltaTime * (isInverted ? -1 : 1);
                fov = controller.ZoomFovConstaints(fov);
                camera.fieldOfView = fov;
            }
        }
    }
}
