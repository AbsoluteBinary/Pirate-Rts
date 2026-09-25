using System.Collections;
using _Project.Scripts.UI.WorldMap;
using DG.Tweening;
using UnityEngine;

namespace _Project.Scripts.PlayerShip_Movement
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
        
        [Header("Movement Height")]
        private static float travelHeightOffset = 1f; // ← ADJUST THIS: Height ship sails above water

        public ClickBehavior ClickBehavior;
        private Coroutine coroutine;
        public int timeRemaining;

        private enum DOTweenType
        {
            MovementOneWay,
            MovementTwoWay
        }

        private void Awake()
        {
            //Debug.Log($"PlayerShipController: Awake called on {gameObject.name}. IsActive: {gameObject.activeInHierarchy}");
            
            WorldSpaceInteractionsEventBus.MarkerButtonClicked += OnMarkerButtonClicked;

            if (ship == null)
                Debug.LogWarning("PlayerShipController: Ship was empty. WorldFleetArrival will fill it.");

            if (!gameObject.activeSelf)
            {
                Debug.LogWarning($"<color=yellow>PlayerShipController: GameObject is not active. Enabling it.");
                gameObject.SetActive(true);
            }

            if (frButton == null)
            {
                Debug.LogError($"<color=red>PlayerShipController: frButton is not assigned!");
                return;
            }
            if (ship == null)
            {
                Debug.LogError($"<color=red>PlayerShipController: Ship is not assigned!");
                return;
            }

            WorldSpaceInteractionsEventBus.MarkerButtonClicked += OnMarkerButtonClicked;
        }
        
        public void ReplaceVisual(GameObject prefab, string shipName)
        {
            if (prefab == null)
            {
                Debug.LogWarning("[Fleet] No combat prefab to place on the map.");
                return;
            }

            Vector3 position = transform.position;
            Quaternion rotation = transform.rotation;

            if (ship != null)
            {
                position = ship.transform.position;
                rotation = ship.transform.rotation;
                ship.SetActive(false);
            }

            var spawned = Instantiate(prefab, position, rotation);
            spawned.name = string.IsNullOrEmpty(shipName) ? prefab.name : shipName;
            ship = spawned;

            Debug.Log($"<color=lime>[Fleet] Map model '{spawned.name}' at {position}</color>");
        }

        private void Start()
        {
            //Debug.Log($"PlayerShipController: Start called on {gameObject.name}.");
        }

        private void OnDestroy()
        {
            WorldSpaceInteractionsEventBus.MarkerButtonClicked -= OnMarkerButtonClicked;
        }

        public void SetFrButton(GameObject newFrButton)
        {
            frButton = newFrButton;
            if (frButton == null)
            {
                Debug.LogError($"<color=yellow>PlayerShipController: SetFrButton received null!");
                return;
            }
            //Debug.Log($"PlayerShipController: frButton updated to {frButton.name}");
        }

        private void OnMarkerButtonClicked()
        {
            //Debug.Log("PlayerShipController: World Space Button Clicked via EventBus!");
            StartTween();
        }

        public void MoveToLocation(Vector3 location)
        {
            if (ship == null) return;

            FreeRoamTween?.Kill();

            // Apply height offset so ship sails above water
            Vector3 adjustedTarget = new Vector3(location.x, location.y + travelHeightOffset, location.z);

            FreeRoamTween = ship.transform.DOLookAt(adjustedTarget, moveDuration * 0.4f);
            FreeRoamTween = ship.transform.DOMove(adjustedTarget, moveDuration)
                .SetEase(Ease.InOutSine)
                .OnComplete(() => Debug.Log($"[Combat] Ship arrived at {adjustedTarget}"));
        }

        public void StartTween()
        {
            if (frButton == null) return;

            targetLocation = frButton.transform.position;
            //Debug.Log($"PlayerShipController: StartTween called. Target location: {targetLocation}");

            if (doTweenType == DOTweenType.MovementOneWay)
            {
                if (coroutine != null) StopCoroutine(coroutine);
                coroutine = StartCoroutine(PopUpCountdown());

                if (targetLocation == Vector3.zero)
                {
                    targetLocation = transform.position;
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
                //Debug.Log($"PlayerShipController: Countdown: {e} seconds remaining.");
                if (e == 1)
                {
                    frButton.transform.position = new Vector3(0, -10, 0);
                    frButton.SetActive(false);
                    //Debug.Log("PlayerShipController: frButton moved off-screen and hidden.");
                }
            }
        }
    }

    public enum ClickBehavior
    {
        None
    }
}