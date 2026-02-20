using System.Collections.Generic;
using System.IO;
using DG.Tweening;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace UI.Manager
{
    public class UIManager : SerializedMonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [System.Serializable]
        public class PanelState
        {
            public string panelName;      // e.g. "LoginPanel"
            public bool isVisible = true;
        }

        [OdinSerialize, ShowInInspector]
        private List<PanelState> panelStates = new();  // Simple list for now (no per-scene split yet)

        private string savePath => Application.persistentDataPath + "/ui_state.dat";
        private EventSystem eventSystem;

        private void Awake()
        {
            eventSystem = FindObjectOfType<EventSystem>();
            
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadState();
        }
        
        public void BlockInputDuringTransition(bool block)
        {
            if (eventSystem != null)
            {
                eventSystem.gameObject.SetActive(!block);
                Debug.Log(block ? "Input blocked" : "Input unblocked");
            }
        }
        
        public void ShowBootLoginPanel()
        {
            var doc = GameObject.Find("LoginScreen")?.GetComponent<UIDocument>();
            if (doc == null || doc.rootVisualElement == null)
            {
                Debug.LogError("LoginScreen UIDocument not found or root null");
                return;
            }

            var panel = doc.rootVisualElement.Q<VisualElement>("LoginPanel");
            if (panel == null)
            {
                Debug.LogError("LoginPanel VisualElement not found");
                return;
            }

            // Force starting state (overrides any UXML default)
            panel.style.display = DisplayStyle.Flex;   // must be Flex for fade to work
            panel.style.opacity = 0f;                  // start invisible

            // Fade in
            DOTween.To(() => panel.resolvedStyle.opacity,
                    x => panel.style.opacity = x,
                    1f, 0.8f)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true)
                .OnComplete(() => Debug.Log("LoginPanel fade-in complete"));

            Debug.Log("[UIManager] LoginPanel activated & fading in from hidden");
        }
        
        public void HideBootLoginPanel()
        {
            var doc = GameObject.Find("LoginScreen")?.GetComponent<UIDocument>();
            if (doc == null || doc.rootVisualElement == null) return;

            var panel = doc.rootVisualElement.Q<VisualElement>("LoginPanel");
            if (panel == null) return;

            DOTween.To(() => panel.resolvedStyle.opacity,
                    x => panel.style.opacity = x,
                    0f, 0.5f)
                .SetEase(Ease.InQuad)
                .OnComplete(() =>
                {
                    panel.style.display = DisplayStyle.None;
                    Debug.Log("Login panel hidden");
                });
        }

        // Public toggle (use this from buttons / logic)
        public void SetPanelVisible(string panelName, bool visible, bool isGlobal = false)
        {
            var existing = panelStates.Find(p => p.panelName == panelName);
            if (existing != null)
            {
                existing.isVisible = visible;
            }
            else
            {
                panelStates.Add(new PanelState { panelName = panelName, isVisible = visible });
            }

            SaveState();
            ApplyAllStates();  // Apply immediately (safe in this version)
        }

        public bool GetPanelVisible(string panelName)
        {
            var panel = panelStates.Find(p => p.panelName == panelName);
            return panel?.isVisible ?? true;  // Default visible if never saved
        }

        // Apply visibility to all matching panels across loaded scenes
        private void ApplyAllStates()
        {
            foreach (var doc in FindObjectsOfType<UIDocument>())
            {
                if (doc.rootVisualElement == null) continue;

                foreach (var state in panelStates)
                {
                    var element = doc.rootVisualElement.Q<VisualElement>(state.panelName);
                    if (element != null)
                    {
                        element.style.display = state.isVisible ? DisplayStyle.Flex : DisplayStyle.None;
                        Debug.Log($"Applied visibility to '{state.panelName}' in UIDocument '{doc.gameObject.name}'");
                    }
                }
            }
        }

        private void SaveState()
        {
            byte[] bytes = SerializationUtility.SerializeValue(panelStates, DataFormat.Binary);
            File.WriteAllBytes(savePath, bytes);
            Debug.Log($"UI state saved: {savePath}");
        }

        private void LoadState()
        {
            if (File.Exists(savePath))
            {
                byte[] bytes = File.ReadAllBytes(savePath);
                panelStates = SerializationUtility.DeserializeValue<List<PanelState>>(bytes, DataFormat.Binary) ?? new();
                Debug.Log("UI state loaded");
            }
            else
            {
                Debug.Log("No UI state file – starting fresh");
            }
        }

        // Test buttons (remove later)
        [Button("Test Hide LoginPanel")]
        private void TestHide() => SetPanelVisible("LoginPanel", false);

        [Button("Test Show LoginPanel")]
        private void TestShow() => SetPanelVisible("LoginPanel", true);
    }
}
