using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace CombatScenes
{
    public class CombatSceneManager : MonoBehaviour
    {
        
        [Header("Config")]
        [SerializeField] private float countdownDuration = 5f; // 5 seconds

        // States (teaching: prevents bugs by checking state everywhere)
        public enum CombatState { PreCombat, Active, Victory, Defeat }
        public CombatState CurrentState { get; private set; } = CombatState.PreCombat;

        // Event for enemies to listen (loose coupling — scalable!)
        public static event Action OnCombatStarted;

        private Sequence countdownSequence;

        private void Awake()
        {
            // Singleton for this scene (dies with scene unload)
            if (FindObjectOfType<CombatSceneManager>() != this)
            {
                Destroy(gameObject);
                return;
            }

            // Hide canvas initially
            //if (countdownCanvas != null) countdownCanvas.gameObject.SetActive(false);
        }

        private async void Start()
        {
            // ───── START COUNTDOWN IMMEDIATELY ON SCENE LOAD ─────
            await StartCountdown();
        }

        [Header("World-Space Countdown Text (NO CANVAS)")]
        [SerializeField] private TextMeshPro worldCountdownText;   // ← Drag your TMP object here
        [SerializeField] private Transform textParent;             // Optional: parent for scaling animation

        private async System.Threading.Tasks.Task StartCountdown()
        {
            if (worldCountdownText == null)
            {
                Debug.LogError("World Countdown Text not assigned!", this);
                return;
            }

            CurrentState = CombatState.PreCombat;
            worldCountdownText.gameObject.SetActive(true);
            worldCountdownText.text = "5";

            // Optional: big bounce animation on the text itself
            Transform target = textParent != null ? textParent : worldCountdownText.transform;
            target.localScale = Vector3.one * 0.8f;

            for (int i = (int)countdownDuration; i >= 1; i--)
            {
                worldCountdownText.text = i.ToString();
                target.DOScale(1.3f, 0.4f).SetEase(Ease.OutBounce)
                    .OnComplete(() => target.DOScale(1f, 0.2f));
                await System.Threading.Tasks.Task.Delay(1000);
            }

            // Final "FIGHT!"
            worldCountdownText.text = "FIGHT!";
            worldCountdownText.color = Color.red;
            target.DOScale(1.8f, 0.35f).SetEase(Ease.OutExpo)
                .OnComplete(() => target.DOScale(0f, 0.3f));

            await System.Threading.Tasks.Task.Delay(800);

            // Combat starts
            Debug.Log("<color=green>[Combat] FIGHT! Enemies activated</color>");
            CurrentState = CombatState.Active;
            OnCombatStarted?.Invoke();

            // Hide text
            worldCountdownText.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            countdownSequence?.Kill();
            OnCombatStarted = null; // Cleanup
        }
    }
}
