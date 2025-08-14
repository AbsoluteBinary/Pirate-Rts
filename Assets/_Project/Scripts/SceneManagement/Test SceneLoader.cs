//Testing
// using System;
// using System.Threading.Tasks;
// using TMPro;
// using UnityEngine;
// using UnityEngine.UI;
// using DG.Tweening;
// using Sirenix.OdinInspector;
// using System.Linq;
// using _Project.Scripts.EventBus; // For EventBus
//
// namespace _Project.Scripts.SceneManagement
// {
//     public class SceneLoader : MonoBehaviour
//     {
//         // Singleton Instance
//         //public static SceneLoader Instance { get; private set; }
//
//         // Event binding for LoadSceneGroupEvent
//         private EventBinding<LoadSceneGroupEvent> loadSceneGroupBinding;
//
//         [TitleGroup("Configuration")]
//         [Title("Fill Smoothing Speed")]
//         [SerializeField] private float fillSmoothingSpeed = 5f;
//
//         [TitleGroup("Configuration")]
//         [Title("Scene Groups")]
//         [SerializeField] private SceneGroup[] sceneGroups;
//
//         [TitleGroup("Configuration")]
//         [Title("Min Loading Time")]
//         [SerializeField] private float minLoadingTime = 3f;
//
//         [BoxGroup("UI Components")]
//         [Title("Background Image")]
//         [SerializeField] private Image backgroundImage;
//
//         [BoxGroup("UI Components")]
//         [Title("Loading Bar Border")]
//         [SerializeField] private Image loadingBarBorder;
//
//         [BoxGroup("UI Components")]
//         [Title("Loading Bar Fill")]
//         [SerializeField] private Image loadingBarFill;
//
//         [BoxGroup("UI Components")]
//         [Title("Loading Text")]
//         [SerializeField] private TextMeshProUGUI loadingText;
//
//         [BoxGroup("UI Components")]
//         [Title("Loading UI Canvas Group")]
//         [SerializeField] private CanvasGroup loadingUICanvasGroup;
//
//         [BoxGroup("UI Components")]
//         [Title("Login UI Canvas")]
//         [SerializeField] private Canvas loginUICanvas;
//
//         [BoxGroup("UI Components")]
//         [Title("Background Object")]
//         [SerializeField] private GameObject backgroundObject;
//
//         [BoxGroup("UI Components")]
//         [Title("Boot UI Container")]
//         [SerializeField] private GameObject bootUIContainer;
//
//         [FoldoutGroup("Runtime States")]
//         [ReadOnly]
//         private float targetProgress;
//
//         [FoldoutGroup("Runtime States")]
//         [ReadOnly]
//         public bool isLoading;
//
//         [FoldoutGroup("Runtime States")]
//         [ReadOnly]
//         private int currentGroupIndex = 0;
//
//         [FoldoutGroup("Runtime States")]
//         [ReadOnly]
//         private CanvasGroup loginUICanvasGroup;
//
//         [FoldoutGroup("Runtime States")]
//         [ReadOnly]
//         private bool isPreparingNewGroup;
//
//         [InlineEditor]
//         public readonly SceneGroupManager manager = new SceneGroupManager();
//
//         private void Awake()
//         {
//             // Singleton setup
//             if (Instance != null && Instance != this)
//             {
//                 Destroy(gameObject);
//                 return;
//             }
//             Instance = this;
//             DontDestroyOnLoad(gameObject);
//
//             // Initialize DOTween
//             DOTween.Init();
//             manager.OnSceneLoaded += sceneName => Debug.Log("Loaded: " + sceneName);
//             manager.OnSceneUnloaded += sceneName => Debug.Log("Unloaded: " + sceneName);
//             manager.OnSceneGroupLoaded += () => Debug.Log("Scene group loaded.");
//
//             // Register event listener for LoadSceneGroupEvent
//             loadSceneGroupBinding = new EventBinding<LoadSceneGroupEvent>(OnLoadSceneGroup);
//             EventBus<LoadSceneGroupEvent>.Register(loadSceneGroupBinding);
//
//             // Set initial UI state
//             if (backgroundImage != null)
//             {
//                 backgroundImage.gameObject.SetActive(true);
//                 backgroundImage.color = new Color(backgroundImage.color.r, backgroundImage.color.g, backgroundImage.color.b, 1f);
//                 backgroundImage.DOFade(1.0f, 1.0f);
//             }
//             if (loadingUICanvasGroup != null)
//             {
//                 loadingUICanvasGroup.gameObject.SetActive(true);
//                 loadingUICanvasGroup.alpha = 1f;
//             }
//             if (loadingBarFill != null) loadingBarFill.fillAmount = 0f;
//             if (loadingText != null) loadingText.text = "Loading...";
//
//             if (bootUIContainer != null) bootUIContainer.SetActive(true);
//         }
//
//         private void OnDestroy()
//         {
//             // Deregister event to prevent memory leaks
//             if (loadSceneGroupBinding != null)
//             {
//                 EventBus<LoadSceneGroupEvent>.Deregister(loadSceneGroupBinding);
//             }
//         }
//
//         private void OnLoadSceneGroup(LoadSceneGroupEvent evt)
//         {
//             Debug.Log($"SceneLoader: Received LoadSceneGroupEvent for index {evt.groupIndex}. TimeScale: {Time.timeScale}");
//             // Force unpause to avoid pause issues
//             Time.timeScale = 1f;
//             LoadSpecificSceneGroup(evt.groupIndex);
//         }
//
//         private async void Start()
//         {
//             if (manager.ActiveSceneGroup == null)
//             {
//                 await LoadSceneGroup(0);
//             }
//         }
//
//         public async Task LoadSpecificSceneGroup(int index)
//         {
//             Debug.Log($"SceneLoader: LoadSpecificSceneGroup called with index {index} on {gameObject.name}");
//             
//             if (index < 0 || index >= sceneGroups.Length)
//             {
//                 Debug.LogError($"Invalid scene group index: {index}. Valid range: 0 to {sceneGroups.Length - 1}");
//                 return;
//             }
//
//             Debug.Log($"SceneLoader: Scenes in group {index}: {(sceneGroups[index] != null ? string.Join(", ", sceneGroups[index].Scenes) : "null")}");
//             
//             if (backgroundObject != null) backgroundObject.SetActive(false);
//             if (loginUICanvas != null) loginUICanvas.gameObject.SetActive(false);
//
//             if (bootUIContainer != null)
//             {
//                 bootUIContainer.SetActive(index == 0);
//                 Debug.Log($"SceneLoader: Boot UI Container set to {(index == 0 ? "active" : "inactive")}");
//             }
//
//             currentGroupIndex = index;
//             NewGroupPrep();
//             Debug.Log($"SceneLoader: Preparing to load group {index}. TimeScale: {Time.timeScale}");
//             await LoadSceneGroup(index);
//             Debug.Log($"SceneLoader: LoadSpecificSceneGroup completed for index {index}");
//         }
//
//         public async Task LoadSceneGroup(int index)
//         {
//             Debug.Log($"SceneLoader: Loading scene group index {index}");
//             
//             if (index < 0 || index >= sceneGroups.Length)
//             {
//                 Debug.LogError($"Invalid scene group index: {index}");
//                 return;
//             }
//
//             currentGroupIndex = index;
//             isLoading = true;
//
//             LoadingProgress progress = new LoadingProgress();
//             progress.Progressed += target => 
//             {
//                 targetProgress = Mathf.Clamp01(target);
//                 Debug.Log($"SceneLoader: Progress updated to {targetProgress * 100f}% for group {index}");
//             };
//
//             ShowLoadingUI();
//
//             try
//             {
//                 Debug.Log($"SceneLoader: Starting to load scenes for group {index}: {(sceneGroups[index] != null ? string.Join(", ", sceneGroups[index].Scenes) : "null")}");
//                 Task loadTask = manager.LoadScenes(sceneGroups[index], progress);
//                 await loadTask;
//                 await Task.Yield();
//                 await Task.Delay(TimeSpan.FromSeconds(minLoadingTime));
//                 Debug.Log($"SceneLoader: Finished loading group {index}");
//             }
//             catch (Exception ex)
//             {
//                 Debug.LogError($"Failed to load scene group {index}: {ex.Message}");
//                 isLoading = false;
//             }
//             finally
//             {
//                 Debug.Log($"SceneLoader: Entering finally block for group {index}");
//                 HideLoadingUI();
//             }
//         }
//
//         private void ShowLoadingUI()
//         {
//             if (loadingUICanvasGroup != null)
//             {
//                 loadingUICanvasGroup.DOFade(1f, 0.4f);
//             }
//         }
//
//         private void HideLoadingUI()
//         {
//             Sequence seq = DOTween.Sequence();
//             if (backgroundImage != null) seq.Append(backgroundImage.DOFade(0f, 0.5f));
//             if (loadingUICanvasGroup != null) seq.Join(loadingUICanvasGroup.DOFade(0f, 0.5f));
//             if (backgroundObject != null && !isPreparingNewGroup)
//                 backgroundObject.SetActive(true);
//             
//             seq.OnComplete(() =>
//             {
//                 if (backgroundImage != null) backgroundImage.gameObject.SetActive(false);
//                 if (loadingUICanvasGroup != null) loadingUICanvasGroup.gameObject.SetActive(false);
//                 isPreparingNewGroup = false;
//                 ShowLoginUI();
//             });
//         }
//
//         private void ShowLoginUI()
//         {
//             if (isPreparingNewGroup) return;
//
//             if (loginUICanvas != null)
//             {
//                 loginUICanvasGroup = loginUICanvas.GetComponent<CanvasGroup>();
//                 if (loginUICanvasGroup != null)
//                 {
//                     loginUICanvasGroup.alpha = 0f;
//                     loginUICanvas.gameObject.SetActive(true);
//                     loginUICanvasGroup.DOFade(1f, 0.5f);
//                 }
//                 else
//                 {
//                     loginUICanvas.gameObject.SetActive(true);
//                 }
//                 Debug.Log("SceneLoader: Login UI activated via serialized reference.");
//             }
//             else
//             {
//                 var allObjects = FindObjectsOfType<GameObject>(true);
//                 var loginCanvasObj = allObjects.FirstOrDefault(go => go.name == "LoginMenuCanvas");
//
//                 if (loginCanvasObj != null)
//                 {
//                     loginUICanvasGroup = loginCanvasObj.GetComponent<CanvasGroup>();
//                     if (loginUICanvasGroup != null)
//                     {
//                         loginUICanvasGroup.alpha = 0f;
//                         loginCanvasObj.SetActive(true);
//                         loginUICanvasGroup.DOFade(1f, 0.5f);
//                     }
//                     else
//                     {
//                         loginCanvasObj.SetActive(true);
//                     }
//                     Debug.Log($"SceneLoader: LoginMenuCanvas found and activated in scene: {loginCanvasObj.scene.name}");
//                 }
//                 else
//                 {
//                     Debug.LogWarning("SceneLoader: LoginMenuCanvas not found (even inactive). Check name or scene.");
//                 }
//             }
//         }
//
//         private void Update()
//         {
//             if (!isLoading || loadingBarFill == null) return;
//
//             float currentFillAmount = loadingBarFill.fillAmount;
//             loadingBarFill.fillAmount = Mathf.Lerp(currentFillAmount, targetProgress, Time.deltaTime * fillSmoothingSpeed);
//             if (loadingText != null)
//                 loadingText.text = $"Loading... {Mathf.RoundToInt(targetProgress * 100f)}%";
//         }
//
//         public void LoadNextSceneGroupForButton()
//         {
//             if (backgroundObject != null) backgroundObject.SetActive(false);
//             if (loginUICanvas != null) loginUICanvas.gameObject.SetActive(false);
//             
//             if (isLoading)
//             {
//                 Debug.LogWarning("SceneLoader: Cannot load next scene group while loading is in progress.");
//                 return;
//             }
//
//             ToggleNextSceneGroup();
//         }
//
//         public void ToggleNextSceneGroup()
//         {
//             if (sceneGroups == null || sceneGroups.Length == 0)
//             {
//                 Debug.LogWarning("SceneLoader: No scene groups assigned to SceneLoader.");
//                 return;
//             }
//             currentGroupIndex = (currentGroupIndex + 1) % sceneGroups.Length;
//             
//             if (bootUIContainer != null)
//             {
//                 bootUIContainer.SetActive(currentGroupIndex == 0);
//             }
//
//             LoadSceneGroup(currentGroupIndex);
//             NewGroupPrep();
//         }
//         
//         private void NewGroupPrep()
//         {
//             isPreparingNewGroup = true;
//         }
//     }
//
//     public class LoadingProgress : IProgress<float>
//     {
//         public event Action<float> Progressed;
//         private const float ratio = 1f;
//
//         public void Report(float value)
//         {
//             float normalizedValue = Mathf.Clamp01(value / ratio);
//             Progressed?.Invoke(normalizedValue);
//         }
//     }
//
//     public class BackgroundObject : MonoBehaviour
//     {
//         private Renderer rend;
//
//         private void Awake()
//         {
//             rend = GetComponent<Renderer>();
//             if (rend != null)
//             {
//                 Color color = rend.material.color;
//                 rend.material.color = new Color(color.r, color.g, color.b, 0f);
//             }
//         }
//
//         public void FadeIn()
//         {
//             if (rend != null)
//             {
//                 rend.material.DOFade(1f, 1f).SetId("FadeInBackgroundObjects");
//             }
//         }
//     }
// }