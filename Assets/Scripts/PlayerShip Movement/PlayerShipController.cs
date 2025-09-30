using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace PlayerShip_Movement
{
    public class PlayerShipController : MonoBehaviour
    {
        private Vector3 targetLocation;
        [SerializeField] private DOTweenType doTweenType = DOTweenType.MovementOneWay;
        private Tween FreeRoamTween;
        [SerializeField] private GameObject ship;
        [Range(1.0f, 400.0f)] [SerializeField] private float moveDuration = 1.0f;
        [SerializeField] private GameObject frButton; // Marker Prefab for target position
        private Vector3 btnPos;
        private Vector3 btnResetPos;
        
        public ClickBehavior ClickBehavior; // Unused, retained for compatibility
        private Coroutine coroutine;
        public int timeRemaining;

        private enum DOTweenType
        {
            MovementOneWay,
            MovementTwoWay
        }

        private void Awake()
        {
            Debug.Log($"PlayerShipController: Awake called on {gameObject.name}. IsActive: {gameObject.activeInHierarchy}");

            // Validate GameObject state
            if (!gameObject.activeSelf)
            {
                Debug.LogWarning("PlayerShipController: GameObject is not active. Enabling it.");
                gameObject.SetActive(true);
            }

            // Validate serialized fields
            if (frButton == null)
            {
                Debug.LogError("PlayerShipController: frButton is not assigned!");
                return;
            }
            if (ship == null)
            {
                Debug.LogError("PlayerShipController: Ship is not assigned!");
                return;
            }
            Debug.Log($"PlayerShipController: frButton ({frButton.name}) and ship ({ship.name}) are assigned. frButton InstanceID: {frButton.GetInstanceID()}");

            // Register for button click event
            EventBus.MarkerButtonClicked += OnMarkerButtonClicked;
            
        }

        private void Start()
        {
            Debug.Log($"PlayerShipController: Start called on {gameObject.name}.");
            if (frButton != null)
            {
                Debug.Log($"PlayerShipController: frButton in Start: {frButton.name}, InstanceID: {frButton.GetInstanceID()}, Active: {frButton.activeInHierarchy}");
            }
        }

        private void OnDestroy()
        {
            EventBus.MarkerButtonClicked -= OnMarkerButtonClicked; // Prevent memory leaks
        }

        public void SetFrButton(GameObject newFrButton)
        {
            frButton = newFrButton;
            if (frButton == null)
            {
                Debug.LogError("PlayerShipController: SetFrButton received null!");
                return;
            }
            Debug.Log($"PlayerShipController: frButton updated to {frButton.name}, InstanceID: {frButton.GetInstanceID()}, Active: {frButton.activeInHierarchy}");
        }

        private void OnMarkerButtonClicked()
        {
            Debug.Log("PlayerShipController: World Space Button Clicked via EventBus!");
            StartTween();
        }

        public void StartTween()
        {
            targetLocation = frButton.transform.position;
            Debug.Log($"PlayerShipController: StartTween called. Target location: {targetLocation}");

            if (doTweenType == DOTweenType.MovementOneWay)
            {
                Debug.Log("PlayerShipController: Start Moving");
                if (coroutine != null) StopCoroutine(coroutine);
                coroutine = StartCoroutine(PopUpCountdown());

                if (targetLocation == Vector3.zero)
                {
                    targetLocation = transform.position;
                    Debug.Log("PlayerShipController: Target location was zero, set to transform.position.");
                }

                FreeRoamTween = ship.transform.DOLookAt(targetLocation, moveDuration / 2);
                FreeRoamTween = ship.transform.DOMove(targetLocation, moveDuration);
            }
        }

        private IEnumerator PopUpCountdown()
        {
            for (var e = timeRemaining; e > 0; e--)
            {
                yield return new WaitForSeconds(1f);
                Debug.Log($"PlayerShipController: Countdown: {e} seconds remaining.");
                if (e == 1)
                {
                    frButton.transform.position = new Vector3(0, -10, 0);
                    frButton.SetActive(false);
                    Debug.Log("PlayerShipController: frButton moved off-screen and hidden.");
                }
            }

            yield return null;
        }
    }

    public enum ClickBehavior
    {
        None,
        // Add other behaviors if needed
    }
}