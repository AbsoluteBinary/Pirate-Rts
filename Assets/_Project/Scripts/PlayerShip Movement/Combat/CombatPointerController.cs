using DG.Tweening;
using TGS;
using UnityEngine;
using _Project.Scripts.Fleet;

namespace _Project.Scripts.PlayerShip_Movement.Combat
{
    public class CombatPointerController : MonoBehaviour
    {
        [Header("Terrain Grid System")]
        [SerializeField] private TerrainGridSystem tgs;

        [Header("Pointer Visual")]
        [SerializeField] private GameObject pointerPrefab;
        [SerializeField, Range(0.5f, 10f)] private float fadeDuration = 0.8f;
        [SerializeField] private float dropHeight = 15f;
        [SerializeField] private float dropDuration = 0.8f;
        [SerializeField] private Ease dropEase = Ease.OutBounce;

        private GameObject currentPointer;

        private void Awake()
        {
            if (pointerPrefab == null)
                Debug.LogError("CombatPointerController: pointerPrefab is missing!");
        }

        private void Start()
        {
            if (tgs == null)
                tgs = TerrainGridSystem.instance;

            if (tgs != null)
                tgs.OnCellClick += OnCellClick;
            else
                Debug.LogError("CombatPointerController: Could not find TerrainGridSystem!");
        }

        private void OnCellClick(TerrainGridSystem sender, int cellIndex, int buttonIndex)
        {
            if (buttonIndex != 1) return; // Only right-click

            // Clean up previous pointer
            if (currentPointer != null)
                Destroy(currentPointer);

            Vector3 targetPos = tgs.CellGetPosition(cellIndex);

            // Spawn pointer above
            Vector3 spawnPos = targetPos + Vector3.up * dropHeight;
            currentPointer = Instantiate(pointerPrefab, spawnPos, Quaternion.identity);

            // Face camera horizontally
            if (Camera.main != null)
            {
                currentPointer.transform.LookAt(Camera.main.transform.position + Vector3.up * 2f);
                currentPointer.transform.rotation = Quaternion.Euler(0, currentPointer.transform.eulerAngles.y, 0);
            }

            // Drop animation
            currentPointer.transform
                .DOMoveY(targetPos.y + 1f, dropDuration)
                .SetEase(dropEase)
                .OnComplete(() =>
                {
                    // === UPDATED: Use Fleet System instead of single ship ===
                    if (FleetManager.Instance != null)
                    {
                        FleetManager.Instance.MoveFleetToLocation(targetPos);
                        Debug.Log($"[CombatPointer] Fleet moving to: {targetPos}");
                    }
                    else
                    {
                        Debug.LogWarning("[CombatPointer] FleetManager not found!");
                    }

                    // Fade out and destroy pointer
                    var rend = currentPointer.GetComponentInChildren<Renderer>();
                    if (rend != null)
                    {
                        rend.material.DOFade(0f, fadeDuration)
                            .OnComplete(() => Destroy(currentPointer));
                    }
                    else
                    {
                        Destroy(currentPointer, fadeDuration);
                    }
                });
        }

        private void OnDestroy()
        {
            if (tgs != null)
                tgs.OnCellClick -= OnCellClick;
        }
    }
}