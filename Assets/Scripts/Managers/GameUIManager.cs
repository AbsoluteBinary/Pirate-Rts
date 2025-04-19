using UnityEngine;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace Managers
{

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance { get; private set; }

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && this != Instance)
        {
            Debug.LogWarning($"Duplicate GameUIManager on {gameObject.name}, destroying this instance.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log($"GameUIManager initialized as singleton on {gameObject.name}.");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I) && SceneUIManager.Instance != null)
        {
            GameObject adminPanel = SceneUIManager.Instance.GetAdminPanel();
            if (adminPanel != null)
            {
                adminPanel.SetActive(!adminPanel.activeSelf);
            }
        }
    }
}
}