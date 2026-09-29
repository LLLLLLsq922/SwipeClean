using System;
using System.Collections.Generic;
using UnityEngine;

namespace SwipeClean.Input
{
    /// <summary>Spatially resamples a stroke into stable UV stamps.</summary>
    public sealed class StrokeInterpolator
    {
        public float MinSpacingUv { get; }
        public int MaxSegmentsPerInput { get; }

        public StrokeInterpolator(float minSpacingUv = 0.002f, int maxSegmentsPerInput = 24)
        {
            MinSpacingUv = Mathf.Max(0.00001f, minSpacingUv);
            MaxSegmentsPerInput = Mathf.Max(1, maxSegmentsPerInput);
        }

        public int Append(Vector2 previousUv, Vector2 currentUv, float activeRadiusUv, List<Vector2> destination)
        {
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            if (!IsFinite(previousUv) || !IsFinite(currentUv))
            {
                return 0;
            }

            var distance = Vector2.Distance(previousUv, currentUv);
            if (distance <= Mathf.Epsilon)
            {
                return 0;
            }

            var spacing = Mathf.Max(Mathf.Abs(activeRadiusUv) * 0.4f, MinSpacingUv);
            var segments = Mathf.Clamp(Mathf.CeilToInt(distance / spacing), 1, MaxSegmentsPerInput);
            for (var i = 1; i <= segments; i++)
            {
                destination.Add(Vector2.LerpUnclamped(previousUv, currentUv, i / (float)segments));
            }

            return segments;
        }

        private static bool IsFinite(Vector2 value) =>
            !float.IsNaN(value.x) && !float.IsInfinity(value.x) &&
            !float.IsNaN(value.y) && !float.IsInfinity(value.y);
    }
}

