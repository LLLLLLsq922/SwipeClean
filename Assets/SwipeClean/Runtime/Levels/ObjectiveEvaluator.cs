using System;

namespace SwipeClean.Levels
{
    public enum ObjectiveSignal
    {
        None,
        InspectSuggested,
        Completed
    }

    /// <summary>Applies inspection threshold, hysteresis and two-sample completion debounce.</summary>
    public sealed class ObjectiveEvaluator
    {
        private readonly float _inspectThreshold01;
        private readonly float _completionThreshold01;
        private readonly float _hysteresis01;
        private int _consecutivePasses;
        private bool _inspectSuggested;
        private bool _completed;

        public ObjectiveEvaluator(float inspectThreshold01 = 0.90f, float completionThreshold01 = 0.985f,
            float hysteresis01 = 0.002f)
        {
            _inspectThreshold01 = Clamp01(inspectThreshold01);
            _completionThreshold01 = Math.Max(_inspectThreshold01, Clamp01(completionThreshold01));
            _hysteresis01 = Math.Max(0f, hysteresis01);
        }

        public ObjectiveSignal Evaluate(float cleanliness01)
        {
            if (_completed)
            {
                return ObjectiveSignal.None;
            }

            cleanliness01 = Clamp01(cleanliness01);
            if (cleanliness01 >= _completionThreshold01)
            {
                _consecutivePasses++;
            }
            else if (cleanliness01 < _completionThreshold01 - _hysteresis01)
            {
                _consecutivePasses = 0;
            }

            if (_consecutivePasses >= 2)
            {
                _completed = true;
                return ObjectiveSignal.Completed;
            }

            if (!_inspectSuggested && cleanliness01 >= _inspectThreshold01)
            {
                _inspectSuggested = true;
                return ObjectiveSignal.InspectSuggested;
            }

            return ObjectiveSignal.None;
        }

        public void Reset()
        {
            _consecutivePasses = 0;
            _inspectSuggested = false;
            _completed = false;
        }

        private static float Clamp01(float value) => value < 0f ? 0f : value > 1f ? 1f : value;
    }
}

