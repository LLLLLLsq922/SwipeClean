using System;
using SwipeClean.Application;

namespace SwipeClean.Core
{
    /// <summary>Process-wide service composition root. Owned by GameBootstrap.</summary>
    public sealed class AppContext : IDisposable
    {
        public IEventBus Events { get; }
        public IAppStateMachine StateMachine { get; }
        public IGameClock Clock { get; }

        public AppContext(IEventBus events, IAppStateMachine stateMachine, IGameClock clock)
        {
            Events = events ?? throw new ArgumentNullException(nameof(events));
            StateMachine = stateMachine ?? throw new ArgumentNullException(nameof(stateMachine));
            Clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        public void Dispose()
        {
        }
    }
}

