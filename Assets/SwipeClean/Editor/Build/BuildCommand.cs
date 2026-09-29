using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace SwipeClean.Editor.Build
{
    public static class BuildCommand
    {
        private static readonly string[] PlayerScenes =
        {
            "Assets/SwipeClean/Scenes/Boot.unity",
            "Assets/SwipeClean/Scenes/Frontend.unity",
            "Assets/SwipeClean/Scenes/Gameplay.unity"
        };

        [MenuItem("SwipeClean/Build/Android Development")]
        public static void BuildAndroidDevelopment() => BuildAndroid(true);

        [MenuItem("SwipeClean/Build/Android Release")]
        public static void BuildAndroidRelease() => BuildAndroid(false);

        [MenuItem("SwipeClean/Build/iOS Development Export")]
        public static void BuildIosDevelopment() => BuildIos(true);

        public static void BuildAndroid(bool development)
        {
            var suffix = development ? "development" : "release";
            var output = Path.GetFullPath($"Builds/Android/SwipeClean-{suffix}.apk");
            Directory.CreateDirectory(Path.GetDirectoryName(output) ?? "Builds/Android");
            var options = development
                ? BuildOptions.Development | BuildOptions.ConnectWithProfiler | BuildOptions.AllowDebugging
                : BuildOptions.None;
            Build(BuildTarget.Android, output, options);
        }

        public static void BuildIos(bool development)
        {
            var suffix = development ? "development" : "release";
            var output = Path.GetFullPath($"Builds/iOS/{suffix}");
            Directory.CreateDirectory(output);
            var options = development ? BuildOptions.Development | BuildOptions.ConnectWithProfiler : BuildOptions.None;
            Build(BuildTarget.iOS, output, options);
        }

        private static void Build(BuildTarget target, string outputPath, BuildOptions options)
        {
            for (var i = 0; i < PlayerScenes.Length; i++)
            {
                if (!File.Exists(PlayerScenes[i]))
                {
                    throw new BuildFailedException($"Required scene is missing: {PlayerScenes[i]}. Run SwipeClean > Project > Configure Project.");
                }
            }

            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = PlayerScenes,
                target = target,
                locationPathName = outputPath,
                options = options
            });

            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new BuildFailedException($"{target} build failed: {report.summary.result} ({report.summary.totalErrors} errors).");
            }

            Debug.Log($"[SwipeClean] {target} build succeeded: {outputPath} ({report.summary.totalSize} bytes).");
        }
    }
}
