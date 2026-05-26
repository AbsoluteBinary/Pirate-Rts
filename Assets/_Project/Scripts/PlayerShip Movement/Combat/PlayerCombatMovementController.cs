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
        [SerializeField] private float turnSpeed = 85f;
        [SerializeField] private float heightOffset = 1.5f;        // Slightly increased
        [SerializeField] private float stopDistance = 3f;

        [Header("Realism")]
        [SerializeField] private float rollAmount = 7f;
        [SerializeField] private float bobAmount = 0.4f;
        [SerializeField] private float bobSpeed = 1.4f;

        private Vector3 targetPosition;
        private bool hasTarget = false;
        private float currentSpeed = 0f;

        private Tween bobTween;

        private void Awake()
        {
            if (shipVisual == null) shipVisual = transform;
            StartGentleBob();
        }

        private void LateUpdate()
        {
            if (shipVisual == null) return;

            // === CRITICAL FIX: Keep ship level at all times ===
            Vector3 euler = shipVisual.eulerAngles;
            shipVisual.rotation = Quaternion.Euler(0f, euler.y, euler.z); // Lock pitch (X) to 0
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
            currentSpeed = Mathf.Max(currentSpeed, 4f);
        }

        private void MoveAndSteer()
        {
            Vector3 toTarget = targetPosition - shipVisual.position;
            float distance = toTarget.magnitude;

            if (distance <= 0.6f)
            {
                ArriveAtDestination();
                return;
            }

            // === IMPROVED: Only rotate on Y axis (no nose dive) ===
            Vector3 desiredDirection = toTarget.normalized;
            desiredDirection.y = 0f;                    // Remove vertical component

            if (desiredDirection.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(desiredDirection, Vector3.up);
                
                shipVisual.rotation = Quaternion.RotateTowards(
                    shipVisual.rotation, targetRot, turnSpeed * Time.deltaTime);
            }

            // Speed control
            bool isClose = distance < stopDistance;
            float targetSpeed = isClose ? 
                Mathf.Lerp(2f, maxSpeed * 0.6f, distance / stopDistance) : maxSpeed;

            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, 
                (isClose ? deceleration : acceleration) * Time.deltaTime);

            // Move forward
            shipVisual.position += shipVisual.forward * currentSpeed * Time.deltaTime;

            // Dynamic roll
            ApplyTurningRoll(desiredDirection);
        }

        private void ApplyTurningRoll(Vector3 desiredDirection)
        {
            float angleDiff = Vector3.SignedAngle(shipVisual.forward, desiredDirection, Vector3.up);
            float targetRoll = Mathf.Clamp(angleDiff * -0.18f, -rollAmount, rollAmount);

            Vector3 currentEuler = shipVisual.localEulerAngles;
            currentEuler.z = Mathf.LerpAngle(currentEuler.z, targetRoll, Time.deltaTime * 6f);
            shipVisual.localEulerAngles = currentEuler;
        }

        private void ArriveAtDestination()
        {
            hasTarget = false;
            currentSpeed = 0f;

            // Smoothly level the ship
            shipVisual.DOLocalRotate(new Vector3(0, shipVisual.localEulerAngles.y, 0), 0.8f)
                .SetEase(Ease.OutSine);

            Debug.Log("[Combat] Ship arrived");
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
        }
    }
}