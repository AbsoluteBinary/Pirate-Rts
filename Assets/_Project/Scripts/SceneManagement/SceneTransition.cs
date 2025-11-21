using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.SceneManagement
{
    public class SceneTransition : MonoBehaviour
    {
        public static SceneTransition Instance { get; private set; }

        [SerializeField] private Image fadeImage;       // Drag a full-screen black Image here
        [SerializeField] private float fadeDuration = 0.4f; // Smooth but fast

        private void Awake()
        {
            // Singleton + persist across scenes
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            // Start fully transparent
            if (fadeImage != null)
            {
                fadeImage.color = new Color(0, 0, 0, 0);
                fadeImage.raycastTarget = false; // Don't block clicks
            }
        }

        /// <summary>
        /// Fades to black → runs your action → fades back in
        /// </summary>
        public void PerformTransition(Action transitionAction)
        {
            // Kill any existing fade
            DOTween.Kill(fadeImage);

            // Fade to black
            fadeImage.DOFade(1f, fadeDuration).SetEase(Ease.InOutSine).OnComplete(() =>
            {
                // Run the load/unload while screen is black
                transitionAction?.Invoke();

                // Fade back in
                fadeImage.DOFade(0f, fadeDuration).SetEase(Ease.InOutSine);
            });
        }
    }
}
