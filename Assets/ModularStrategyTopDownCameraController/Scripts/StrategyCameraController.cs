using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace StrategyCamera
{
#if ENABLE_INPUT_SYSTEM
    [RequireComponent(typeof(UnityEngine.InputSystem.PlayerInput))]
#endif
    [DefaultExecutionOrder(-50)]
    public class StrategyCameraController : MonoBehaviour
    {
        public float minXAngle = 30f, maxXAngle = 75f;
        public float zoomInLimit = 20, zoomOutLimit = 75;
        public Vector3 lowerLeftLimit, upperRightLimit;
        public CameraInputs xinputs = null;
        public Transform cameraTransform
        {
            get;private set;
        }

        [HideInInspector]public new Camera camera;

        private void Awake()
        {
            if (cameraTransform == null)
            {
                cameraTransform = transform.GetChild(0);
                camera = cameraTransform.GetComponent<Camera>();
            }

            this.xinputs = new(gameObject);
        }

        private void OnEnable()
        {
            xinputs.EnableInputs();
        }

        private void OnDisable()
        {
            xinputs.DisableInputs();
        }

        void Update()
        {
            xinputs.UpdateTransientInputs();
        }

        public Vector3 MoveWithConstraints(Vector3 targetPos)
        {
            targetPos.x = Mathf.Clamp(targetPos.x, lowerLeftLimit.x, upperRightLimit.x);
            targetPos.y = Mathf.Clamp(targetPos.y, lowerLeftLimit.y, upperRightLimit.y);
            targetPos.z = Mathf.Clamp(targetPos.z, lowerLeftLimit.z, upperRightLimit.z);

            return targetPos;
        }

        public float ZoomFovConstaints(float fov)
        {
            return Mathf.Clamp(fov, zoomInLimit, zoomOutLimit);
        }
        public float ClampXCameraAnglesDegree(float current)
        {
            if (current > 180)
            {
                current -= 360;
            }

            current = Mathf.Clamp(current, minXAngle, maxXAngle);
            return current;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Vector3 center = (lowerLeftLimit + upperRightLimit) / 2;
            Vector3 size = upperRightLimit - lowerLeftLimit;
            Gizmos.color = Color.green;

            Gizmos.DrawWireCube(center, size);
        }
#endif
    }
}