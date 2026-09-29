using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace SwipeClean.Input
{
    /// <summary>Input System adapter. Callbacks only enqueue immutable raw samples.</summary>
    public sealed class GameplayInputDriver : MonoBehaviour
    {
        private const int PointerId = 0;
        private InputAction _pointAction;
        private InputAction _contactAction;
        private bool _contactActive;

        public event Action<RawPointerSample> SampleReceived;

        private void Awake()
        {
            _pointAction = new InputAction("Point", InputActionType.PassThrough, "<Pointer>/position");
            _contactAction = new InputAction("PrimaryContact", InputActionType.Button, "<Pointer>/press");
            _pointAction.performed += OnPoint;
            _contactAction.performed += OnContactPerformed;
            _contactAction.canceled += OnContactCancelled;
        }

        private void OnEnable()
        {
            _pointAction?.Enable();
            _contactAction?.Enable();
        }

        private void OnDisable()
        {
            if (_contactActive)
            {
                Emit(PointerPhase.Cancelled, InputState.currentTime);
            }

            _contactActive = false;
            _pointAction?.Disable();
            _contactAction?.Disable();
        }

        private void OnDestroy()
        {
            if (_pointAction != null)
            {
                _pointAction.performed -= OnPoint;
                _pointAction.Dispose();
            }

            if (_contactAction != null)
            {
                _contactAction.performed -= OnContactPerformed;
                _contactAction.canceled -= OnContactCancelled;
                _contactAction.Dispose();
            }
        }

        private void OnPoint(InputAction.CallbackContext context)
        {
            if (_contactActive)
            {
                Emit(PointerPhase.Moved, context.time);
            }
        }

        private void OnContactPerformed(InputAction.CallbackContext context)
        {
            if (_contactActive)
            {
                return;
            }

            _contactActive = true;
            Emit(PointerPhase.Began, context.time);
        }

        private void OnContactCancelled(InputAction.CallbackContext context)
        {
            if (!_contactActive)
            {
                return;
            }

            Emit(PointerPhase.Ended, context.time);
            _contactActive = false;
        }

        private void Emit(PointerPhase phase, double timeSeconds)
        {
            var position = _pointAction?.ReadValue<Vector2>() ?? Vector2.zero;
            SampleReceived?.Invoke(new RawPointerSample(PointerId, position, timeSeconds, phase));
        }
    }
}
