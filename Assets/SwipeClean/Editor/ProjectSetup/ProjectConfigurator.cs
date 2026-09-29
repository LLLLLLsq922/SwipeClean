using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace SwipeClean.Editor.ProjectSetup
{
    [InitializeOnLoad]
    public static class ProjectConfigurator
    {
        private const string SceneRoot = "Assets/SwipeClean/Scenes";
        private const string SettingsRoot = "Assets/SwipeClean/Generated/Settings";
        private const string BootScenePath = SceneRoot + "/Boot.unity";
        private const string FrontendScenePath = SceneRoot + "/Frontend.unity";
        private const string GameplayScenePath = SceneRoot + "/Gameplay.unity";
        private const string TestHarnessScenePath = SceneRoot + "/TestHarness.unity";
        private const string PipelineAssetPath = SettingsRoot + "/SwipeCleanUniversalRenderPipeline.asset";
        private const string RendererAssetPath = SettingsRoot + "/SwipeCleanUniversalRenderer.asset";
        private const string GraphicsSettingsPath = "ProjectSettings/GraphicsSettings.asset";

        private static readonly string[] CleaningShaderPaths =
        {
            "Assets/SwipeClean/Content/Shaders/StainInitialize.shader",
            "Assets/SwipeClean/Content/Shaders/StainUpdate.shader",
            "Assets/SwipeClean/Content/Shaders/SurfaceComposite.shader",
            "Assets/SwipeClean/Content/Shaders/CoverageReduce.shader"
        };

        static ProjectConfigurator()
        {
            if (!File.Exists(BootScenePath) || !TryValidateCleaningShaderInclusion(out _))
            {
                EditorApplication.delayCall += ConfigureProject;
            }
        }

        [MenuItem("SwipeClean/Project/Configure Project")]
        public static void ConfigureProject()
        {
            EnsureFolder("Assets/SwipeClean/Generated");
            EnsureFolder(SettingsRoot);
            EnsureFolder(SceneRoot);

            ConfigureSerialization();
            ConfigurePlayer();
            ConfigureRenderPipeline();
            ConfigureCleaningShaderInclusion();
            CreateSceneIfMissing(BootScenePath, "Boot Scene\nRuntime services are created before scene load.");
            CreateSceneIfMissing(FrontendScenePath, "Frontend Scene");
            CreateSceneIfMissing(GameplayScenePath, "Gameplay Scene");
            CreateSceneIfMissing(TestHarnessScenePath, "Development Test Harness");
            ConfigureBuildScenes();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.OpenScene(BootScenePath, OpenSceneMode.Single);
            Debug.Log("[SwipeClean] Project configuration completed.");
        }

        private static void ConfigureSerialization()
        {
            EditorSettings.serializationMode = SerializationMode.ForceText;
#pragma warning disable CS0618
            EditorSettings.externalVersionControl = "Visible Meta Files";
#pragma warning restore CS0618

            var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset");
            if (assets.Length == 0)
            {
                return;
            }

            var settings = new SerializedObject(assets[0]);
            var inputHandler = settings.FindProperty("activeInputHandler");
            if (inputHandler != null)
            {
                inputHandler.intValue = 1;
                settings.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void ConfigurePlayer()
        {
            PlayerSettings.companyName = "SwipeClean Studio";
            PlayerSettings.productName = "SwipeClean";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.runInBackground = false;
            PlayerSettings.usePlayerLog = true;
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.example.swipeclean");
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, "com.example.swipeclean");
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.iOS, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.bundleVersionCode = 3;
            PlayerSettings.iOS.buildNumber = "3";
            PlayerSettings.bundleVersion = "0.1.2";
        }

        private static void ConfigureRenderPipeline()
        {
            var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererAssetPath);
            if (renderer == null)
            {
                renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
                AssetDatabase.CreateAsset(renderer, RendererAssetPath);
            }

            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelineAssetPath);
            if (pipeline == null)
            {
                pipeline = ScriptableObject.CreateInstance<UniversalRenderPipelineAsset>();
                var serializedPipeline = new SerializedObject(pipeline);
                var rendererList = serializedPipeline.FindProperty("m_RendererDataList");
                if (rendererList != null)
                {
                    rendererList.arraySize = 1;
                    rendererList.GetArrayElementAtIndex(0).objectReferenceValue = renderer;
                }

                var defaultRenderer = serializedPipeline.FindProperty("m_DefaultRendererIndex");
                if (defaultRenderer != null)
                {
                    defaultRenderer.intValue = 0;
                }

                serializedPipeline.ApplyModifiedPropertiesWithoutUndo();
                AssetDatabase.CreateAsset(pipeline, PipelineAssetPath);
            }

            GraphicsSettings.defaultRenderPipeline = pipeline;
            QualitySettings.renderPipeline = pipeline;
        }

        public static void ConfigureCleaningShaderInclusion()
        {
            var graphicsAssets = AssetDatabase.LoadAllAssetsAtPath(GraphicsSettingsPath);
            if (graphicsAssets.Length == 0)
            {
                throw new InvalidDataException($"Unable to load {GraphicsSettingsPath}.");
            }

            var graphicsSettings = new SerializedObject(graphicsAssets[0]);
            var alwaysIncluded = graphicsSettings.FindProperty("m_AlwaysIncludedShaders");
            if (alwaysIncluded == null || !alwaysIncluded.isArray)
            {
                throw new InvalidDataException("GraphicsSettings.m_AlwaysIncludedShaders is unavailable.");
            }

            for (var pathIndex = 0; pathIndex < CleaningShaderPaths.Length; pathIndex++)
            {
                var path = CleaningShaderPaths[pathIndex];
                var shader = AssetDatabase.LoadAssetAtPath<Shader>(path);
                if (shader == null)
                {
                    throw new FileNotFoundException($"Required cleaning shader is missing: {path}", path);
                }

                var alreadyIncluded = false;
                for (var shaderIndex = 0; shaderIndex < alwaysIncluded.arraySize; shaderIndex++)
                {
                    if (alwaysIncluded.GetArrayElementAtIndex(shaderIndex).objectReferenceValue == shader)
                    {
                        alreadyIncluded = true;
                        break;
                    }
                }

                if (!alreadyIncluded)
                {
                    var newIndex = alwaysIncluded.arraySize;
                    alwaysIncluded.InsertArrayElementAtIndex(newIndex);
                    alwaysIncluded.GetArrayElementAtIndex(newIndex).objectReferenceValue = shader;
                }
            }

            graphicsSettings.ApplyModifiedPropertiesWithoutUndo();
        }

        public static bool TryValidateCleaningShaderInclusion(out string error)
        {
            var graphicsAssets = AssetDatabase.LoadAllAssetsAtPath(GraphicsSettingsPath);
            if (graphicsAssets.Length == 0)
            {
                error = $"Unable to load {GraphicsSettingsPath}.";
                return false;
            }

            var graphicsSettings = new SerializedObject(graphicsAssets[0]);
            var alwaysIncluded = graphicsSettings.FindProperty("m_AlwaysIncludedShaders");
            if (alwaysIncluded == null || !alwaysIncluded.isArray)
            {
                error = "GraphicsSettings.m_AlwaysIncludedShaders is unavailable.";
                return false;
            }

            for (var pathIndex = 0; pathIndex < CleaningShaderPaths.Length; pathIndex++)
            {
                var path = CleaningShaderPaths[pathIndex];
                var shader = AssetDatabase.LoadAssetAtPath<Shader>(path);
                if (shader == null)
                {
                    error = $"Required cleaning shader is missing: {path}";
                    return false;
                }

                var included = false;
                for (var shaderIndex = 0; shaderIndex < alwaysIncluded.arraySize; shaderIndex++)
                {
                    if (alwaysIncluded.GetArrayElementAtIndex(shaderIndex).objectReferenceValue == shader)
                    {
                        included = true;
                        break;
                    }
                }

                if (!included)
                {
                    error = $"Required cleaning shader is not included in player builds: {shader.name}";
                    return false;
                }
            }

            error = string.Empty;
            return true;
        }

        private static void CreateSceneIfMissing(string path, string markerText)
        {
            if (File.Exists(path))
            {
                return;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var marker = new GameObject(markerText);
            marker.transform.position = Vector3.zero;
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, path);
        }

        private static void ConfigureBuildScenes()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(BootScenePath, true),
                new EditorBuildSettingsScene(FrontendScenePath, true),
                new EditorBuildSettingsScene(GameplayScenePath, true)
            };
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            var parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            var name = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent ?? "Assets", name);
        }
    }
}
