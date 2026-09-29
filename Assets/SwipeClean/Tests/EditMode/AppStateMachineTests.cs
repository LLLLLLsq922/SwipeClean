using NUnit.Framework;
using SwipeClean.Application;

namespace SwipeClean.Tests.EditMode
{
    public sealed class AppStateMachineTests
    {
        [Test]
        public void LegalPath_ReachesPlaying()
        {
            var machine = new AppStateMachine();

            Assert.That(machine.TryTransition(AppState.Home).Success, Is.True);
            Assert.That(machine.TryTransition(AppState.LevelSelect).Success, Is.True);
            Assert.That(machine.TryTransition(AppState.LoadingLevel).Success, Is.True);
            Assert.That(machine.TryTransition(AppState.Intro).Success, Is.True);
            Assert.That(machine.TryTransition(AppState.Playing).Success, Is.True);
            Assert.That(machine.Current, Is.EqualTo(AppState.Playing));
        }

        [Test]
        public void IllegalTransition_DoesNotChangeState()
        {
            var machine = new AppStateMachine(AppState.Home);

            var result = machine.TryTransition(AppState.Playing);

            Assert.That(result.Success, Is.False);
            Assert.That(result.Error.Code, Is.EqualTo(AppErrorCode.InvalidTransition));
            Assert.That(machine.Current, Is.EqualTo(AppState.Home));
        }

        [Test]
        public void ActiveLease_RejectsSecondTransition()
        {
            var machine = new AppStateMachine(AppState.Home);
            Assert.That(machine.TryBeginTransition(AppState.LevelSelect, out var lease, out _), Is.True);

            Assert.That(machine.TryBeginTransition(AppState.Settings, out _, out var rejection), Is.False);
            Assert.That(rejection.Error.Code, Is.EqualTo(AppErrorCode.Busy));

            lease.Dispose();
            Assert.That(machine.IsBusy, Is.False);
        }
    }
}

