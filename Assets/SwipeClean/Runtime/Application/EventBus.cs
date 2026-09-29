using System;
using System.Collections.Generic;

namespace SwipeClean.Application
{
    public interface IEventBus
    {
        IDisposable Subscribe<T>(Action<T> handler);
        void Publish<T>(in T message);
    }

    public sealed class EventBus : IEventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers = new Dictionary<Type, List<Delegate>>();
        private readonly Action<Exception> _onSubscriberError;

        public EventBus(Action<Exception> onSubscriberError = null)
        {
            _onSubscriberError = onSubscriberError;
        }

        public IDisposable Subscribe<T>(Action<T> handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            var type = typeof(T);
            if (!_handlers.TryGetValue(type, out var list))
            {
                list = new List<Delegate>();
                _handlers.Add(type, list);
            }

            list.Add(handler);
            return new Subscription<T>(this, handler);
        }

        public void Publish<T>(in T message)
        {
            if (!_handlers.TryGetValue(typeof(T), out var list) || list.Count == 0)
            {
                return;
            }

            var snapshot = list.ToArray();
            for (var i = 0; i < snapshot.Length; i++)
            {
                try
                {
                    ((Action<T>)snapshot[i]).Invoke(message);
                }
                catch (Exception exception)
                {
                    _onSubscriberError?.Invoke(exception);
                }
            }
        }

        private void Unsubscribe<T>(Action<T> handler)
        {
            if (!_handlers.TryGetValue(typeof(T), out var list))
            {
                return;
            }

            list.Remove(handler);
            if (list.Count == 0)
            {
                _handlers.Remove(typeof(T));
            }
        }

        private sealed class Subscription<T> : IDisposable
        {
            private EventBus _owner;
            private Action<T> _handler;

            public Subscription(EventBus owner, Action<T> handler)
            {
                _owner = owner;
                _handler = handler;
            }

            public void Dispose()
            {
                if (_owner == null)
                {
                    return;
                }

                _owner.Unsubscribe(_handler);
                _owner = null;
                _handler = null;
            }
        }
    }

    public readonly struct CleanlinessChanged
    {
        public float Previous { get; }
        public float Current { get; }

        public CleanlinessChanged(float previous, float current)
        {
            Previous = previous;
            Current = current;
        }
    }

    public readonly struct LevelObjectiveMet
    {
        public string SessionId { get; }
        public float Cleanliness { get; }

        public LevelObjectiveMet(string sessionId, float cleanliness)
        {
            SessionId = sessionId;
            Cleanliness = cleanliness;
        }
    }

    public readonly struct LevelCompleted
    {
        public SwipeClean.Domain.LevelResult Result { get; }
        public LevelCompleted(SwipeClean.Domain.LevelResult result) => Result = result;
    }
}

