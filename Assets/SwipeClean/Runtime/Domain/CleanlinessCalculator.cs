using System;
using System.Collections.Generic;

namespace SwipeClean.Domain
{
    public readonly struct StainMassSample
    {
        public float InitialMeanMass { get; }
        public float CurrentMeanMass { get; }
        public float ObjectiveWeight { get; }
        public bool CountsTowardObjective { get; }

        public StainMassSample(float initialMeanMass, float currentMeanMass, float objectiveWeight, bool countsTowardObjective = true)
        {
            InitialMeanMass = Math.Max(0f, initialMeanMass);
            CurrentMeanMass = Math.Max(0f, currentMeanMass);
            ObjectiveWeight = Math.Max(0f, objectiveWeight);
            CountsTowardObjective = countsTowardObjective;
        }
    }

    /// <summary>Pure weighted-mass cleanliness calculation shared by runtime and tests.</summary>
    public static class CleanlinessCalculator
    {
        public static float Calculate(IReadOnlyList<StainMassSample> samples, float epsilon = 0.000001f)
        {
            if (samples == null)
            {
                throw new ArgumentNullException(nameof(samples));
            }

            double initialTotal = 0d;
            double remainingTotal = 0d;

            for (var i = 0; i < samples.Count; i++)
            {
                var sample = samples[i];
                if (!sample.CountsTowardObjective)
                {
                    continue;
                }

                initialTotal += sample.InitialMeanMass * sample.ObjectiveWeight;
                remainingTotal += sample.CurrentMeanMass * sample.ObjectiveWeight;
            }

            if (initialTotal <= Math.Max(epsilon, 0.0000001f))
            {
                return 0f;
            }

            var cleanliness = 1d - remainingTotal / initialTotal;
            return (float)(cleanliness < 0d ? 0d : cleanliness > 1d ? 1d : cleanliness);
        }
    }
}

