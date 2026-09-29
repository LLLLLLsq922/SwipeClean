using System;
using SwipeClean.Domain;

namespace SwipeClean.Levels
{
    public enum LevelSessionState
    {
        Created,
        Running,
        Paused,
        Completed,
        Disposed
    }

    public sealed class LevelSession : IDisposable
    {
        private readonly Func<double> _nowSeconds;
        private readonly ObjectiveEvaluator _objectiveEvaluator;
        private double _startedAt;
        private double _pausedAt;
        private double _totalPausedSeconds;

        public string SessionId { get; }
        public string LevelId { get; }
        public LevelSessionState State { get; private set; }
        public float Cleanliness01 { get; private set; }

        public event Action<float> CleanlinessChanged;
        public event Action<LevelResult> Completed;

        public LevelSession(string levelId, Func<double> nowSeconds, ObjectiveEvaluator objectiveEvaluator = null)
        {
            LevelId = string.IsNullOrWhiteSpace(levelId) ? throw new ArgumentException("Level ID is required.", nameof(levelId)) : levelId;
            _nowSeconds = nowSeconds ?? throw new ArgumentNullException(nameof(nowSeconds));
            _objectiveEvaluator = objectiveEvaluator ?? new ObjectiveEvaluator();
            SessionId = Guid.NewGuid().ToString("N");
            State = LevelSessionState.Created;
        }

        public void Start()
        {
            if (State != LevelSessionState.Created)
            {
                throw new InvalidOperationException($"Cannot start a session in state {State}.");
            }

            _startedAt = _nowSeconds();
            State = LevelSessionState.Running;
        }

        public void SetPaused(bool isPaused)
        {
            if (isPaused && State == LevelSessionState.Running)
            {
                _pausedAt = _nowSeconds();
                State = LevelSessionState.Paused;
            }
            else if (!isPaused && State == LevelSessionState.Paused)
            {
                _totalPausedSeconds += Math.Max(0d, _nowSeconds() - _pausedAt);
                State = LevelSessionState.Running;
            }
        }

        public ObjectiveSignal RecordCleanliness(float cleanliness01)
        {
            if (State != LevelSessionState.Running)
            {
                return ObjectiveSignal.None;
            }

            var clamped = cleanliness01 < 0f ? 0f : cleanliness01 > 1f ? 1f : cleanliness01;
            if (Math.Abs(Cleanliness01 - clamped) > 0.00001f)
            {
                Cleanliness01 = clamped;
                CleanlinessChanged?.Invoke(Cleanliness01);
            }

            var signal = _objectiveEvaluator.Evaluate(Cleanliness01);
            if (signal == ObjectiveSignal.Completed)
            {
                Finish();
            }

            return signal;
        }

        public void Dispose()
        {
            State = LevelSessionState.Disposed;
            CleanlinessChanged = null;
            Completed = null;
        }

        private void Finish()
        {
            var duration = Math.Max(0d, _nowSeconds() - _startedAt - _totalPausedSeconds);
            var score = ScoreCalculator.Calculate(new ScoreInput(Cleanliness01, duration, 45d, 0));
            State = LevelSessionState.Completed;
            var result = new LevelResult(SessionId, LevelId, Cleanliness01, score.Stars, score.Score,
                score.Coins, duration, DateTimeOffset.UtcNow);
            Completed?.Invoke(result);
        }
    }
}

