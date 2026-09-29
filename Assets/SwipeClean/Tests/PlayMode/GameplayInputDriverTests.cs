using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SwipeClean.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SwipeClean.Tests.PlayMode
{
    public sealed class GameplayInputDriverTests : InputTestFixture
    {
        private GameObject _driverObject;
        private GameplayInputDriver _driver;
        private Touchscreen _touchscreen;
        private readonly List<RawPointerSample> _samples = new List<RawPointerSample>();

        public override void Setup()
        {
            base.Setup();
            _samples.Clear();
            _touchscreen = InputSystem.AddDevice<Touchscreen>();
            _driverObject = new GameObject("GameplayInputDriver Test");
            _driver = _driverObject.AddComponent<GameplayInputDriver>();
            _driver.SampleReceived += _samples.Add;
        }

        public override void TearDown()
        {
            if (_driver != null)
            {
                _driver.SampleReceived -= _samples.Add;
            }

            if (_driverObject != null)
            {
                Object.DestroyImmediate(_driverObject);
            }

            base.TearDown();
        }

        [Test]
        public void TouchPressMoveRelease_EmitsCompleteStrokeLifecycle()
        {
            BeginTouch(7, new Vector2(120f, 240f), screen: _touchscreen);
            MoveTouch(7, new Vector2(260f, 420f), screen: _touchscreen);
            EndTouch(7, new Vector2(260f, 420f), screen: _touchscreen);

            Assert.That(_samples, Is.Not.Empty);
            Assert.That(_samples[0].Phase, Is.EqualTo(PointerPhase.Began));
            Assert.That(_samples.Any(sample => sample.Phase == PointerPhase.Moved), Is.True);
            Assert.That(_samples[^1].Phase, Is.EqualTo(PointerPhase.Ended));
            Assert.That(_samples[^1].ScreenPositionPx, Is.EqualTo(new Vector2(260f, 420f)));
        }
    }
}
