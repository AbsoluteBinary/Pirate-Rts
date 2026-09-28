using System.Collections;
using _Project.Scripts.Fleet;
using _Project.Scripts.PlayerShip_Movement.Combat;
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
        public void AttachFlagship(GameObject prefab, string shipName, Transform pawn)
        {
            if (prefab == null || pawn == null) return;

            Vector3 position = pawn.position;
            Quaternion rotation = pawn.rotation;

            if (ship != null && ship.transform != pawn)
            {
                position = ship.transform.position;
                rotation = ship.transform.rotation;

                var oldHelm = ship.GetComponent<PlayerCombatMovementController>();
                if (oldHelm != null)
                    oldHelm.enabled = false;

                ship.SetActive(false);
            }

            pawn.SetPositionAndRotation(position, rotation);

            var visual = Instantiate(prefab, pawn);
            visual.name = string.IsNullOrEmpty(shipName) ? prefab.name : shipName;
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;

            ship = pawn.gameObject;
            
            if (PendingBattle.HasMapPosition)
            {
                pawn.position = PendingBattle.MapPosition;
                Debug.Log($"<color=lime>[Battle] Returned to {pawn.position}</color>");
                PendingBattle.Clear();
            }
            Debug.Log($"<color=lime>[Fleet] Controller ship is now '{ship.name}'</color>");
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

                var oldHelm = ship.GetComponent<PlayerCombatMovementController>();
                if (oldHelm != null)
                    oldHelm.enabled = false;

                ship.SetActive(false);
            }

            var spawned = Instantiate(prefab, position, rotation);
            spawned.name = string.IsNullOrEmpty(shipName) ? prefab.name : shipName;
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(spawned, gameObject.scene);
            ship = spawned;

            Debug.Log($"<color=lime>[Fleet] Controller ship is now '{ship.name}'</color>");
        }
        
        public void OrderMove(Vector3 worldPosition)
        {
            SailTo(worldPosition);
        }

        public PlayerCombatMovementController Helm =>
            ship != null ? ship.GetComponent<PlayerCombatMovementController>() : null;

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
            SailTo(location);
        }

        public void StartTween()
        {
            if (frButton == null) return;

            targetLocation = frButton.transform.position;
            if (targetLocation == Vector3.zero)
                targetLocation = transform.position;

            SailTo(targetLocation);
        }

        private void SailTo(Vector3 location)
        {
            if (ship == null) return;

            FreeRoamTween?.Kill();
            ship.transform.DOKill();

            var helm = ship.GetComponent<PlayerCombatMovementController>();
            if (helm == null)
            {
                Debug.LogWarning($"[Move] '{ship.name}' has no PlayerCombatMovementController.");
                return;
            }

            helm.MoveToLocation(location);
        }

        private IEnumerator PopUpCountdown()
        {
            for (var e = timeRemaining; e > 0; e--)
            {
                yield return new WaitForSeconds(1f);
                
                if (e == 1 && frButton != null)
                {
                    frButton.SetActive(false);
                }
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