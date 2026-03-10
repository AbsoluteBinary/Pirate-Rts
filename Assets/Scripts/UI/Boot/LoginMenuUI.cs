using _Project.Scripts.SceneManagement;
using UI.IMGUI;
using UI.WorldMap;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Boot // ← adjust namespace to match your project
{
    [RequireComponent(typeof(UIDocument))]
    public class LoginMenuUI : MonoBehaviour
    {
        [SerializeField] private int targetSceneGroupIndex = 2;
        
        private void Awake()
        {
            WorldSpaceInteractionsEventBus.HarbourButtonClicked += OnStartGameClicked;
        }
        private void OnEnable()
        {
            var doc = GetComponent<UIDocument>();
            if (doc == null || doc.rootVisualElement == null)
            {
                Debug.LogError("UIDocument missing on LoginMenu!");
                return;
            }

            var root = doc.rootVisualElement;
            root.Clear();

            // Root container - centers everything
            var container = new VisualElement { name = "LoginPanel" };
            container.style.position = Position.Absolute;
            container.style.top = Length.Percent(50);
            container.style.left = Length.Percent(50);
            container.style.translate = new Translate(Length.Percent(-50), Length.Percent(-50));
            container.style.width = Length.Percent(40);     // 40% width – adjust for your liking
            container.style.minWidth = 400;                 // don't get too small on low res
            container.style.backgroundColor = new StyleColor(new Color(0.04f, 0.08f, 0.16f, 0.85f)); // dark semi-transparent
            container.style.borderTopLeftRadius     = 16;
            container.style.borderTopRightRadius    = 16;
            container.style.borderBottomLeftRadius  = 16;
            container.style.borderBottomRightRadius = 16;
            container.style.borderTopWidth    = 2;
            container.style.borderRightWidth  = 2;
            container.style.borderBottomWidth = 2;
            container.style.borderLeftWidth   = 2;
            container.style.borderTopColor    = new StyleColor(new Color(0f, 0.5f, 1f)); // azure border
            container.style.borderRightColor  = new StyleColor(new Color(0f, 0.5f, 1f));
            container.style.borderBottomColor = new StyleColor(new Color(0f, 0.5f, 1f));
            container.style.borderLeftColor   = new StyleColor(new Color(0f, 0.5f, 1f));
            container.style.paddingTop = 40;
            container.style.paddingBottom = 40;
            container.style.paddingLeft = 30;
            container.style.paddingRight = 30;
            container.style.alignItems = Align.Center;
            container.style.flexDirection = FlexDirection.Column;

            // Title
            var title = new Label("Legendary SeaDog's");
            title.style.fontSize = 48;
            title.style.color = new StyleColor(new Color(0f, 0.5f, 1f)); // azure
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.marginBottom = 40;
            title.style.unityTextAlign = TextAnchor.MiddleCenter;
            container.Add(title);

            // Buttons – orange accent
            AddLoginButton(container, "Start Game");
            AddLoginButton(container, "Enter Harbour");
            AddLoginButton(container, "Close Game");

            root.Add(container);

            Debug.Log("<color=cyan>Login Menu rebuilt – centered azure/orange style</color>");
        }

        private void AddLoginButton(VisualElement parent, string text)
        {
            var btn = new Button { text = text };
            
            btn.clicked += () =>
            {
                switch (text)
                {
                    case "Start Game":
                        OnStartGameClicked();
                        break;

                    case "Enter Harbour":
                        OnEnterHarbourClicked();
                        break;

                    case "Close Game":
                        OnCloseGameClicked();
                        break;
                }
            };
            
            
            btn.style.width = Length.Percent(80);
            btn.style.height = 60;
            btn.style.marginBottom = 20;
            btn.style.fontSize = 24;
            btn.style.unityFontStyleAndWeight = FontStyle.Bold;
            btn.style.backgroundColor = new StyleColor(new Color(1f, 0.43f, 0.13f)); // vivid orange #FF6F20-ish
            btn.style.color = Color.white;
            btn.style.borderTopLeftRadius     = 12;
            btn.style.borderTopRightRadius    = 12;
            btn.style.borderBottomLeftRadius  = 12;
            btn.style.borderBottomRightRadius = 12;
            btn.style.borderTopWidth    = 0; // no border, or add if you want outline
            btn.style.borderRightWidth  = 0;
            btn.style.borderBottomWidth = 0;
            btn.style.borderLeftWidth   = 0;

            // Optional hover/active feedback (UI Toolkit supports :hover via USS, but programmatic here)
            btn.RegisterCallback<PointerEnterEvent>(evt => btn.style.backgroundColor = new StyleColor(new Color(1f, 0.55f, 0.25f))); // lighter orange
            btn.RegisterCallback<PointerLeaveEvent>(evt => btn.style.backgroundColor = new StyleColor(new Color(1f, 0.43f, 0.13f)));
            btn.RegisterCallback<PointerDownEvent>(evt => 
                btn.style.scale = new StyleScale(new Scale(new Vector2(0.95f, 0.95f))));

            btn.RegisterCallback<PointerUpEvent>(evt => 
                btn.style.scale = new StyleScale(new Scale(new Vector2(1f, 1f))));

            parent.Add(btn);
        }
        
        // Add these private methods in the same class
        private void OnStartGameClicked()
        {
            if (IMGUILoadingOverlay.Instance != null)
            {
                IMGUILoadingOverlay.Instance.TriggerLoadingScreen();
            }
            
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.ToggleNextSceneGroup();
            }
            else
            {
                Debug.LogError("SceneLoader.Instance is null");
            }
            
            Debug.Log("Start Game pressed → load save / new game flow");
            // Example: SceneLoader.Instance?.BeginSceneTransition(1); // or whatever your next scene index is
        }

        private void OnEnterHarbourClicked()
        {
            if (IMGUILoadingOverlay.Instance != null)
            {
                IMGUILoadingOverlay.Instance.TriggerLoadingScreen();
            }
            
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.ToggleNextSceneGroup();
            }
            
            _ = SceneLoader.Instance.BeginSceneTransition(targetSceneGroupIndex);
            
            Debug.Log("Enter Harbour pressed → load harbour scene directly");
            // Example: SceneLoader.Instance?.LoadSpecificSceneGroup(1); // or whatever your harbour group is
        }

        private void OnCloseGameClicked()
        {
            Debug.Log("Close Game pressed → quit application");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
        }
    }
}
