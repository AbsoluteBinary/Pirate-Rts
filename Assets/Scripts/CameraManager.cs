using UnityEngine;

public class CameraManager : MonoBehaviour
{
    // public static CameraManager Instance { get; private set; }
    // [SerializeField] private Camera buildCamera;
    // [SerializeField] private Camera playerCamera;
    //
    // private void Awake()
    // {
    //     if (Instance == null)
    //     {
    //         Instance = this;
    //         DontDestroyOnLoad(gameObject);
    //         Debug.Log("CameraManager initialized.");
    //     }
    //     else
    //     {
    //         Destroy(gameObject);
    //     }
    // }
    //
    // public Camera GetBuildCamera() => buildCamera;
    // public Camera GetPlayerCamera() => playerCamera;
    //
    // // Optional: Update cameras for specific scenes
    // public void UpdateCamerasForScene(string sceneName)
    // {
    //     if (sceneName == "HarbourScene")
    //     {
    //         buildCamera = GameObject.Find("BuildCamera")?.GetComponent<Camera>();
    //         playerCamera = GameObject.Find("PlayerCamera")?.GetComponent<Camera>();
    //         Debug.Log($"CameraManager: Updated cameras for {sceneName}. BuildCamera: {buildCamera}, PlayerCamera: {playerCamera}");
    //     }
    // }
}