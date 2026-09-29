using UnityEngine;

namespace SwipeClean.Input
{
    public sealed class StrokeKinematics
    {
        private readonly float _smoothingHz;
        private readonly float _slowSpeed;
        private readonly float _fastSpeed;
        private readonly float _maxPressureHoldSeconds;
        private float _smoothedSpeed;
        private double _pointerDownTime;

        public StrokeKinematics(float smoothingHz = 18f, float slowSpeed = 0.05f, float fastSpeed = 1.2f,
            float maxPressureHoldSeconds = 0.8f)
        {
            _smoothingHz = Mathf.Max(0f, smoothingHz);
            _slowSpeed = Mathf.Max(0f, slowSpeed);
            _fastSpeed = Mathf.Max(_slowSpeed + 0.0001f, fastSpeed);
            _maxPressureHoldSeconds = Mathf.Max(0.01f, maxPressureHoldSeconds);
        }

        public void Begin(double timeSeconds)
        {
            _pointerDownTime = timeSeconds;
            _smoothedSpeed = 0f;
        }

        public StrokeSample Evaluate(Vector2 previousUv, Vector2 currentUv, double previousTimeSeconds,
            double currentTimeSeconds, bool isFirst, bool isLast)
        {
            var delta = currentUv - previousUv;
            var deltaTime = Mathf.Max((float)(currentTimeSeconds - previousTimeSeconds), 1f / 240f);
            var rawSpeed = delta.magnitude / deltaTime;
            var blend = 1f - Mathf.Exp(-_smoothingHz * deltaTime);
            _smoothedSpeed = Mathf.Lerp(_smoothedSpeed, rawSpeed, blend);
            var speed01 = Mathf.InverseLerp(_slowSpeed, _fastSpeed, _smoothedSpeed);
            var pressure01 = Mathf.Clamp01((float)(currentTimeSeconds - _pointerDownTime) / _maxPressureHoldSeconds);

            return new StrokeSample(currentUv, delta, _smoothedSpeed, speed01, pressure01,
                currentTimeSeconds, isFirst, isLast);
        }
    }
}

