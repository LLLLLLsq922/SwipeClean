using System;
using System.Collections.Generic;

namespace SwipeClean.Application
{
    public enum AppState
    {
        Boot,
        PrivacyNotice,
        Home,
        LevelSelect,
        Tools,
        Collection,
        Settings,
        LoadingLevel,
        Intro,
        Playing,
        Paused,
        Inspecting,
        Completed,
        Results,
        FatalError
    }

    public readonly struct TransitionResult
    {
        public bool Success { get; }
        public AppState Previous { get; }
        public AppState Current { get; }
        public AppError Error { get; }

        public TransitionResult(bool success, AppState previous, AppState current, AppError error)
        {
            Success = success;
            Previous = previous;
            Current = current;
            Error = error;
        }
    }

    public interface IAppStateMachine
    {
        AppState Current { get; }
        bool IsBusy { get; }
        TransitionResult TryTransition(AppState target);
        bool TryBeginTransition(AppState target, out TransitionLease lease, out TransitionResult rejection);
    }

    /// <summary>Lease used to serialize asynchronous transitions.</summary>
    public sealed class TransitionLease : IDisposable
    {
        private AppStateMachine _owner;
        private readonly AppState _target;

        internal TransitionLease(AppStateMachine owner, AppState target)
        {
            _owner = owner;
            _target = target;
        }

        public TransitionResult Commit()
        {
            if (_owner == null)
            {
                return new TransitionResult(false, _target, _target, new AppError(AppErrorCode.Busy, "Transition lease has ended."));
            }

            var result = _owner.Commit(this, _target);
            _owner = null;
            return result;
        }

        public void Dispose()
        {
            if (_owner == null)
            {
                return;
            }

            _owner.Cancel(this);
            _owner = null;
        }
    }

    public sealed class AppStateMachine : IAppStateMachine
    {
        private static readonly Dictionary<AppState, HashSet<AppState>> Allowed = BuildTransitions();
        private TransitionLease _activeLease;

        public AppState Current { get; private set; }
        public bool IsBusy => _activeLease != null;

        public event Action<TransitionResult> Transitioned;

        public AppStateMachine(AppState initial = AppState.Boot)
        {
            Current = initial;
        }

        public TransitionResult TryTransition(AppState target)
        {
            if (!TryBeginTransition(target, out var lease, out var rejection))
            {
                return rejection;
            }

            using (lease)
            {
                return lease.Commit();
            }
        }

        public bool TryBeginTransition(AppState target, out TransitionLease lease, out TransitionResult rejection)
        {
            if (_activeLease != null)
            {
                lease = null;
                rejection = Failure(target, AppErrorCode.Busy, "Another transition is already in progress.");
                return false;
            }

            if (!CanTransition(Current, target))
            {
                lease = null;
                rejection = Failure(target, AppErrorCode.InvalidTransition, $"Transition {Current} -> {target} is not allowed.");
                return false;
            }

            lease = new TransitionLease(this, target);
            _activeLease = lease;
            rejection = default;
            return true;
        }

        internal TransitionResult Commit(TransitionLease lease, AppState target)
        {
            if (!ReferenceEquals(_activeLease, lease))
            {
                return Failure(target, AppErrorCode.Busy, "Transition lease is no longer active.");
            }

            var previous = Current;
            Current = target;
            _activeLease = null;
            var result = new TransitionResult(true, previous, Current, AppError.None);
            Transitioned?.Invoke(result);
            return result;
        }

        internal void Cancel(TransitionLease lease)
        {
            if (ReferenceEquals(_activeLease, lease))
            {
                _activeLease = null;
            }
        }

        private TransitionResult Failure(AppState target, AppErrorCode code, string message) =>
            new TransitionResult(false, Current, Current, new AppError(code, message));

        private static bool CanTransition(AppState current, AppState target) =>
            current == target || (Allowed.TryGetValue(current, out var next) && next.Contains(target));

        private static Dictionary<AppState, HashSet<AppState>> BuildTransitions()
        {
            var map = new Dictionary<AppState, HashSet<AppState>>();
            Add(map, AppState.Boot, AppState.PrivacyNotice, AppState.Home, AppState.FatalError);
            Add(map, AppState.PrivacyNotice, AppState.Home, AppState.FatalError);
            Add(map, AppState.Home, AppState.LevelSelect, AppState.Tools, AppState.Collection, AppState.Settings);
            Add(map, AppState.LevelSelect, AppState.Home, AppState.LoadingLevel);
            Add(map, AppState.Tools, AppState.Home);
            Add(map, AppState.Collection, AppState.Home);
            Add(map, AppState.Settings, AppState.Home);
            Add(map, AppState.LoadingLevel, AppState.Intro, AppState.FatalError);
            Add(map, AppState.Intro, AppState.Playing, AppState.LevelSelect);
            Add(map, AppState.Playing, AppState.Paused, AppState.Inspecting, AppState.Completed);
            Add(map, AppState.Paused, AppState.Playing, AppState.LevelSelect);
            Add(map, AppState.Inspecting, AppState.Playing, AppState.Completed);
            Add(map, AppState.Completed, AppState.Results);
            Add(map, AppState.Results, AppState.LoadingLevel, AppState.LevelSelect);
            Add(map, AppState.FatalError, AppState.Boot);
            return map;
        }

        private static void Add(Dictionary<AppState, HashSet<AppState>> map, AppState state, params AppState[] targets) =>
            map[state] = new HashSet<AppState>(targets);
    }
}
