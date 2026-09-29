using NUnit.Framework;
using SwipeClean.Cleaning;
using SwipeClean.Levels;
using UnityEngine;

namespace SwipeClean.Tests.EditMode
{
    public sealed class CleaningAndLevelTests
    {
        [Test]
        public void BrushQueue_PreservesOrderAndReportsOverflow()
        {
            var queue = new BrushCommandQueue(2);
            var first = Command(1);
            var second = Command(2);

            Assert.That(queue.TryEnqueue(first), Is.True);
            Assert.That(queue.TryEnqueue(second), Is.True);
            Assert.That(queue.TryEnqueue(Command(3)), Is.False);
            Assert.That(queue.TryDequeue(out var dequeued), Is.True);
            Assert.That(dequeued.Sequence, Is.EqualTo(1));
        }

        [Test]
        public void BrushCommand_ClampsUnsafeValues()
        {
            var command = new BrushCommand(-1, BrushOperation.Clean, new Vector2(-2f, 5f), Vector2.zero,
                9f, 2f, -1f, 99f, 2f, 1f, 1);

            Assert.That(command.LayerIndex, Is.Zero);
            Assert.That(command.CenterUv, Is.EqualTo(new Vector2(0f, 1f)));
            Assert.That(command.RadiusUv, Is.EqualTo(0.5f));
            Assert.That(command.DeltaTime, Is.EqualTo(1f / 15f));
        }

        [Test]
        public void Objective_RequiresTwoPassingSnapshots()
        {
            var evaluator = new ObjectiveEvaluator(0.9f, 0.985f, 0.002f);
            Assert.That(evaluator.Evaluate(0.99f), Is.EqualTo(ObjectiveSignal.InspectSuggested));
            Assert.That(evaluator.Evaluate(0.99f), Is.EqualTo(ObjectiveSignal.Completed));
            Assert.That(evaluator.Evaluate(1f), Is.EqualTo(ObjectiveSignal.None));
        }

        [Test]
        public void Session_CompletesWithDeterministicDuration()
        {
            var now = 100d;
            using var session = new LevelSession("level_01", () => now,
                new ObjectiveEvaluator(0.9f, 0.98f, 0.002f));
            var completed = false;
            session.Completed += _ => completed = true;
            session.Start();
            now = 112d;

            session.RecordCleanliness(0.99f);
            session.RecordCleanliness(0.99f);

            Assert.That(completed, Is.True);
            Assert.That(session.State, Is.EqualTo(LevelSessionState.Completed));
        }

        private static BrushCommand Command(uint sequence) => new BrushCommand(0, BrushOperation.Clean,
            Vector2.one * 0.5f, Vector2.right, 0.05f, 0.7f, 1f, 0f, 0f, 1f / 60f, sequence);
    }
}

