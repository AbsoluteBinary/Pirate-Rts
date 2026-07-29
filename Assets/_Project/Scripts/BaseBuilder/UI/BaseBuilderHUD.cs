using System;
using _Project.Scripts.BaseBuilder.Runtime.Core;
using _Project.Scripts.BaseBuilder.Runtime.Data;
using UnityEngine;
using UnityEngine.UIElements;

namespace _Project.Scripts.BaseBuilder.UI
{
    public class BaseBuilderHUD : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PanelRenderer panelRenderer;
        [SerializeField] private BaseBuilderController builderController;

        // Events other systems can listen to
        public event Action OnSelectModeClicked;
        public event Action OnBuildOnWaterClicked;
        public event Action OnBuildOnLandClicked;
        public event Action OnExitClicked;
        public event Action OnPickUpClicked;
        public event Action OnDeleteClicked;
        public event Action OnLockClicked;
        public event Action OnUnlockClicked;

        private VisualElement root;
        private VisualElement topBar;
        private VisualElement filterBar;

        private void OnEnable()
        {
            if (panelRenderer == null)
                panelRenderer = GetComponent<PanelRenderer>();

            panelRenderer.RegisterUIReloadCallback(OnUIReady);
        }

        private void OnDisable()
        {
            if (panelRenderer != null)
                panelRenderer.UnregisterUIReloadCallback(OnUIReady);
        }

        private void OnUIReady(PanelRenderer renderer, VisualElement rootElement)
        {
            root = rootElement;
            BuildUI();
        }

        private void BuildUI()
        {
            root.Clear();

            // ===================== TOP BAR =====================
            topBar = new VisualElement();
            topBar.style.position = Position.Absolute;
            topBar.style.top = 0;
            topBar.style.left = 0;
            topBar.style.right = 0;
            topBar.style.height = 56;
            topBar.style.backgroundColor = new Color(0.12f, 0.14f, 0.22f, 0.95f);
            topBar.style.flexDirection = FlexDirection.Row;
            topBar.style.alignItems = Align.Center;
            topBar.style.paddingLeft = 16;
            topBar.style.paddingRight = 16;

            // Mode Buttons
            topBar.Add(CreateModeButton("Select", BuilderMode.Select));
            topBar.Add(CreateModeButton("Build on Water", BuilderMode.BuildOnWater));
            topBar.Add(CreateModeButton("Build on Land", BuilderMode.BuildOnLand));

            // Spacer
            var spacer = new VisualElement();
            spacer.style.flexGrow = 1;
            topBar.Add(spacer);

            // Action Buttons
            topBar.Add(CreateActionButton("Pick Up", () => OnPickUpClicked?.Invoke()));
            topBar.Add(CreateActionButton("Delete", () => OnDeleteClicked?.Invoke()));
            topBar.Add(CreateActionButton("Lock", () => OnLockClicked?.Invoke()));
            topBar.Add(CreateActionButton("Unlock", () => OnUnlockClicked?.Invoke()));

            // Exit Button
            var exitBtn = new Button { text = "Exit Build Mode" };
            StyleButton(exitBtn, new Color(0.55f, 0.15f, 0.15f));
            exitBtn.clicked += () => OnExitClicked?.Invoke();
            topBar.Add(exitBtn);

            root.Add(topBar);

            // ===================== FILTER BAR =====================
            filterBar = new VisualElement();
            filterBar.style.position = Position.Absolute;
            filterBar.style.top = 56;
            filterBar.style.left = 0;
            filterBar.style.right = 0;
            filterBar.style.height = 42;
            filterBar.style.backgroundColor = new Color(0.10f, 0.12f, 0.18f, 0.92f);
            filterBar.style.flexDirection = FlexDirection.Row;
            filterBar.style.alignItems = Align.Center;
            filterBar.style.paddingLeft = 16;

            filterBar.Add(new Label("Filter:")
            {
                style =
                {
                    color = Color.white,
                    marginRight = 12,
                    unityFontStyleAndWeight = FontStyle.Bold
                }
            });

            filterBar.Add(CreateFilterButton("All", SelectFilter.All));
            filterBar.Add(CreateFilterButton("Walls", SelectFilter.Walls));
            filterBar.Add(CreateFilterButton("Turrets", SelectFilter.Turrets));
            filterBar.Add(CreateFilterButton("Land", SelectFilter.Land));

            root.Add(filterBar);
        }

        // ==================== BUTTON HELPERS ====================

        private Button CreateModeButton(string text, BuilderMode mode)
        {
            var btn = new Button { text = text };
            StyleModeButton(btn);

            btn.clicked += () =>
            {
                builderController?.SetMode(mode);
                // Raise specific events if needed
                switch (mode)
                {
                    case BuilderMode.Select: OnSelectModeClicked?.Invoke(); break;
                    case BuilderMode.BuildOnWater: OnBuildOnWaterClicked?.Invoke(); break;
                    case BuilderMode.BuildOnLand: OnBuildOnLandClicked?.Invoke(); break;
                }
            };

            return btn;
        }

        private Button CreateFilterButton(string text, SelectFilter filter)
        {
            var btn = new Button { text = text };
            StyleFilterButton(btn);

            btn.clicked += () => builderController?.SetSelectFilter(filter);
            return btn;
        }

        private Button CreateActionButton(string text, Action onClick)
        {
            var btn = new Button { text = text };
            StyleActionButton(btn);
            btn.clicked += onClick;
            return btn;
        }
        
        private void StyleButton(Button btn, Color backgroundColor)
        {
            btn.style.height = 40;
            btn.style.paddingLeft = 16;
            btn.style.paddingRight = 16;
            btn.style.backgroundColor = backgroundColor;
            btn.style.color = Color.white;
            btn.style.borderTopLeftRadius = 6;
            btn.style.borderTopRightRadius = 6;
            btn.style.borderBottomLeftRadius = 6;
            btn.style.borderBottomRightRadius = 6;
        }

        // ==================== STYLING HELPERS ====================

        private void StyleModeButton(Button btn)
        {
            btn.style.height = 40;
            btn.style.marginRight = 8;
            btn.style.paddingLeft = 14;
            btn.style.paddingRight = 14;
            btn.style.backgroundColor = new Color(0.20f, 0.25f, 0.40f);
            btn.style.color = Color.white;
            btn.style.borderTopLeftRadius = 6;
            btn.style.borderTopRightRadius = 6;
            btn.style.borderBottomLeftRadius = 6;
            btn.style.borderBottomRightRadius = 6;
        }

        private void StyleFilterButton(Button btn)
        {
            btn.style.height = 32;
            btn.style.marginRight = 6;
            btn.style.paddingLeft = 12;
            btn.style.paddingRight = 12;
            btn.style.backgroundColor = new Color(0.18f, 0.22f, 0.35f);
            btn.style.color = Color.white;
            btn.style.borderTopLeftRadius = 4;
            btn.style.borderTopRightRadius = 4;
            btn.style.borderBottomLeftRadius = 4;
            btn.style.borderBottomRightRadius = 4;
        }

        private void StyleActionButton(Button btn)
        {
            btn.style.height = 40;
            btn.style.marginRight = 8;
            btn.style.paddingLeft = 14;
            btn.style.paddingRight = 14;
            btn.style.backgroundColor = new Color(0.25f, 0.30f, 0.45f);
            btn.style.color = Color.white;
            btn.style.borderTopLeftRadius = 6;
            btn.style.borderTopRightRadius = 6;
            btn.style.borderBottomLeftRadius = 6;
            btn.style.borderBottomRightRadius = 6;
        }
    }
}