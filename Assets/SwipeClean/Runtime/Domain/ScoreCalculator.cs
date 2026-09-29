using System;

namespace SwipeClean.Domain
{
    public readonly struct ScoreInput
    {
        public float Cleanliness01 { get; }
        public double DurationSeconds { get; }
        public double ParSeconds { get; }
        public int IncorrectToolUses { get; }

        public ScoreInput(float cleanliness01, double durationSeconds, double parSeconds, int incorrectToolUses)
        {
            Cleanliness01 = cleanliness01;
            DurationSeconds = Math.Max(0d, durationSeconds);
            ParSeconds = Math.Max(1d, parSeconds);
            IncorrectToolUses = Math.Max(0, incorrectToolUses);
        }
    }

    public readonly struct ScoreOutput
    {
        public int Score { get; }
        public int Stars { get; }
        public int Coins { get; }

        public ScoreOutput(int score, int stars, int coins)
        {
            Score = score;
            Stars = stars;
            Coins = coins;
        }
    }

    /// <summary>Deterministic MVP scoring policy; no Unity or wall-clock dependency.</summary>
    public static class ScoreCalculator
    {
        public static ScoreOutput Calculate(in ScoreInput input)
        {
            var cleanliness = Clamp01(input.Cleanliness01);
            var cleanlinessScore = (int)Math.Round(cleanliness * 8000f, MidpointRounding.AwayFromZero);
            var pace01 = Clamp01((float)(input.ParSeconds / Math.Max(input.DurationSeconds, 1d)));
            var paceScore = (int)Math.Round(pace01 * 2000f, MidpointRounding.AwayFromZero);
            var score = Math.Max(0, cleanlinessScore + paceScore - input.IncorrectToolUses * 150);

            var stars = cleanliness >= 0.995f ? 3 : cleanliness >= 0.95f ? 2 : cleanliness >= 0.90f ? 1 : 0;
            var coins = stars == 3 ? 30 : stars == 2 ? 20 : stars == 1 ? 12 : 0;
            return new ScoreOutput(score, stars, coins);
        }

        private static float Clamp01(float value) => value < 0f ? 0f : value > 1f ? 1f : value;
    }
}

