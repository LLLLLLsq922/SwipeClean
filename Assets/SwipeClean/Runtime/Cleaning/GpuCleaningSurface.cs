using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace SwipeClean.Cleaning
{
    /// <summary>
    /// M1 single-layer GPU cleaning surface. Owns all render textures and only exposes a composite output.
    /// </summary>
    public sealed class GpuCleaningSurface : MonoBehaviour, IDisposable
    {
        private const int MaxStampsPerBatch = 16;
        private const int MaxCommandsPerFrame = 48;
        private const float CoverageIntervalSeconds = 0.16f;

        private readonly BrushCommandQueue _commands = new BrushCommandQueue(256);
        private readonly Vector4[] _stamp0 = new Vector4[MaxStampsPerBatch];
        private readonly Vector4[] _stamp1 = new Vector4[MaxStampsPerBatch];
        private readonly List<RenderTexture> _reductionChain = new List<RenderTexture>();

        private Material _initializeMaterial;
        private Material _updateMaterial;
        private Material _compositeMaterial;
        private Material _reduceMaterial;
        private RenderTexture _stateRead;
        private RenderTexture _stateWrite;
        private RenderTexture _composite;
        private float _coverageTimer;
        private float _initialMeanMass = -1f;
        private bool _coveragePending;
        private bool _forceSynchronousCoverage;
        private int _generation;

        public RenderTexture OutputTexture => _composite;
        public float Cleanliness01 { get; private set; }
        public bool IsInitialized { get; private set; }
        public bool IsCoverageReady => _initialMeanMass > 0f;
        public int PendingCommandCount => _commands.Count;

        public event Action<float> CleanlinessChanged;

        public bool Initialize(int resolution = 512)
        {
            DisposeResources();
            resolution = Mathf.Clamp(Mathf.ClosestPowerOfTwo(resolution), 128, 1024);

            var initializeShader = Shader.Find("Hidden/SwipeClean/StainInitialize");
            var updateShader = Shader.Find("Hidden/SwipeClean/StainUpdate");
            var compositeShader = Shader.Find("Hidden/SwipeClean/SurfaceComposite");
            var reduceShader = Shader.Find("Hidden/SwipeClean/CoverageReduce");
            if (initializeShader == null || updateShader == null || compositeShader == null || reduceShader == null)
            {
                Debug.LogError("[SwipeClean] Required cleaning shaders are unavailable in this player build. " +
                               "Run SwipeClean > Project > Configure Project and rebuild the application.");
                return false;
            }

            _initializeMaterial = new Material(initializeShader) { hideFlags = HideFlags.HideAndDontSave };
            _updateMaterial = new Material(updateShader) { hideFlags = HideFlags.HideAndDontSave };
            _compositeMaterial = new Material(compositeShader) { hideFlags = HideFlags.HideAndDontSave };
            _reduceMaterial = new Material(reduceShader) { hideFlags = HideFlags.HideAndDontSave };

            _stateRead = CreateRenderTexture(resolution, resolution, "SwipeClean State A");
            _stateWrite = CreateRenderTexture(resolution, resolution, "SwipeClean State B");
            _composite = CreateRenderTexture(resolution, resolution, "SwipeClean Composite");
            CreateReductionChain(resolution);

            Graphics.Blit(null, _stateRead, _initializeMaterial);
            Graphics.Blit(_stateRead, _stateWrite);
            Graphics.Blit(_stateRead, _composite, _compositeMaterial);

            _generation++;
            _coverageTimer = 0f;
            _initialMeanMass = -1f;
            _forceSynchronousCoverage = false;
            Cleanliness01 = 0f;
            IsInitialized = true;
            RequestCoverage();
            return true;
        }

        public bool Enqueue(in BrushCommand command)
        {
            if (!IsInitialized || command.Operation != BrushOperation.Clean)
            {
                return false;
            }

            return _commands.TryEnqueue(command);
        }

        private void Update()
        {
            if (!IsInitialized)
            {
                return;
            }

            FlushCommands();
            _coverageTimer -= Time.unscaledDeltaTime;
            if (_coverageTimer <= 0f)
            {
                RequestCoverage();
            }
        }

        private void FlushCommands()
        {
            var processed = 0;
            while (_commands.Count > 0 && processed < MaxCommandsPerFrame)
            {
                var batchCount = 0;
                while (batchCount < MaxStampsPerBatch && processed < MaxCommandsPerFrame &&
                       _commands.TryDequeue(out var command))
                {
                    _stamp0[batchCount] = new Vector4(command.CenterUv.x, command.CenterUv.y,
                        command.RadiusUv, command.Hardness01);
                    _stamp1[batchCount] = new Vector4(command.DirectionUv.x, command.DirectionUv.y,
                        command.StrengthPerSecond, command.DeltaTime);
                    batchCount++;
                    processed++;
                }

                _updateMaterial.SetInt("_StampCount", batchCount);
                _updateMaterial.SetVectorArray("_Stamp0", _stamp0);
                _updateMaterial.SetVectorArray("_Stamp1", _stamp1);
                Graphics.Blit(_stateRead, _stateWrite, _updateMaterial);
                (_stateRead, _stateWrite) = (_stateWrite, _stateRead);
            }

            if (processed > 0)
            {
                Graphics.Blit(_stateRead, _composite, _compositeMaterial);
            }
        }

        private void RequestCoverage()
        {
            if (_coveragePending || _reductionChain.Count == 0)
            {
                return;
            }

            _coverageTimer = CoverageIntervalSeconds;
            Texture source = _stateRead;
            for (var i = 0; i < _reductionChain.Count; i++)
            {
                Graphics.Blit(source, _reductionChain[i], _reduceMaterial);
                source = _reductionChain[i];
            }

            if (SystemInfo.supportsAsyncGPUReadback && !_forceSynchronousCoverage)
            {
                _coveragePending = true;
                var requestGeneration = _generation;
                AsyncGPUReadback.Request(_reductionChain[^1], 0, TextureFormat.RGBA32,
                    request => OnCoverageReadback(request, requestGeneration));
            }
            else
            {
                ReadCoverageSynchronously();
            }
        }

        private void OnCoverageReadback(AsyncGPUReadbackRequest request, int requestGeneration)
        {
            _coveragePending = false;
            if (!IsInitialized || requestGeneration != _generation)
            {
                return;
            }

            if (request.hasError)
            {
                _forceSynchronousCoverage = true;
                Debug.LogWarning("[SwipeClean] Async GPU coverage readback failed; using synchronous mobile fallback.");
                ReadCoverageSynchronously();
                return;
            }

            var data = request.GetData<byte>();
            if (data.Length >= 1)
            {
                ApplyMeanMass(data[0] / 255f);
            }
        }

        private void ReadCoverageSynchronously()
        {
            var previous = RenderTexture.active;
            RenderTexture.active = _reductionChain[^1];
            var pixel = new Texture2D(1, 1, TextureFormat.RGBA32, false, true);
            pixel.ReadPixels(new Rect(0, 0, 1, 1), 0, 0, false);
            pixel.Apply(false, false);
            RenderTexture.active = previous;
            ApplyMeanMass(pixel.GetPixel(0, 0).r);
            DestroyOwned(pixel);
        }

        private void ApplyMeanMass(float currentMeanMass)
        {
            if (_initialMeanMass < 0f)
            {
                _initialMeanMass = Mathf.Max(currentMeanMass, 0.000001f);
                return;
            }

            var previous = Cleanliness01;
            Cleanliness01 = Mathf.Clamp01(1f - currentMeanMass / _initialMeanMass);
            if (!Mathf.Approximately(previous, Cleanliness01))
            {
                CleanlinessChanged?.Invoke(Cleanliness01);
            }
        }

        private void CreateReductionChain(int resolution)
        {
            var size = Mathf.Max(1, resolution / 2);
            while (true)
            {
                _reductionChain.Add(CreateRenderTexture(size, size, $"SwipeClean Coverage {size}"));
                if (size == 1)
                {
                    break;
                }

                size = Mathf.Max(1, size / 2);
            }
        }

        private static RenderTexture CreateRenderTexture(int width, int height, string textureName)
        {
            var texture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.Linear)
            {
                name = textureName,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                useMipMap = false,
                autoGenerateMips = false,
                hideFlags = HideFlags.HideAndDontSave
            };
            texture.Create();
            return texture;
        }

        public void Dispose()
        {
            DisposeResources();
            IsInitialized = false;
            _generation++;
        }

        private void OnDestroy() => Dispose();

        private void DisposeResources()
        {
            _commands.Clear();
            ReleaseRenderTexture(ref _stateRead);
            ReleaseRenderTexture(ref _stateWrite);
            ReleaseRenderTexture(ref _composite);
            for (var i = 0; i < _reductionChain.Count; i++)
            {
                var texture = _reductionChain[i];
                if (texture != null)
                {
                    texture.Release();
                    DestroyOwned(texture);
                }
            }

            _reductionChain.Clear();
            DestroyOwned(_initializeMaterial);
            DestroyOwned(_updateMaterial);
            DestroyOwned(_compositeMaterial);
            DestroyOwned(_reduceMaterial);
            _initializeMaterial = null;
            _updateMaterial = null;
            _compositeMaterial = null;
            _reduceMaterial = null;
            _coveragePending = false;
            _forceSynchronousCoverage = false;
        }

        private static void ReleaseRenderTexture(ref RenderTexture texture)
        {
            if (texture == null)
            {
                return;
            }

            texture.Release();
            DestroyOwned(texture);
            texture = null;
        }

        private static void DestroyOwned(UnityEngine.Object value)
        {
            if (value == null)
            {
                return;
            }

            if (UnityEngine.Application.isPlaying)
            {
                UnityEngine.Object.Destroy(value);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(value);
            }
        }
    }
}
