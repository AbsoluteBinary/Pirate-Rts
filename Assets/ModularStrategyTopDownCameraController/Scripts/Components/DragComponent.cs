
using UnityEngine;


namespace StrategyCamera
{
    [RequireComponent(typeof(StrategyCameraController))]
    public class DragComponent : MonoBehaviour
    {
        private bool hasRefPos = false;
        private StrategyCameraController controller;
        private CameraInputs xinputs
        {
            get { return controller.xinputs; }
        }
        private Transform CameraRig
        {
            get
            {
                return controller.transform;
            }
        }
        private Vector3 clickStartPos;
        void Start()
        {
            this.controller = GetComponent<StrategyCameraController>();
        }

        void Update()
        {
            if(xinputs.hasTouchScreen && xinputs.touchCount > 1) {
                return;
            }

            if (xinputs.isPointerDownThisFrame)
            {
                var hit = CameraToolsUtilities.GetPointerWorldPosition(xinputs.pointerPosition, controller.camera);
                hasRefPos = hit.Item1;
                clickStartPos = xinputs.pointerPosition;
                return;
            }

            if (xinputs.isPointerHolding && hasRefPos)
            {
                var pointerWorldInfo = CameraToolsUtilities.GetPointerWorldPosition(xinputs.pointerPosition, controller.camera);
                var previousFrameInfo = CameraToolsUtilities.GetPointerWorldPosition(clickStartPos, controller.camera);

                if (!pointerWorldInfo.Item1 || !previousFrameInfo.Item1)
                    return;

                var delta = pointerWorldInfo.Item2 - previousFrameInfo.Item2;
                delta.y = 0;
                //CameraRig.position -= delta;
                var targetPos = CameraRig.position - delta;
                controller.transform.position = controller.MoveWithConstraints(targetPos);
                clickStartPos = xinputs.pointerPosition;
            }
            else
            {
                hasRefPos = false;
            }
        }


    }
}
