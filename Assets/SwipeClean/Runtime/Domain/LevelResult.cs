using System;

namespace SwipeClean.Domain
{
    /// <summary>Immutable result of one completed level session.</summary>
    public readonly struct LevelResult
    {
        public string SessionId { get; }
        public string LevelId { get; }
        public float Cleanliness01 { get; }
        public int Stars { get; }
        public int Score { get; }
        public int CoinsEarned { get; }
        public double DurationSeconds { get; }
        public DateTimeOffset CompletedAtUtc { get; }

        public LevelResult(
            string sessionId,
            string levelId,
            float cleanliness01,
            int stars,
            int score,
            int coinsEarned,
            double durationSeconds,
            DateTimeOffset completedAtUtc)
        {
            SessionId = sessionId ?? throw new ArgumentNullException(nameof(sessionId));
            LevelId = levelId ?? throw new ArgumentNullException(nameof(levelId));
            Cleanliness01 = Clamp01(cleanliness01);
            Stars = Clamp(stars, 0, 3);
            Score = Math.Max(0, score);
            CoinsEarned = Math.Max(0, coinsEarned);
            DurationSeconds = Math.Max(0d, durationSeconds);
            CompletedAtUtc = completedAtUtc;
        }

        private static float Clamp01(float value) => value < 0f ? 0f : value > 1f ? 1f : value;
        private static int Clamp(int value, int min, int max) => value < min ? min : value > max ? max : value;
    }
}

