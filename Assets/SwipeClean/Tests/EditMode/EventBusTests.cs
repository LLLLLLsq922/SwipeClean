using System;
using NUnit.Framework;
using SwipeClean.Application;

namespace SwipeClean.Tests.EditMode
{
    public sealed class EventBusTests
    {
        [Test]
        public void SubscriberFailure_IsolatedFromOtherSubscribers()
        {
            var reportedErrors = 0;
            var delivered = 0;
            var bus = new EventBus(_ => reportedErrors++);
            using var throwing = bus.Subscribe<int>(_ => throw new InvalidOperationException("expected"));
            using var healthy = bus.Subscribe<int>(value => delivered += value);

            bus.Publish(3);

            Assert.That(reportedErrors, Is.EqualTo(1));
            Assert.That(delivered, Is.EqualTo(3));
        }

        [Test]
        public void DisposedSubscription_StopsReceiving()
        {
            var delivered = 0;
            var bus = new EventBus();
            var subscription = bus.Subscribe<int>(_ => delivered++);
            subscription.Dispose();

            bus.Publish(1);

            Assert.That(delivered, Is.Zero);
        }
    }
}

