
using UnityEngine;

namespace StrategyCamera
{
    public class PanComponent : MonoBehaviour
    {
        public float sensitivity = 10.0f;
        public bool invertX,invertY;

        private Transform cameraTransform;
        private StrategyCameraController controller;
        private void Start()
        {
            this.controller = GetComponent<StrategyCameraController>();
            if(cameraTransform == null)
            {
                cameraTransform = controller.cameraTransform;
            }
        }

        private void Update()
        {
            if (controller.xinputs.isMiddleMouseButtonDown)
            {
                LookAround(controller.xinputs.mouseDelta);
            }
        }
        void LookAround(Vector2 delta)
        {
            delta = delta.normalized;

            var rotationy = delta.y * sensitivity * Time.deltaTime * (invertY ? -1:1);
            var rotationX = delta.x * sensitivity * Time.deltaTime * (invertX ? -1 : 1);

            var rotY = Quaternion.AngleAxis(rotationy, Vector3.right);
            var rotX = Quaternion.AngleAxis(rotationX, Vector3.up);

            var result = cameraTransform.localRotation;
            result *=  rotY * rotX;
            var eulerResult = result.eulerAngles;
            var x = controller.ClampXCameraAnglesDegree(eulerResult.x);
            cameraTransform.localRotation = Quaternion.Euler(x, eulerResult.y, 0);
        }

    }
}
        