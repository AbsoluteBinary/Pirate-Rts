using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using _Project.Scripts.EventBus;
using _Project.Scripts.Harbour.Data;

// For EventBus

namespace _Project.Scripts.SceneManagement
{
    public class SceneLoader : MonoBehaviour
    {
        // Singleton Instance
        public static SceneLoader Instance { get; private set; }
        
        // Event binding for LoadSceneGroupEvent
        private EventBinding<LoadSceneGroupEvent> loadSceneGroupBinding;

        // Configuration for loading bar animation speed
        //TODO: Removed*
        //[SerializeField] private float fillSmoothingSpeed = 5f;
        // Array of scene groups to load
        [SerializeField] private SceneGroup[] sceneGroups;
        // Minimum time to display loading screen
        [SerializeField] private float minLoadingTime = 3f;
        
        // UI Component References
        [SerializeField] private Image backgroundImage;          // Background image shown during loading
        [SerializeField] private Image loadingBarBorder;        // Static border of the loading bar
        [SerializeField] private Image loadingBarFill;          // Fill image that progresses
        [SerializeField] private TextMeshProUGUI loadingText;   // Text displaying "Loading..." or progress
        //[SerializeField] private CanvasGroup loadingUICanvasGroup; // Group containing loading UI elements
        
        // Tracks the target progress of the loading bar
        private float targetProgress;
        // Flag to indicate if a scene group is currently loading
        public bool isLoading;
        // Index of the current scene group
        private int currentGroupIndex = 0;
        // Cached reference to login UI CanvasGroup
        //private CanvasGroup loginUICanvasGroup;

        // Solution 3: Flag to indicate if preparing for a new group (prevents enabling)
        private bool isPreparingNewGroup;

        // Manages scene loading and unloading
        public readonly SceneGroupManager manager = new SceneGroupManager();
        
        // private void OnApplicationQuit()
        // {
        //     ResetUi(); // Call your function here
        // }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
    
            DOTween.Init();

            manager.OnSceneLoaded += _ => { };
            manager.OnSceneUnloaded += _ => { };
            manager.OnSceneGroupLoaded += () => { };

            loadSceneGroupBinding = new EventBinding<LoadSceneGroupEvent>(OnLoadSceneGroupEvent);
            EventBus<LoadSceneGroupEvent>.Register(loadSceneGroupBinding);

            if (backgroundImage != null)
            {
                backgroundImage.gameObject.SetActive(true);
                backgroundImage.color = new Color(backgroundImage.color.r, backgroundImage.color.g, backgroundImage.color.b, 1f);
                backgroundImage.DOFade(1.0f, 1.0f);
            }
        }

        private void OnDestroy()
        {
            // Deregister event to prevent memory leaks
            EventBus<LoadSceneGroupEvent>.Deregister(loadSceneGroupBinding);
        }

        private void OnLoadSceneGroupEvent(LoadSceneGroupEvent e)
        {
            _ = LoadSpecificSceneGroup(e.groupIndex);
        }

        // private async void Start()
        // {
        //     if (manager.ActiveSceneGroup == null)
        //     {
        //         await LoadSceneGroup(0);
        //     }
        // }

        public async Task LoadSceneGroup(int index)
        {
            //Debug.Log("Loading scene group...");
            if (index < 0 || index >= sceneGroups.Length)
            {
                Debug.LogError($"<color =yellow>Invalid scene group index: " + index);
                return;
            }

            currentGroupIndex = index;
            isLoading = true;

            LoadingProgress progress = new LoadingProgress();
            progress.Progressed += target => targetProgress = Mathf.Clamp01(target);

            ShowLoadingUI();

            try
            {
                Task loadTask = manager.LoadScenes(sceneGroups[index], progress);
                await loadTask;
                await Task.Yield(); // Wait for the next frame
                await Task.Delay(TimeSpan.FromSeconds(minLoadingTime));
            }
            catch (Exception ex)
            {
                Debug.LogError($"<color=red>Failed to load scene group {index}: {ex.Message}");
            }
            finally
            {
                HideLoadingUI();
            }
        }

        public void ShowLoadingUI()
        {
            // if (loadingUICanvasGroup != null)
            // {
            //     loadingUICanvasGroup.DOFade(1f, 0.4f);
            // }
        }
        
        public void InitiateLoad(int index)
        {
            ShowLoadingUI(); // Covers the flash instantly
            _ = LoadSpecificSceneGroup(index);
        }

        private void HideLoadingUI()
        {
            Sequence seq = DOTween.Sequence();
            if (backgroundImage != null) seq.Append(backgroundImage.DOFade(0f, 0.5f));
           // if (loadingUICanvasGroup != null) seq.Join(loadingUICanvasGroup.DOFade(0f, 0.5f));
    
            // Insert callback at end of fade (almost faded out) to show login UI if Boot group
            seq.AppendCallback(() =>
            {
                if (currentGroupIndex == 0)
                {
                    //ShowLoginUI();
                }
                else
                {
                    //Debug.Log("Skipping ShowLoginUI() for non-Boot group: " + currentGroupIndex);
                }
            });

            seq.OnComplete(() =>
            {
                if (backgroundImage != null) backgroundImage.gameObject.SetActive(false);
               // if (loadingUICanvasGroup != null) loadingUICanvasGroup.gameObject.SetActive(false);
                isPreparingNewGroup = false;
                isLoading = false; // Reset isLoading to allow new loads
                //Debug.Log("isLoading reset to false"); // Optional: For testing
            });
        }

        // private void ShowLoginUI()
        // {
        //     var mainMenuManager = MainMenuDataIOManager.Instance;
        //     if (mainMenuManager != null)
        //     {
        //         mainMenuManager.ToggleCanvasOn();
        //         mainMenuManager.ToggleBackgroundOn();
        //     }
        //     else
        //     {
        //         Debug.LogWarning("MainMenuDataIOManager instance not found.");
        //     }
        // }
        //TODO remove this! (Moved/Moving to IMGUILoadingOverlay(Currently but should posibly move to UIManager))
        // private void HideLoginUIBootOut()
        // {
        //     var mainMenuManager = MainMenuDataIOManager.Instance;
        //     if (mainMenuManager == null)
        //     {
        //         Debug.LogWarning($"<color=orange>MainMenuDataIOManager instance not found.");
        //         return;
        //     }
        //
        //     // Hide the login/main menu UI
        //     mainMenuManager.ToggleBackgroundOff();
        //     mainMenuManager.ToggleCanvasOff();
        //
        //     // === CLEAN UP OLD COMPONENT BOXES ===
        //     // Boot scene cleanup (always safe to call)
        //     mainMenuManager.DisableBootComponentIOBox();
        //
        //     // Harbour scene cleanup (if you have it)
        //     mainMenuManager.DisableHarbourComponentIOBox();  // ← Your new method!
        //
        //     // Runtime/world scene cleanup (the one with tag "ComponentBoxIO")
        //     mainMenuManager.DisableComponentIOBox();
        // }

        // private void ResetUi()
        // {
        //     var mainMenuManager = MainMenuDataIOManager.Instance;
        //     if (mainMenuManager != null)
        //     {
        //         mainMenuManager.SaveUIState();
        //     }
        // }

        private void Update()
        {
            if (!isLoading || loadingBarFill == null) return;

            float currentFillAmount = loadingBarFill.fillAmount;
            //TODO: Removed*
            //loadingBarFill.fillAmount = Mathf.Lerp(currentFillAmount, targetProgress, Time.deltaTime * fillSmoothingSpeed);
            if (loadingText != null)
                loadingText.text = $"Loading... {Mathf.RoundToInt(targetProgress * 100f)}%";
        }

        public void LoadNextSceneGroupForButton()
        {
            //Debug.Log("Load next Scene Group");
            if (isLoading)
            {
                //Debug.LogWarning($"<color=yellow>Cannot load next scene group while loading is in progress.");
                return;
            }

            _ = ToggleNextSceneGroup();
        }

        // New method: Load a specific scene group by index (for external calls)
        public async Task LoadSpecificSceneGroup(int index)
        {
            if (index < 0 || index >= sceneGroups.Length)
            {
                Debug.LogError($"<color=red>Invalid scene group index: " + index);
                return;
            }

            // Cache old group for unloading after new load
            SceneGroup oldGroup = manager.ActiveSceneGroup;

            // New: Disable old container before loading new (prevents overlap/pause)
            // var mainMenuManager = MainMenuDataIOManager.Instance;
            // if (mainMenuManager != null && oldGroup != null)
            // {
            //     int oldIndex = Array.IndexOf(sceneGroups, oldGroup);
            //     if (oldIndex == 0)
            //     {
            //         mainMenuManager.DisableBootComponentIOBox();
            //     }
            //     else
            //     {
            //         mainMenuManager.DisableComponentIOBox();
            //     }
            // }

            currentGroupIndex = index;
            NewGroupPrep();

            // Load new group first (this sets the new active scene)
            await LoadSceneGroup(index);
            
            if (index == 2) // e.g. 2
            {
                if (HarbourController.Instance != null)
                {
                    HarbourController.Instance.OnHarbourEntered();
                }
            }

            // Then unload old if exists and different
            if (oldGroup != null && oldGroup != sceneGroups[index])
            {
                //Debug.Log("Unloading previous scene group after new activation...");
                await manager.UnloadScenes();
            }
        }
        
        /// <summary>
        /// NEW PROFESSIONAL ENTRY POINT — Smooth fade transition + load
        /// Use this from ALL buttons instead of LoadSpecificSceneGroup directly
        /// </summary>
        public async Task BeginSceneTransition(int targetGroupIndex)
        {
            // ───── STEP 1: SMOOTH FADE TO BLACK (covers everything beautifully) ─────
            //await FadeToBlack();

            // ───── STEP 2: YELLOW DEBUG MESSAGE ─────
            //Debug.Log($"<color=yellow>Call start Load process → Target Group: {targetGroupIndex}</color>");

            // ───── STEP 3: Load the new scene group safely behind black screen ─────
            await LoadSpecificSceneGroup(targetGroupIndex);

            // ───── STEP 4: SMOOTH FADE BACK IN (reveals new world perfectly) ─────
            //await FadeFromBlack();
        }
        

        public async Task ToggleNextSceneGroup()
        {
            //HideLoginUIBootOut();
            if (sceneGroups == null || sceneGroups.Length == 0)
            {
                Debug.LogWarning($"<color=red>No scene groups assigned to SceneLoader.");
                return;
            }
            currentGroupIndex = (currentGroupIndex + 1) % sceneGroups.Length;
    
            // Cache old group
            SceneGroup oldGroup = manager.ActiveSceneGroup;

            // New: Disable old container before loading new
            // var mainMenuManager = MainMenuDataIOManager.Instance;
            // if (mainMenuManager != null && oldGroup != null)
            // {
            //     int oldIndex = Array.IndexOf(sceneGroups, oldGroup);
            //     if (oldIndex == 0)
            //     {
            //         mainMenuManager.DisableBootComponentIOBox();
            //     }
            //     else
            //     {
            //         mainMenuManager.DisableComponentIOBox();
            //     }
            // }

            // Load new group
            await LoadSceneGroup(currentGroupIndex);
            NewGroupPrep();

            // Unload old
            if (oldGroup != null && oldGroup != sceneGroups[currentGroupIndex])
            {
                //Debug.Log("Testing unload: Unloading previous scene group...");
                await manager.UnloadScenes();
            }
        }
        
        private void NewGroupPrep()
        {
            isPreparingNewGroup = true;
        }
        
        // Add this at the end of SceneLoader.cs

        [Header("Dev Testing")]
        [SerializeField] private bool bypassLoginForTesting = false; // Inspector toggle (Editor-only)

        private async void Start()
        {
            if (manager.ActiveSceneGroup == null)
            {
#if UNITY_EDITOR // Dev-only bypass
                if (bypassLoginForTesting)
                {
                    await LoadSceneGroup(0); // Load Boot minimally
                    await BypassLoginAndLoadNext();
                    return;
                }
#endif
                await LoadSceneGroup(0); // Original Boot load + login
            }
        }

        private async Task BypassLoginAndLoadNext()
        {
            //Debug.Log("Bypassing login for testing...");

            // Step 1: Instantly hide login UI (skip fades for speed)
            // var mainMenuManager = MainMenuDataIOManager.Instance;
            // if (mainMenuManager != null)
            // {
            //     mainMenuManager.ToggleCanvasOff();
            //     mainMenuManager.ToggleBackgroundOff();
            //     mainMenuManager.DisableBootComponentIOBox(); // Handles audio/EventSystem disable
            // }

            // Step 2: Load next group (e.g., index 1) with minimal delay
            minLoadingTime = 0f; // Override for testing (restore if needed)
            currentGroupIndex = 1; // Or your desired next index
            await LoadSceneGroup(currentGroupIndex);

            // Step 3: Unload Boot extras (UI/camera/audio/scene group)
            if (manager.ActiveSceneGroup != null)
            {
                await manager.UnloadScenes(); // Unloads previous (Boot)
                //Debug.Log("Unloaded Boot scene group.");
            }

            // Optional: Disable Boot camera if separate (find via tag/type)
            var bootCamera = GameObject.FindWithTag("BootCamera"); // Add tag if needed
            if (bootCamera != null) bootCamera.SetActive(false);

            // Restore minLoadingTime if changed
            minLoadingTime = 3f;
            //Debug.Log("Bypass complete—ready for testing!");
        }
    }

    public class LoadingProgress : IProgress<float>
    {
        public event Action<float> Progressed;
        private const float ratio = 1f;

        public void Report(float value)
        {
            float normalizedValue = Mathf.Clamp01(value / ratio);
            Progressed?.Invoke(normalizedValue);
        }
    }
    
    
}