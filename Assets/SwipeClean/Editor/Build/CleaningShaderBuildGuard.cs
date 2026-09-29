using SwipeClean.Editor.ProjectSetup;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace SwipeClean.Editor.Build
{
    /// <summary>Prevents a player build from silently stripping shaders loaded through Shader.Find.</summary>
    public sealed class CleaningShaderBuildGuard : IPreprocessBuildWithReport
    {
        public int callbackOrder => -10000;

        public void OnPreprocessBuild(BuildReport report)
        {
            ProjectConfigurator.ConfigureCleaningShaderInclusion();
            AssetDatabase.SaveAssets();

            if (!ProjectConfigurator.TryValidateCleaningShaderInclusion(out var error))
            {
                throw new BuildFailedException(error);
            }
        }
    }
}
