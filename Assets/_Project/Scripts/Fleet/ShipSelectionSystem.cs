using UnityEngine;
using UnityEngine.InputSystem;
using _Project.Scripts.PlayerShip_Movement.Combat;

namespace _Project.Scripts.Fleet
{
    public class ShipSelectionSystem : MonoBehaviour
    {
        [Header("Selection Settings")]
        [SerializeField] private LayerMask shipLayer;

        private Camera mainCamera;
        private float lastMoveTime;

        private void Awake()
        {
            mainCamera = Camera.main;
        }

        private void Update()
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (IsPointerOverUI()) return;
                TrySelectShip();
            }

            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                if (IsPointerOverUI()) return;
                TryMoveFleet();
            }

            if (Keyboard.current.ctrlKey.isPressed && Keyboard.current.aKey.wasPressedThisFrame)
            {
                FleetManager.Instance?.SelectAll();
            }
        }

        private void TrySelectShip()
        {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit, 300f, shipLayer))
            {
                if (hit.transform.TryGetComponent<PlayerCombatMovementController>(out var ship))
                {
                    FleetManager.Instance?.SelectSingle(ship);
                    Debug.Log($"[Selection] Selected ship: {ship.name}");
                    return;
                }
            }

            FleetManager.Instance?.SelectAll();
        }

        private void TryMoveFleet()
        {
            if (Time.time - lastMoveTime < 0.1f) return; // Prevent double commands

            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit, 300f))
            {
                lastMoveTime = Time.time;
                Vector3 targetPos = hit.point;
                FleetManager.Instance?.MoveFleetToLocation(targetPos);
                Debug.Log($"[Fleet] Moving selected ships to {targetPos}");
            }
        }

        private bool IsPointerOverUI()
        {
            return UnityEngine.EventSystems.EventSystem.current != null && 
                   UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
        }
    }
}