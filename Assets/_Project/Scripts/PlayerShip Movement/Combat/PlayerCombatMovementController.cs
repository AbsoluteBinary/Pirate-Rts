using DG.Tweening;
using UnityEngine;

namespace _Project.Scripts.PlayerShip_Movement.Combat
{
    public class PlayerCombatMovementController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform shipVisual;

        [Header("Movement Settings")]
        [SerializeField] private float maxSpeed = 18f;
        [SerializeField] private float acceleration = 12f;
        [SerializeField] private float deceleration = 18f;
        [SerializeField] private float inertiaFactor = 0.92f;      // ← New: Momentum feel
        [SerializeField] private float turnSpeed = 110f;
        [SerializeField] private float heightOffset = 1.5f;
        [SerializeField] private float stopDistance = 4f;

        [Header("Arrival Settings")]
        [SerializeField] private float arrivalThreshold = 1.2f;
        [SerializeField] private float finalRotationDuration = 0.6f;

        [Header("Realism")]
        [SerializeField] private float rollAmount = 7f;
        [SerializeField] private float bobAmount = 0.4f;
        [SerializeField] private float bobSpeed = 1.4f;

        private Vector3 targetPosition;
        private bool hasTarget = false;
        private float currentSpeed = 0f;

        private Tween bobTween;
        private Tween finalRotationTween;

        private void Awake()
        {
            if (shipVisual == null) shipVisual = transform;
            StartGentleBob();
        }

        private void LateUpdate()
        {
            if (shipVisual == null) return;
            Vector3 euler = shipVisual.eulerAngles;
            shipVisual.rotation = Quaternion.Euler(0f, euler.y, euler.z);
        }

        private void Update()
        {
            if (!hasTarget) return;
            MoveAndSteer();
        }

        public void MoveToLocation(Vector3 worldPosition)
        {
            targetPosition = new Vector3(worldPosition.x, worldPosition.y + heightOffset, worldPosition.z);
            hasTarget = true;
            currentSpeed = Mathf.Max(currentSpeed, 5f);
        }

        private void MoveAndSteer()
        {
            Vector3 toTarget = targetPosition - shipVisual.position;
            float distance = toTarget.magnitude;

            if (distance <= arrivalThreshold)
            {
                ArriveAtDestination();
                return;
            }

            // Aim
            Vector3 desiredDirection = toTarget.normalized;
            desiredDirection.y = 0f;

            if (desiredDirection.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(desiredDirection, Vector3.up);
                shipVisual.rotation = Quaternion.RotateTowards(
                    shipVisual.rotation, targetRot, turnSpeed * Time.deltaTime);
            }

            // Speed Control with Inertia
            bool isClose = distance < stopDistance;
            float targetSpeed = isClose 
                ? Mathf.Lerp(1f, maxSpeed * 0.35f, distance / stopDistance) 
                : maxSpeed;

            // Apply acceleration / deceleration
            float smoothSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, 
                (isClose ? deceleration : acceleration) * Time.deltaTime);

            // Add inertia (feels much more boat-like)
            currentSpeed = Mathf.Lerp(currentSpeed, smoothSpeed, 1f - inertiaFactor);

            // Move forward
            shipVisual.position += shipVisual.forward * currentSpeed * Time.deltaTime;

            ApplyTurningRoll(desiredDirection);
        }

        private void ApplyTurningRoll(Vector3 desiredDirection)
        {
            float angleDiff = Vector3.SignedAngle(shipVisual.forward, desiredDirection, Vector3.up);
            float targetRoll = Mathf.Clamp(angleDiff * -0.18f, -rollAmount, rollAmount);

            Vector3 currentEuler = shipVisual.localEulerAngles;
            currentEuler.z = Mathf.LerpAngle(currentEuler.z, targetRoll, Time.deltaTime * 8f);
            shipVisual.localEulerAngles = currentEuler;
        }

        private void ArriveAtDestination()
        {
            hasTarget = false;

            // Final gentle slowdown + leveling
            finalRotationTween?.Kill();
            finalRotationTween = shipVisual.DOLocalRotate(
                new Vector3(0, shipVisual.localEulerAngles.y, 0), 
                finalRotationDuration)
                .SetEase(Ease.OutSine);

            Debug.Log("[Combat] Ship arrived with momentum.");
        }

        private void StartGentleBob()
        {
            bobTween = shipVisual.DOLocalMoveY(shipVisual.localPosition.y + bobAmount, bobSpeed)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        private void OnDestroy()
        {
            bobTween?.Kill();
            finalRotationTween?.Kill();
        }
    }
}