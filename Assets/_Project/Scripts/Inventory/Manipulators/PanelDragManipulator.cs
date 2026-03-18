using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace _Project.Scripts.Inventory.Manipulators
{
    public class PanelDragManipulator : PointerManipulator
    {
        private bool isDragging;
        private Vector2 offset;
        private Vector2 currentPosition;  // ← we keep track of the current translate ourselves

        public PanelDragManipulator()
        {
            activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerDownEvent>(OnPointerDown);
            target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            target.RegisterCallback<PointerUpEvent>(OnPointerUp);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
            target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
        }

        void OnPointerDown(PointerDownEvent evt)
        {
            if (!CanStartManipulation(evt) || isDragging) return;

            // Read current position from style (resolved value)
            currentPosition = target.resolvedStyle.translate;

            // Offset from where the pointer clicked inside the panel
            offset = evt.localPosition;

            isDragging = true;
            target.CapturePointer(evt.pointerId);
            evt.StopPropagation();
        }

        void OnPointerMove(PointerMoveEvent evt)
        {
            
            if (!isDragging || !target.HasPointerCapture(evt.pointerId)) return;

            // Calculate new position
            Vector2 pointerPos = new Vector2(evt.localPosition.x, evt.localPosition.y);
            Vector2 delta = pointerPos - offset;
            Vector2 newPosition = currentPosition + delta;

            // Apply to style.translate
            target.style.translate = new Translate(newPosition.x, newPosition.y);
            
            newPosition.x = Mathf.Clamp(newPosition.x, 0, Screen.width - target.resolvedStyle.width);
            newPosition.y = Mathf.Clamp(newPosition.y, 0, Screen.height - target.resolvedStyle.height);
            
            // In OnPointerUp
            PlayerPrefs.SetFloat("PanelX", newPosition.x);
            PlayerPrefs.SetFloat("PanelY", newPosition.y);

            evt.StopPropagation();
        }

        void OnPointerUp(PointerUpEvent evt)
        {
            if (!CanStopManipulation(evt) || !isDragging) return;

            isDragging = false;
            target.ReleasePointer(evt.pointerId);
            evt.StopPropagation();
        }
    }
}