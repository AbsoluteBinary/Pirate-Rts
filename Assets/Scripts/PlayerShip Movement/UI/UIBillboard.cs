using UnityEngine;

namespace PlayerShip_Movement.UI
{
    public class UIBillboard : MonoBehaviour
    {
        private Camera mainCamera;

        private void Start()
        {
            mainCamera = Camera.main; // Cache once for efficiency
        }

        private void LateUpdate()
        {
            if (mainCamera == null) return; // Safety check (e.g., if camera changes post-scene load)
            transform.LookAt(mainCamera.transform.position);
            // Optional: Constrain to Y-axis only if you want flat billboarding (no tilt)
            // transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
        }
    }
}
