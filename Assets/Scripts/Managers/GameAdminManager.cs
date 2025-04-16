using UnityEngine;

namespace Managers
{
    using UnityEngine;

public class GameAdminManager : MonoBehaviour
{
    public static GameAdminManager Instance { get; private set; }
    [SerializeField] private GameObject adminCanvasPrefab; // Reference to the Canvas prefab
    private GameObject adminPanel; // Reference to the Admin panel

    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("GameAdminManager initialized as singleton.");
        }
        else
        {
            Debug.LogWarning("Duplicate GameAdminManager found, destroying this instance.");
            Destroy(gameObject);
            return;
        }

        // Instantiate the Canvas prefab
        if (adminCanvasPrefab != null)
        {
            GameObject canvasInstance = Instantiate(adminCanvasPrefab);
            canvasInstance.name = "PersistentAdminCanvas";
            DontDestroyOnLoad(canvasInstance);
            Debug.Log("Canvas instantiated: " + canvasInstance.name);
            adminPanel = canvasInstance.transform.Find("Admin")?.gameObject;

            if (adminPanel != null)
            {
                adminPanel.SetActive(false);
                Debug.Log($"Admin panel found, parent: {adminPanel.transform.parent.name}, set to inactive.");
            }
            else
            {
                Debug.LogError("Admin panel not found in Canvas prefab! Check prefab hierarchy.");
                foreach (Transform child in canvasInstance.transform)
                {
                    Debug.Log("Child found: " + child.name);
                }
            }
        }
        else
        {
            Debug.LogError("Admin Canvas prefab is not assigned!");
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            Debug.Log("I key pressed, toggling Admin panel!");
            if (adminPanel != null)
            {
                Debug.Log($"Before toggle, Admin panel parent: {adminPanel.transform.parent?.name ?? "None"}");
                adminPanel.SetActive(!adminPanel.activeSelf);
                Debug.Log($"Admin panel toggled to: {adminPanel.activeSelf}, parent: {adminPanel.transform.parent?.name ?? "None"}");
            }
            else
            {
                Debug.LogWarning("Admin panel reference is missing!");
            }
        }
    }
}
}