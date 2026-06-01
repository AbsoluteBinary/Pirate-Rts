using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using _Project.Scripts.Combat;

namespace _Project.Scripts.UI.Combat
{
    /// <summary>
    /// Builds and manages the Combat HUD using UI Toolkit (programmatic).
    /// This replaces the old IMGUI code.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class ShipHUDController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Health playerHealth;

        private UIDocument uiDocument;
        private VisualElement root;

        // UI Elements
        private ProgressBar healthBar;
        private Label healthLabel;
        private Label playerNameLabel;
        private Label enemyNameLabel;

        // Resource labels
        private readonly Dictionary<string, Label> playerResources = new();
        private readonly Dictionary<string, Label> enemyResources = new();

        private Button leaveBattleButton;

        private void Awake()
        {
            uiDocument = GetComponent<UIDocument>();
            root = uiDocument.rootVisualElement;

            if (playerHealth == null)
                playerHealth = FindFirstObjectByType<Health>();

            BuildHUD();
            BindHealthEvents();
        }

        private void OnDestroy()
        {
            if (playerHealth != null)
                playerHealth.OnHealthChanged.RemoveListener(UpdateHealthUI);
        }

        #region HUD Construction

        private void BuildHUD()
        {
            root.Clear();

            root.style.flexDirection = FlexDirection.Column;
            root.style.flexGrow = 1;
            root.style.paddingTop = 0;
            root.style.marginTop = 0;

            // ============================================
            // TOP HUD (Absolutely anchored)
            // ============================================
            var topHUD = new VisualElement();
            topHUD.style.position = Position.Absolute;
            topHUD.style.top = 0;
            topHUD.style.left = 0;
            topHUD.style.right = 0;
            topHUD.style.flexDirection = FlexDirection.Column;
            topHUD.style.paddingTop = 0;
            topHUD.style.marginTop = 0;

            // --- Profile Row ---
            var profileRow = new VisualElement();
            profileRow.style.flexDirection = FlexDirection.Row;
            profileRow.style.justifyContent = Justify.SpaceBetween;
            profileRow.style.alignItems = Align.FlexStart;
            profileRow.style.marginLeft = 15;
            profileRow.style.marginRight = 15;
            profileRow.style.marginTop = 6;
            profileRow.style.paddingTop = 0;

            var enemyProfile = CreateProfileSection("Enemy", true);
            var playerProfile = CreateProfileSection("Player", false);

            profileRow.Add(enemyProfile);
            profileRow.Add(playerProfile);
            topHUD.Add(profileRow);

            // --- Health Bar (Reliable Horizontal Centering) ---
            var healthSection = CreateCenterHealthBar();

            healthSection.style.position = Position.Absolute;

            // === Reliable Centering ===
            healthSection.style.left = Length.Percent(50);
            healthSection.style.translate = new Translate(Length.Percent(-50), 0);

            // Vertical position (adjust as needed)
            healthSection.style.top = 2;

            // Make sure these are NOT set (they can break centering)
            healthSection.style.right = StyleKeyword.Null;   

            topHUD.Add(healthSection);
            

            root.Add(topHUD);

            // Spacer
            var spacer = new VisualElement();
            spacer.style.flexGrow = 1;
            root.Add(spacer);

            // Bottom Resources
            var bottomRow = new VisualElement();
            bottomRow.style.flexDirection = FlexDirection.Row;
            bottomRow.style.justifyContent = Justify.SpaceBetween;
            bottomRow.style.alignItems = Align.FlexEnd;
            bottomRow.style.marginLeft = 15;
            bottomRow.style.marginRight = 15;
            bottomRow.style.marginBottom = 15;

            var enemyResources = CreateResourcePanel("Enemy", true);
            var playerResources = CreateResourcePanel("Player", false);

            bottomRow.Add(enemyResources);
            bottomRow.Add(playerResources);
            root.Add(bottomRow);
        }

        private VisualElement CreateTopRow()
        {
            var topRow = new VisualElement();
            topRow.style.flexDirection = FlexDirection.Row;
            topRow.style.justifyContent = Justify.SpaceBetween;
            topRow.style.marginBottom = 10;

            // Enemy Profile (Top Left)
            var enemyProfile = CreateProfileSection("Enemy", true);
            topRow.Add(enemyProfile);

            // Player Profile (Top Right)
            var playerProfile = CreateProfileSection("Player", false);
            topRow.Add(playerProfile);

            return topRow;
        }

        private VisualElement CreateProfileSection(string side, bool isEnemy)
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.alignItems = Align.Center;

            // Hexagon-style frame (placeholder)
            var frame = new VisualElement();
            frame.style.width = 70;
            frame.style.height = 70;
            frame.style.backgroundColor = isEnemy ? new Color(0.6f, 0.2f, 0.2f) : new Color(0.2f, 0.4f, 0.7f);
            frame.style.borderTopLeftRadius = 35;
            frame.style.borderTopRightRadius = 35;
            frame.style.borderBottomLeftRadius = 35;
            frame.style.borderBottomRightRadius = 35;
            frame.style.borderLeftWidth = 3;
            frame.style.borderRightWidth = 3;
            frame.style.borderTopWidth = 3;
            frame.style.borderBottomWidth = 3;
            frame.style.borderLeftColor = Color.white;
            frame.style.borderRightColor = Color.white;
            frame.style.borderTopColor = Color.white;
            frame.style.borderBottomColor = Color.white;

            // Portrait placeholder
            var portrait = new VisualElement();
            portrait.style.width = 60;
            portrait.style.height = 60;
            portrait.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f);
            portrait.style.borderTopLeftRadius = 30;
            portrait.style.borderTopRightRadius = 30;
            portrait.style.borderBottomLeftRadius = 30;
            portrait.style.borderBottomRightRadius = 30;
            frame.Add(portrait);

            // Name Label
            var nameLabel = new Label(side);
            nameLabel.style.fontSize = 16;
            nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            nameLabel.style.marginLeft = 8;
            nameLabel.style.color = Color.white;

            if (!isEnemy)
                playerNameLabel = nameLabel;
            else
                enemyNameLabel = nameLabel;

            container.Add(frame);
            container.Add(nameLabel);

            return container;
        }

        private VisualElement CreateCenterHealthBar()
        {
            var container = new VisualElement();

            // Set explicit width here (more reliable than on child)
            container.style.width = Length.Percent(35);     // Try 30, 35, or 40

            healthBar = new ProgressBar();
            healthBar.style.width = Length.Percent(100);
            healthBar.highValue = 100;

            healthLabel = new Label("0 / 100");
            healthLabel.style.fontSize = 13;
            healthLabel.style.color = Color.white;
            healthLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            healthLabel.style.marginTop = 3;

            container.Add(healthBar);
            container.Add(healthLabel);

            return container;
        }

        private VisualElement CreateBottomRow()
        {
            var bottomRow = new VisualElement();
            bottomRow.style.flexDirection = FlexDirection.Row;
            bottomRow.style.justifyContent = Justify.SpaceBetween;

            var enemyResources = CreateResourcePanel("Enemy", true);
            var playerResources = CreateResourcePanel("Player", false);

            bottomRow.Add(enemyResources);
            bottomRow.Add(playerResources);

            return bottomRow;
        }

        private VisualElement CreateResourcePanel(string owner, bool isEnemy)
        {
            var panel = new VisualElement();
            panel.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.85f);
            panel.style.paddingLeft = 12;
            panel.style.paddingRight = 12;
            panel.style.paddingTop = 8;
            panel.style.paddingBottom = 8;
            panel.style.borderTopLeftRadius = 6;
            panel.style.borderTopRightRadius = 6;
            panel.style.borderBottomLeftRadius = 6;
            panel.style.borderBottomRightRadius = 6;

            var title = new Label($"{owner} Resources");
            title.style.fontSize = 13;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.marginBottom = 4;
            panel.Add(title);

            string[] resources = { "Iron", "Metal", "Energy", "Exotics" };

            foreach (var res in resources)
            {
                var row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.style.justifyContent = Justify.SpaceBetween;
                row.style.width = 160;

                var label = new Label(res);
                var value = new Label("0");

                value.style.unityTextAlign = TextAnchor.MiddleRight;

                if (isEnemy)
                    enemyResources[res] = value;
                else
                    playerResources[res] = value;

                row.Add(label);
                row.Add(value);
                panel.Add(row);
            }

            return panel;
        }

        #endregion

        #region Health Binding

        private void BindHealthEvents()
        {
            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged.AddListener(UpdateHealthUI);
                UpdateHealthUI(playerHealth.CurrentHealth);
            }
        }

        private void UpdateHealthUI(float currentHealth)
        {
            if (healthBar == null || healthLabel == null || playerHealth == null) return;

            float max = playerHealth.MaxHealth;
            healthBar.value = (currentHealth / max) * 100f;
            healthLabel.text = $"{Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(max)}";
        }

        #endregion

        #region Future Features

        public void ShowLeaveBattleButton(bool show)
        {
            if (leaveBattleButton == null) return;
            leaveBattleButton.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void OnPlayerDied()
        {
            // TODO: Pause turrets, show defeat screen, etc.
            Debug.Log("[ShipHUDController] Player has died. Combat systems should pause.");
        }

        #endregion
    }
}