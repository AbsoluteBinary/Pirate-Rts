using UnityEngine;

namespace StrategyCamera
{
    public class TranslateComponent : MonoBehaviour
    {
        public float movementSpeed = 100;
        public float edgeThresholdFactor = 0.05f;

        private Transform cameraTransform;
        private float edgeThreshold = 0.05f;
        private StrategyCameraController controller;
        private bool isPointerActive;
        private CameraInputs xinputs
        {
            get { return controller.xinputs; }
        }
        private void Start()
        {
            this.controller = GetComponent<StrategyCameraController>();
            edgeThreshold = Screen.width * edgeThresholdFactor;
            if (cameraTransform == null)
            {
                cameraTransform = controller.cameraTransform;
            }
        }

        void Update()
        {
            if(xinputs.isPointerDownThisFrame)
            {
                isPointerActive = true;
            }

            if(isPointerActive == false)
            {
                return;
            }

            Move(xinputs.translate);
            Move(GetMouseCornerDirection());
        }

        void Move(Vector2 delta)
        {
            var pos = transform.position;
            var x = cameraTransform.right * delta.x;
            var z = cameraTransform.forward * delta.y;

            pos += x+z;
            pos = new Vector3(pos.x, transform.position.y, pos.z);
            var targetPos = Vector3.Lerp(transform.position, pos,Time.deltaTime * movementSpeed);
            controller.transform.position = controller.MoveWithConstraints(targetPos);
        }

        Vector2 GetMouseCornerDirection()
        {
            Vector2 mousePosition = xinputs.pointerPosition;
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            float xDirection = 0;
            float yDirection = 0;

            if (mousePosition.x <= edgeThreshold)
            {
                xDirection = -1;
            }
            else if (mousePosition.x >= screenWidth - edgeThreshold)
            {
                xDirection = 1;
            }

            if (mousePosition.y <= edgeThreshold)
            {
                yDirection = -1;
            }
            else if (mousePosition.y >= screenHeight - edgeThreshold)
            {
                yDirection = 1;
            }

            return new Vector2(xDirection, yDirection);
        }
    }
}