using _Project.Scripts.SceneManagement;
using _Project.Scripts.UI.IMGUI;
using _Project.Scripts.UI.WorldMap;
using UnityEngine;
using UnityEngine.UIElements;

namespace _Project.Scripts.UI.Boot
{
    [RequireComponent(typeof(UIDocument))]
    public class LoginPanel : MonoBehaviour
    {
        [SerializeField] private int targetSceneGroupIndex = 2;
        [SerializeField] private int combatSceneIndex = 3;
        
        private void Awake()
        {
            WorldSpaceInteractionsEventBus.HarbourButtonClicked += OnStartGameClicked;
        }

        private void OnDestroy()
        {
            WorldSpaceInteractionsEventBus.HarbourButtonClicked -= OnStartGameClicked;
        }

        private void OnEnterHarbourClicked()
        {
            if (IMGUILoadingOverlay.Instance != null)
                IMGUILoadingOverlay.Instance.TriggerLoadingScreen();

            if (SceneLoader.Instance != null)
                _ = SceneLoader.Instance.BeginSceneTransition(targetSceneGroupIndex);
            else
                Debug.LogError("SceneLoader.Instance is null");
        }

        private bool _uiBuilt = false;
        private void OnEnable()
        {
            var doc = GetComponent<UIDocument>();
            if (doc == null || doc.rootVisualElement == null)
            {
                Debug.LogError("UIDocument missing on LoginMenu!");
                return;
            }

            var root = doc.rootVisualElement;

            if (_uiBuilt && root.Q("LoginPanel") != null)
                return;

            root.Clear();

            var container = new VisualElement { name = "LoginPanel" };
            container.style.display = DisplayStyle.None;
            container.style.position = Position.Absolute;
            container.style.top = Length.Percent(50);
            container.style.left = Length.Percent(50);
            container.style.translate = new Translate(Length.Percent(-50), Length.Percent(-50));
            container.style.width = Length.Percent(40);
            container.style.minWidth = 400;
            container.style.backgroundColor = new StyleColor(new Color(0.04f, 0.08f, 0.16f, 0.85f));
            container.style.borderTopLeftRadius     = 16;
            container.style.borderTopRightRadius    = 16;
            container.style.borderBottomLeftRadius  = 16;
            container.style.borderBottomRightRadius = 16;
            container.style.borderTopWidth    = 2;
            container.style.borderRightWidth  = 2;
            container.style.borderBottomWidth = 2;
            container.style.borderLeftWidth   = 2;
            container.style.borderTopColor    = new StyleColor(new Color(0f, 0.5f, 1f));
            container.style.borderRightColor  = new StyleColor(new Color(0f, 0.5f, 1f));
            container.style.borderBottomColor = new StyleColor(new Color(0f, 0.5f, 1f));
            container.style.borderLeftColor   = new StyleColor(new Color(0f, 0.5f, 1f));
            container.style.paddingTop = 40;
            container.style.paddingBottom = 40;
            container.style.paddingLeft = 30;
            container.style.paddingRight = 30;
            container.style.alignItems = Align.Center;
            container.style.flexDirection = FlexDirection.Column;

            var title = new Label("Legendary SeaDog's");
            title.style.fontSize = 40;
            title.style.color = new StyleColor(new Color(0f, 0.5f, 1f));
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.marginBottom = 40;
            title.style.unityTextAlign = TextAnchor.MiddleCenter;
            container.Add(title);

            AddLoginButton(container, "Start Game");
            AddLoginButton(container, "Enter Harbour");
            AddLoginButton(container, "Load Combat Scene");   // ← NEW BUTTON
            AddLoginButton(container, "Close Game");

            root.Add(container);
        }

        private void AddLoginButton(VisualElement parent, string text)
        {
            var btn = new Button { text = text };

            btn.clicked += () =>
            {
                switch (text)
                {
                    case "Start Game":          OnStartGameClicked();           break;
                    case "Enter Harbour":       OnEnterHarbourClicked();        break;
                    case "Load Combat Scene":   OnLoadCombatSceneClicked();     break;   // ← NEW
                    case "Close Game":          OnCloseGameClicked();           break;
                }
            };

            btn.style.width = Length.Percent(80);
            btn.style.height = 60;
            btn.style.marginBottom = 20;
            btn.style.fontSize = 24;
            btn.style.unityFontStyleAndWeight = FontStyle.Bold;
            btn.style.backgroundColor = new StyleColor(new Color(1f, 0.43f, 0.13f));
            btn.style.color = Color.white;
            btn.style.borderTopLeftRadius     = 12;
            btn.style.borderTopRightRadius    = 12;
            btn.style.borderBottomLeftRadius  = 12;
            btn.style.borderBottomRightRadius = 12;
            btn.style.borderTopWidth    = 0;
            btn.style.borderRightWidth  = 0;
            btn.style.borderBottomWidth = 0;
            btn.style.borderLeftWidth   = 0;

            btn.RegisterCallback<PointerEnterEvent>(evt => btn.style.backgroundColor = new StyleColor(new Color(1f, 0.55f, 0.25f)));
            btn.RegisterCallback<PointerLeaveEvent>(evt => btn.style.backgroundColor = new StyleColor(new Color(1f, 0.43f, 0.13f)));
            btn.RegisterCallback<PointerDownEvent>(evt => btn.style.scale = new StyleScale(new Scale(new Vector2(0.95f, 0.95f))));
            btn.RegisterCallback<PointerUpEvent>(evt => btn.style.scale = new StyleScale(new Scale(new Vector2(1f, 1f))));

            parent.Add(btn);
        }

        private void OnStartGameClicked()
        {
            if (IMGUILoadingOverlay.Instance != null)
                IMGUILoadingOverlay.Instance.TriggerLoadingScreen();

            if (SceneLoader.Instance != null)
                _ = SceneLoader.Instance.ToggleNextSceneGroup();
            else
                Debug.LogError("SceneLoader.Instance is null");
        }
        

        /// <summary>
        /// NEW: Load Combat Scene button handler
        /// Put your combat scene loading logic here
        /// </summary>
        private void OnLoadCombatSceneClicked()
        {
            Debug.Log("Load Combat Scene button clicked - ready for logic!");

            if (IMGUILoadingOverlay.Instance != null)
                IMGUILoadingOverlay.Instance.TriggerLoadingScreen();

            // TODO: Add your combat scene loading logic here
            //Example:
            if (SceneLoader.Instance != null)
            {
                _ = SceneLoader.Instance.BeginSceneTransition(combatSceneIndex);    
            }
            
        }

        private void OnCloseGameClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}