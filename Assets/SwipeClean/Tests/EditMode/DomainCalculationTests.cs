using System.Collections.Generic;
using NUnit.Framework;
using SwipeClean.Domain;

namespace SwipeClean.Tests.EditMode
{
    public sealed class DomainCalculationTests
    {
        [Test]
        public void Cleanliness_UsesWeightedInitialMass()
        {
            var samples = new List<StainMassSample>
            {
                new StainMassSample(1f, 0.5f, 1f),
                new StainMassSample(0.5f, 0f, 2f),
                new StainMassSample(1f, 1f, 100f, false)
            };

            var cleanliness = CleanlinessCalculator.Calculate(samples);

            Assert.That(cleanliness, Is.EqualTo(0.75f).Within(0.0001f));
        }

        [Test]
        public void Cleanliness_WithNoObjectiveMass_ReturnsZero()
        {
            var samples = new List<StainMassSample> { new StainMassSample(0f, 0f, 1f) };
            Assert.That(CleanlinessCalculator.Calculate(samples), Is.Zero);
        }

        [TestCase(0.90f, 1)]
        [TestCase(0.95f, 2)]
        [TestCase(0.995f, 3)]
        public void Score_MapsCleanlinessToStars(float cleanliness, int expectedStars)
        {
            var score = ScoreCalculator.Calculate(new ScoreInput(cleanliness, 45d, 45d, 0));
            Assert.That(score.Stars, Is.EqualTo(expectedStars));
            Assert.That(score.Score, Is.GreaterThan(0));
        }
    }
}

