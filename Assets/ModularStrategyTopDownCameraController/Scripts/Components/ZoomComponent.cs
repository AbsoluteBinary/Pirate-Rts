
using UnityEngine;

namespace StrategyCamera
{
    public class ZoomComponent : MonoBehaviour
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
            var fov = camera.fieldOfView + dir * speed * Time.deltaTime * (isInverted ? -1 : 1);
            fov = controller.ZoomFovConstaints(fov);
            camera.fieldOfView = fov;
        }
    }
}