using _Project.Scripts.Fleet;
using _Project.Scripts.PlayerShip_Movement.Combat;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Scripts.PlayerShip_Movement
{
    public class MapSailController : MonoBehaviour
    {
        [SerializeField] private GameObject ship;
        [SerializeField] private GameObject markerPrefab;
        [SerializeField] private Camera mapCamera;

        private GameObject _pin;
        private Tween _pinTween;

        public Transform thePawn;
        public PlayerCombatMovementController Helm =>
            ship != null ? ship.GetComponent<PlayerCombatMovementController>() : null;

        private void OnDestroy()
        {
            _pinTween?.Kill();
            if (_pin != null)
                Destroy(_pin);
        }

        private void Update()
        {
            if (markerPrefab == null || Mouse.current == null || !Mouse.current.rightButton.wasPressedThisFrame)
                return;

            Camera cam = mapCamera != null ? mapCamera : Camera.main;
            if (cam == null)
                return;

            Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (!new Plane(Vector3.up, Vector3.zero).Raycast(ray, out float distance))
                return;

            PlaceMarker(ray.GetPoint(distance));
        }

        public void OrderMove(Vector3 worldPosition)
        {
            SailTo(worldPosition);
        }

        public void AttachFlagship(GameObject prefab, string shipName, Transform pawn)
        {
            thePawn = pawn;

            if (prefab == null || pawn == null)
                return;

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
                PendingBattle.Clear();
            }

            Debug.Log($"<color=lime>[Fleet] Controller ship is now '{ship.name}'</color>");
        }

        private void PlaceMarker(Vector3 point)
        {
            if (_pin != null)
                Destroy(_pin);

            _pin = Instantiate(markerPrefab, point + Vector3.up * 12f, Quaternion.identity);
            _pinTween?.Kill();
            _pinTween = _pin.transform.DOMove(point + Vector3.up * 1f, 0.6f)
                .SetEase(Ease.OutBounce)
                .OnComplete(() =>
                {
                    SailTo(point);
                    var pin = _pin;
                    _pin = null;
                    if (pin != null)
                        Destroy(pin, 1.5f);
                });
        }

        private void SailTo(Vector3 location)
        {
            var helm = Helm;
            if (helm == null)
            {
                Debug.LogWarning("[Move] No helm on the pawn.");
                return;
            }

            helm.MoveToLocation(location);
        }
    }
}