using UnityEngine;

namespace _Project.Scripts.SceneManagement
{
    public class ViewDriver : MonoBehaviour
    {
        private void OnEnable()
        {
            FollowView();
        }

        private void Start()
        {
            FollowView();
        }

        private void FollowView()
        {
            GameplayView.Instance?.Follow(transform);
        }

        private void OnDisable()
        {
            if (GameplayView.Instance != null && GameplayView.Instance.Driver == transform)
                GameplayView.Instance.Follow(null);
        }
    }
}