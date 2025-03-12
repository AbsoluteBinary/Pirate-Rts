
using UnityEngine;

namespace StrategyCamera
{
    public class SelectorComponent : MonoBehaviour
    {
        public SelectionProperties selectionProperties;
        public float clickTime = 0.2f;
        public ISelectable selectedInterface
        {
            get;
            private set;
        }

        private CameraInputs xinputs
        {
            get { return controller.xinputs; }
        }

        private StrategyCameraController controller;
        private float pointerSDownTime, pointerUpTime;
        void Start()
        {
            this.controller = GetComponent<StrategyCameraController>();
        }

        private void Update()
        {
            if (xinputs.isPointerDownThisFrame)
            {
                SelectionStart();
            }

            if (xinputs.isPointerReleaseThisFrame)
            {
                SelectionEnd();
                if ((pointerUpTime - pointerSDownTime) < clickTime)
                {
                    if (selectedInterface != null)
                    {
                        selectedInterface.Deselect();
                        selectedInterface = null;
                    }
                    var selected = SelectUnderClick();
                    if (selected.Item1 != null)
                    {
                        selectedInterface = selected.Item1;

                        selectedInterface.Select(selectionProperties);
                    }

                }

            }
        }

        private void SelectionStart()
        {
            pointerSDownTime = Time.time;
        }

        private void SelectionEnd()
        {
            pointerUpTime = Time.time;
        }

        (ISelectable, GameObject) SelectUnderClick()
        {
            Ray ray = controller.camera.ScreenPointToRay(xinputs.pointerPosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                var allMono = hit.collider.gameObject.GetComponents<MonoBehaviour>();

                foreach (var monoBehaviour in allMono)
                {
                    var selectable = monoBehaviour as ISelectable;

                    if (selectable != null)
                    {
                        return (selectable, monoBehaviour.gameObject);
                    }
                }
            }
            return (null, null);
        }
    }

    [System.Serializable]
    public struct SelectionProperties
    {
        public Color color;
    }
}