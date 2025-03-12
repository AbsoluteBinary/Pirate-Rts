
using System.Collections;
using UnityEngine;


namespace StrategyCamera
{
    [RequireComponent(typeof(SelectorComponent))]
    [RequireComponent(typeof(StrategyCameraController))]
    public class FollowComponent : MonoBehaviour
    {
        public float moveSpeed = 1f, rotationTransitionTime = 0.55f;
        public bool shouldLockOnSelected;
        public float lockDefaultAngle = 70;
        private SelectorComponent mSelector;
        private StrategyCameraController mController;
        private Vector3 offsetCenterOfScreen = Vector3.zero;
        private Quaternion defaultRot = Quaternion.identity;
        private ISelectable previousLockObject = null;
        private Coroutine lookRoutine;

        private Transform camTransform
        {
            get
            {
                return mController.cameraTransform;
            }
        }
        private Transform selectedTransform
        {
            get
            {
                return mSelector.selectedInterface?.GetSelfTransform();
            }
        }
        private void Start()
        {
            mSelector = GetComponent<SelectorComponent>();
            mController = GetComponent<StrategyCameraController>();

            var screenCenterWorldPos = CameraToolsUtilities.GetViewportToWorldPosition(new Vector2(0.5f, 0.55f), mController.camera);
            offsetCenterOfScreen = screenCenterWorldPos.Item2;
            defaultRot = camTransform.localRotation;
        }

        private void Update()
        {
            if (mSelector.selectedInterface != null && shouldLockOnSelected)
            {
                LockOn(selectedTransform.position);

                if (previousLockObject != mSelector.selectedInterface)
                {
                    if (lookRoutine != null)
                    {
                        StopCoroutine(lookRoutine);
                        lookRoutine = null;
                    }

                    lookRoutine = StartCoroutine(SmoothLookAtTarget());
                    previousLockObject = mSelector.selectedInterface;
                }
            }
        }

        private IEnumerator SmoothLookAtTarget()
        {
            var timeLapsed = 0f;
            while (timeLapsed <= rotationTransitionTime)
            {
                float t = timeLapsed / rotationTransitionTime;
                t = t * t;
                var targetRot = Quaternion.Slerp(camTransform.localRotation, defaultRot, t);
                camTransform.localRotation = targetRot;
                timeLapsed += Time.deltaTime;

                yield return null;
            }
            lookRoutine = null;
        }
        private void LockOn(Vector3 lockPosition)
        {
            var targetPos = lockPosition + offsetCenterOfScreen;
            targetPos = Vector3.Lerp(mController.transform.position, targetPos, Time.deltaTime * moveSpeed);
            mController.transform.position = mController.MoveWithConstraints(targetPos);
        }
    }
}