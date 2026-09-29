using NUnit.Framework;
using SwipeClean.Editor.ProjectSetup;

namespace SwipeClean.Tests.EditMode
{
    public sealed class CleaningShaderInclusionTests
    {
        [Test]
        public void RequiredCleaningShaders_AreIncludedInPlayerBuilds()
        {
            Assert.That(ProjectConfigurator.TryValidateCleaningShaderInclusion(out var error), Is.True, error);
        }
    }
}
