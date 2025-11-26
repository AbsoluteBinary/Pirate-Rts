using TMPro;
using UnityEngine;

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
