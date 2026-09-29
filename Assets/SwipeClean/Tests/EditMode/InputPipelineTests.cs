using System.Collections.Generic;
using NUnit.Framework;
using SwipeClean.Input;
using UnityEngine;

namespace SwipeClean.Tests.EditMode
{
    public sealed class InputPipelineTests
    {
        [Test]
        public void Interpolator_UsesRadiusRelativeSpacing()
        {
            var destination = new List<Vector2>();
            var interpolator = new StrokeInterpolator();

            var count = interpolator.Append(Vector2.zero, new Vector2(0.1f, 0f), 0.05f, destination);

            Assert.That(count, Is.EqualTo(5));
            Assert.That(destination[^1], Is.EqualTo(new Vector2(0.1f, 0f)));
        }

        [Test]
        public void Interpolator_RejectsNonFiniteCoordinates()
        {
            var destination = new List<Vector2>();
            var count = new StrokeInterpolator().Append(Vector2.zero,
                new Vector2(float.NaN, 0f), 0.05f, destination);
            Assert.That(count, Is.Zero);
        }

        [Test]
        public void Kinematics_LongPressPressureIsTimeBased()
        {
            var kinematics = new StrokeKinematics(maxPressureHoldSeconds: 0.8f);
            kinematics.Begin(10d);

            var sample = kinematics.Evaluate(Vector2.zero, new Vector2(0.1f, 0f), 10d, 10.4d, true, false);

            Assert.That(sample.Pressure01, Is.EqualTo(0.5f).Within(0.001f));
            Assert.That(sample.SpeedUvPerSecond, Is.GreaterThan(0f));
        }

        [Test]
        public void PointerTracker_KeepsBlockedTouchBlockedForLifetime()
        {
            var tracker = new PrimaryPointerTracker();
            Assert.That(tracker.TryAccept(new RawPointerSample(1, Vector2.zero, 0d, PointerPhase.Began), true), Is.False);
            Assert.That(tracker.TryAccept(new RawPointerSample(1, Vector2.one, 0.1d, PointerPhase.Moved), false), Is.False);
            Assert.That(tracker.TryAccept(new RawPointerSample(1, Vector2.one, 0.2d, PointerPhase.Ended), false), Is.False);
            Assert.That(tracker.HasActivePointer, Is.False);
        }
    }
}

