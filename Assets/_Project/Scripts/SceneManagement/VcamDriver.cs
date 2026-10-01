using UnityEngine;

namespace _Project.Scripts.SceneManagement
{
    public class VcamDriver : MonoBehaviour
    {
        [SerializeField] private VcamChannel channel;

        private void OnEnable()
        {
            Claim();
        }

        private void Start()
        {
            Claim();
        }

        public void Claim()
        {
            var target = transform.parent != null ? transform.parent : transform;
            FindAnyObjectByType<RigDirector>()?.SetFollow(channel, target);
        }
    }
}