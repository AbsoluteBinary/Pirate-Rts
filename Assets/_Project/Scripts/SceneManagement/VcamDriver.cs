using Unity.Cinemachine;
using UnityEngine;

namespace _Project.Scripts.SceneManagement
{
    public class VcamDriver : MonoBehaviour
    {
        [SerializeField] private VcamChannel channel;

        private CinemachineCamera _vcam;
        private Camera _lens;

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
            var rig = FindAnyObjectByType<RigDirector>();
            if (rig == null)
                return;

            var target = transform.parent != null ? transform.parent : transform;
            rig.SetFollow(channel, target);
            _vcam = rig.CameraFor(channel);
            _lens = GetComponent<Camera>();
        }

        private void LateUpdate()
        {
            if (_vcam == null || _lens == null)
                return;

            var lens = _vcam.Lens;
            lens.FieldOfView = _lens.fieldOfView;
            lens.OrthographicSize = _lens.orthographicSize;
            lens.ModeOverride = _lens.orthographic
                ? LensSettings.OverrideModes.Orthographic
                : LensSettings.OverrideModes.Perspective;
            _vcam.Lens = lens;
        }
    }
}