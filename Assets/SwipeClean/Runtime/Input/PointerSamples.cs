using UnityEngine;

namespace SwipeClean.Input
{
    public enum PointerPhase
    {
        Began,
        Moved,
        Ended,
        Cancelled
    }

    public readonly struct RawPointerSample
    {
        public int PointerId { get; }
        public Vector2 ScreenPositionPx { get; }
        public double TimeSeconds { get; }
        public PointerPhase Phase { get; }

        public RawPointerSample(int pointerId, Vector2 screenPositionPx, double timeSeconds, PointerPhase phase)
        {
            PointerId = pointerId;
            ScreenPositionPx = screenPositionPx;
            TimeSeconds = timeSeconds;
            Phase = phase;
        }
    }

    public readonly struct StrokeSample
    {
        public Vector2 SurfaceUv { get; }
        public Vector2 DeltaUv { get; }
        public float SpeedUvPerSecond { get; }
        public float Speed01 { get; }
        public float Pressure01 { get; }
        public double TimeSeconds { get; }
        public bool IsFirst { get; }
        public bool IsLast { get; }

        public StrokeSample(Vector2 surfaceUv, Vector2 deltaUv, float speedUvPerSecond, float speed01,
            float pressure01, double timeSeconds, bool isFirst, bool isLast)
        {
            SurfaceUv = surfaceUv;
            DeltaUv = deltaUv;
            SpeedUvPerSecond = speedUvPerSecond;
            Speed01 = speed01;
            Pressure01 = pressure01;
            TimeSeconds = timeSeconds;
            IsFirst = isFirst;
            IsLast = isLast;
        }
    }
}

