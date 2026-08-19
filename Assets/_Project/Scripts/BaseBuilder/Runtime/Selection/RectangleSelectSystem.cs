using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Scripts.BaseBuilder.Runtime.Selection
{
    public class RectangleSelectSystem
    {
        public enum State { Idle, Dragging }
        public State CurrentState { get; private set; } = State.Idle;

        public Vector2 StartScreen { get; private set; }
        public Vector2 CurrentScreen { get; private set; }

        public bool IsDragging => CurrentState == State.Dragging;

        public event System.Action OnDragStarted;
        public event System.Action OnDragUpdated;
        public event System.Action OnDragEnded; // later: pass rect / selection

        public void Tick()
        {
            bool ctrl = Keyboard.current != null && Keyboard.current.leftCtrlKey.isPressed;
            bool lmbDown = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
            bool lmbHeld = Mouse.current != null && Mouse.current.leftButton.isPressed;
            bool lmbUp   = Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame;

            Vector2 mouse = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;

            if (CurrentState == State.Idle)
            {
                if (ctrl && lmbDown)
                {
                    StartScreen = mouse;
                    CurrentScreen = mouse;
                    CurrentState = State.Dragging;
                    OnDragStarted?.Invoke();
                }
                return;
            }

            // Dragging
            if (lmbHeld)
            {
                CurrentScreen = mouse;
                OnDragUpdated?.Invoke();
            }

            if (lmbUp || !ctrl) // release or lose Ctrl → end
            {
                CurrentState = State.Idle;
                OnDragEnded?.Invoke();
            }
        }

        public void Cancel()
        {
            if (CurrentState == State.Idle) return;
            CurrentState = State.Idle;
            OnDragEnded?.Invoke();
        }

        /// <summary>Normalized rect in screen pixels (xMin, yMin, width, height). Y is bottom-left origin (Input System).</summary>
        public Rect GetScreenRect()
        {
            float xMin = Mathf.Min(StartScreen.x, CurrentScreen.x);
            float xMax = Mathf.Max(StartScreen.x, CurrentScreen.x);
            float yMin = Mathf.Min(StartScreen.y, CurrentScreen.y);
            float yMax = Mathf.Max(StartScreen.y, CurrentScreen.y);
            return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
        }
    }
}