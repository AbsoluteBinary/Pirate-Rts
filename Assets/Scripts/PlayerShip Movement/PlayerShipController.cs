using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace PlayerShip_Movement
{
    public class PlayerShipController : MonoBehaviour
    {
        private Vector3 targetLocation;
        [SerializeField] private DOTweenType doTweenType = DOTweenType.MovementOneWay;
        private Tween FreeRoamTween;
        [SerializeField] private GameObject ship;
        [Range(1.0f, 400.0f)] [SerializeField] private float moveDuration = 1.0f;
        [SerializeField] private GameObject frButton; // Changed from Button to GameObject
        private Vector3 btnPos;
        private Vector3 btnResetPos;

        public ClickBehavior ClickBehavior; // Unused, retained for compatibility
        private Coroutine coroutine;
        public int timeRemaining;

        private enum DOTweenType
        {
            MovementOneWay,
            MovementTwoWay
        }

        private void Start()
        {
            // Validate serialized fields
            Debug.Assert(frButton != null, "FrButton is not assigned!");
            Debug.Assert(ship != null, "Ship is not assigned!");
        }

        public void StartTween()
        {
            // Set target location to the button's position
            targetLocation = frButton.transform.position;

            if (doTweenType == DOTweenType.MovementOneWay)
            {
                Debug.Log("Start Moving");
                // If coroutine is running, stop it
                if (coroutine != null) StopCoroutine(coroutine);
                // Start coroutine
                coroutine = StartCoroutine(PopUpCountdown());

                if (targetLocation == Vector3.zero)
                    targetLocation = transform.position;

                // Movement
                FreeRoamTween = ship.transform.DOLookAt(targetLocation, moveDuration / 2);
                FreeRoamTween = ship.transform.DOMove(targetLocation, moveDuration);
            }
        }

        private IEnumerator PopUpCountdown()
        {
            for (var e = timeRemaining; e > 0; e--)
            {
                yield return new WaitForSeconds(1f);
                if (e == 1)
                {
                    // Move button off-screen and hide it
                    frButton.transform.position = new Vector3(0, -10, 0); // Adjust as needed for UI
                    frButton.SetActive(false); // Hide button
                }
            }

            yield return null;
        }
    }

    // Retained for compatibility, but appears unused
    public enum ClickBehavior
    {
        None,
        // Add other behaviors if needed
    }
}


