using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;


namespace StrategyCamera
{
    public class CameraInputs
    {
        public bool hasTouchScreen { get; private set; }
        public int touchCount { get; private set; }
        public float zoom { get; private set; }
        public Vector2 translate { get; private set; }
        public Vector2 pointerPosition { get; private set; }
        public Vector2 mouseDelta { get; private set; }
        public bool isMiddleMouseButtonDown { get; private set; }
        public bool isPointerHolding { get; private set; }
        public bool isPointerDownThisFrame { get; private set; }
        public bool isPointerReleaseThisFrame { get; private set; }

        private PlayerInput _playerInput;
        private InputAction _iaMousePosition, _iaTranslate, _iaLookAound, _iaClickHold, _iaZoom, _iaMiddleMouseButton;

        public CameraInputs(GameObject controller)
        {
            EnhancedTouchSupport.Enable();

            PlayerInput playerInput = controller.GetComponent<PlayerInput>();
            SetupInputActions(playerInput);
        }

        private void SetupInputActions(PlayerInput playerInput)
        {
            this._playerInput = playerInput;

            _iaMousePosition = _playerInput.actions["MousePosition"];
            _iaLookAound = _playerInput.actions["LookAround"];
            _iaMiddleMouseButton = _playerInput.actions["MiddleMouseButton"];
            _iaZoom = _playerInput.actions["Zoom"];
            _iaTranslate = _playerInput.actions["Translate"];
            _iaClickHold = _playerInput.actions["ClickHold"];

            hasTouchScreen = (Touchscreen.current != null);
        }

        public void UpdateTransientInputs()
        {

            touchCount = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count;
            pointerPosition = _iaMousePosition.ReadValue<Vector2>();
            mouseDelta = _iaLookAound.ReadValue<Vector2>();
            isMiddleMouseButtonDown = _iaMiddleMouseButton.IsPressed();
            zoom = _iaZoom.ReadValue<float>();
            translate = _iaTranslate.ReadValue<Vector2>();
            isPointerHolding = _iaClickHold.IsPressed();
            isPointerDownThisFrame = _iaClickHold.WasPressedThisFrame();
            isPointerReleaseThisFrame = _iaClickHold.WasReleasedThisFrame();
        }

        public void EnableInputs()
        {
            _iaMousePosition.Enable();
            _iaLookAound.Enable();
            _iaMiddleMouseButton.Enable();
            _iaZoom.Enable();
            _iaTranslate.Enable();
            _iaClickHold.Enable();
        }
        public void DisableInputs()
        {
            _iaMousePosition.Disable();
            _iaLookAound.Disable();
            _iaMiddleMouseButton.Disable();
            _iaZoom.Disable();
            _iaTranslate.Disable();
            _iaClickHold.Disable();
        }
    }
}