using Unity.Cinemachine;
using UnityEngine;

namespace _Project.Scripts.SceneManagement
{
    [DefaultExecutionOrder(10000)]
    public class GameplayView : MonoBehaviour
    {
        public static GameplayView Instance { get; private set; }

        public Transform Driver { get; private set; }

        private CinemachineBrain _brain;
        private Camera _view;

        private void Awake()
        {
            Instance = this;
            _view = GetComponent<Camera>();
            _brain = GetComponent<CinemachineBrain>();
        }

        public void Follow(Transform driver)
        {
            Driver = driver;
            if (_brain != null)
                _brain.enabled = driver == null;
        }

        private void LateUpdate()
        {
            if (Driver == null || _view == null)
                return;

            var lens = Driver.GetComponentInChildren<Camera>(true);
            if (lens != null && lens.transform != Driver)
            {
                transform.SetPositionAndRotation(
                    Driver.TransformPoint(lens.transform.localPosition),
                    Driver.rotation * lens.transform.localRotation);

                _view.orthographic = lens.orthographic;
                if (lens.orthographic)
                    _view.orthographicSize = lens.orthographicSize;
                else
                    _view.fieldOfView = lens.fieldOfView;
                return;
            }

            transform.SetPositionAndRotation(Driver.position, Driver.rotation);
        }
    }
}