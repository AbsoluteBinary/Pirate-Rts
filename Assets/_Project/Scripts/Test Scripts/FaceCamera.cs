using TMPro;
using UnityEngine;

namespace _Project.Scripts.Test_Scripts
{
    [RequireComponent(typeof(TextMeshPro))]
    public class FaceCamera : MonoBehaviour
    {
        private Camera cam;

        private void Awake() => cam = Camera.main;

        private void LateUpdate()
        {
            if (cam != null)
                transform.rotation = Quaternion.LookRotation(transform.position - cam.transform.position);
        }
    }
}
