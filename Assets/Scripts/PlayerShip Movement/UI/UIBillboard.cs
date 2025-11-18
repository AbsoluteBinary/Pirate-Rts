using UnityEngine;

namespace PlayerShip_Movement.UI
{
    public class UIBillboard : MonoBehaviour
    {
        [Header("Optional: Assign manually if needed")]
        [SerializeField] private Camera targetCamera; // Drag your player camera here if you want

        private void LateUpdate()
        {
            // NEW: Always get the current active camera — never caches the wrong one!
            Camera cam = targetCamera != null ? targetCamera : Camera.main;

            if (cam == null) return;

            // Make UI face the camera properly
            transform.LookAt(cam.transform.position);

            // Optional: Keep it upright (prevents tilting on sloped cameras)
            transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
        }
    }
}
