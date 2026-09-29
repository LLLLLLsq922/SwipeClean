using System.Collections;
using System.Collections.Generic;
using SwipeClean.Application;
using SwipeClean.Cleaning;
using SwipeClean.Core;
using SwipeClean.Domain;
using SwipeClean.Input;
using SwipeClean.Levels;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace SwipeClean.UI
{
    /// <summary>Programmatic M1 vertical slice used until authored screens and content replace it.</summary>
    [DefaultExecutionOrder(-5000)]
    public sealed class PrototypeFlowController : MonoBehaviour
    {
        private static PrototypeFlowController _instance;

        private readonly PrimaryPointerTracker _pointerTracker = new PrimaryPointerTracker();
        private readonly StrokeInterpolator _interpolator = new StrokeInterpolator();
        private readonly StrokeKinematics _kinematics = new StrokeKinematics();
        private readonly List<Vector2> _interpolatedPoints = new List<Vector2>(32);

        private Canvas _canvas;
        private RectTransform _safeRoot;
        private RectTransform _surfaceRect;
        private Image _progressFill;
        private Text _progressText;
        private Text _debugText;
        private GameObject _screenRoot;
        private GameplayInputDriver _inputDriver;
        private GpuCleaningSurface _cleaningSurface;
        private LevelSession _session;
        private Vector2 _previousUv;
        private double _previousTime;
        private uint _sequence;
        private float _smoothedFps = 60f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureCreated()
        {
            if (_instance != null)
            {
                return;
            }

            var root = new GameObject("[SwipeClean] Prototype UI");
            root.AddComponent<PrototypeFlowController>();
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            CreateCanvas();
            ShowHome();
        }

        private void Update()
        {
            var unscaledDelta = Time.unscaledDeltaTime;
            if (unscaledDelta > 0f)
            {
                _smoothedFps = Mathf.Lerp(_smoothedFps, 1f / unscaledDelta, 0.08f);
            }

            if (_debugText != null && _cleaningSurface != null)
            {
                _debugText.text = $"DEV  FPS {_smoothedFps:0}  Queue {_cleaningSurface.PendingCommandCount}";
            }
        }

        private void CreateCanvas()
        {
            var canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            _canvas = canvasObject.GetComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 100;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            var safeObject = new GameObject("SafeArea", typeof(RectTransform), typeof(SafeAreaFitter));
            safeObject.transform.SetParent(canvasObject.transform, false);
            _safeRoot = safeObject.GetComponent<RectTransform>();
            Stretch(_safeRoot);

            if (EventSystem.current == null)
            {
                var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                eventSystem.transform.SetParent(transform, false);
            }
        }

        private void ShowHome()
        {
            Time.timeScale = 1f;
            DisposeLevelRuntime();
            RebuildScreen("Home");

            AddPanel(_screenRoot.transform, "Background", new Color(0.055f, 0.075f, 0.105f, 1f), Vector2.zero, Vector2.one);
            AddText(_screenRoot.transform, "SwipeClean", 92, FontStyle.Bold, TextAnchor.MiddleCenter,
                new Color(0.76f, 0.96f, 0.91f), new Vector2(0.08f, 0.63f), new Vector2(0.92f, 0.82f));
            AddText(_screenRoot.transform, "把杂乱擦掉，留下清爽。", 38, FontStyle.Normal, TextAnchor.MiddleCenter,
                new Color(0.78f, 0.84f, 0.88f), new Vector2(0.08f, 0.53f), new Vector2(0.92f, 0.64f));

            var button = AddButton(_screenRoot.transform, "开始清洁", new Vector2(0.2f, 0.30f), new Vector2(0.8f, 0.40f));
            button.onClick.AddListener(StartFirstLevel);
            AddText(_screenRoot.transform, "M1 PLAYABLE PROTOTYPE", 23, FontStyle.Bold, TextAnchor.MiddleCenter,
                new Color(0.38f, 0.55f, 0.58f), new Vector2(0.1f, 0.08f), new Vector2(0.9f, 0.14f));
        }

        private void StartFirstLevel()
        {
            var state = GameBootstrap.Current.StateMachine;
            if (state.Current == AppState.Home)
            {
                state.TryTransition(AppState.LevelSelect);
                state.TryTransition(AppState.LoadingLevel);
            }
            else if (state.Current == AppState.Results)
            {
                state.TryTransition(AppState.LoadingLevel);
            }

            if (!BuildGameplayScreen())
            {
                state.TryTransition(AppState.FatalError);
                DisposeLevelRuntime();
                ShowFatal("清洁渲染器初始化失败");
                return;
            }

            state.TryTransition(AppState.Intro);
            state.TryTransition(AppState.Playing);
        }

        private bool BuildGameplayScreen()
        {
            DisposeLevelRuntime();
            RebuildScreen("Gameplay");
            AddPanel(_screenRoot.transform, "Background", new Color(0.035f, 0.05f, 0.075f, 1f), Vector2.zero, Vector2.one);

            AddText(_screenRoot.transform, "第 1 关  ·  擦亮玻璃", 40, FontStyle.Bold, TextAnchor.MiddleLeft,
                Color.white, new Vector2(0.07f, 0.90f), new Vector2(0.72f, 0.97f));
            var pauseButton = AddButton(_screenRoot.transform, "暂停", new Vector2(0.76f, 0.905f), new Vector2(0.94f, 0.965f), 28);
            pauseButton.onClick.AddListener(Pause);

            var progressBackground = AddPanel(_screenRoot.transform, "ProgressBackground", new Color(0.13f, 0.17f, 0.21f, 1f),
                new Vector2(0.07f, 0.845f), new Vector2(0.93f, 0.875f));
            progressBackground.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 4f);
            var fillObject = AddPanel(progressBackground.transform, "Fill", new Color(0.27f, 0.90f, 0.72f, 1f), Vector2.zero, Vector2.one);
            _progressFill = fillObject.GetComponent<Image>();
            var fillRect = _progressFill.rectTransform;
            fillRect.anchorMax = new Vector2(0f, 1f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            _progressText = AddText(_screenRoot.transform, "清洁度 0%", 29, FontStyle.Bold, TextAnchor.MiddleCenter,
                new Color(0.78f, 0.96f, 0.90f), new Vector2(0.1f, 0.785f), new Vector2(0.9f, 0.835f));

            var frame = AddPanel(_screenRoot.transform, "SurfaceFrame", new Color(0.11f, 0.15f, 0.18f, 1f),
                new Vector2(0.07f, 0.18f), new Vector2(0.93f, 0.775f));
            var frameRect = frame.GetComponent<RectTransform>();
            frameRect.offsetMin += new Vector2(8f, 8f);
            frameRect.offsetMax -= new Vector2(8f, 8f);

            var surfaceObject = new GameObject("CleaningSurface", typeof(RectTransform), typeof(RawImage));
            surfaceObject.transform.SetParent(frame.transform, false);
            _surfaceRect = surfaceObject.GetComponent<RectTransform>();
            Stretch(_surfaceRect, 18f);
            var surfaceImage = surfaceObject.GetComponent<RawImage>();
            surfaceImage.raycastTarget = true;

            _cleaningSurface = gameObject.AddComponent<GpuCleaningSurface>();
            if (!_cleaningSurface.Initialize(512))
            {
                return false;
            }

            surfaceImage.texture = _cleaningSurface.OutputTexture;
            _cleaningSurface.CleanlinessChanged += OnCleanlinessChanged;

            AddText(_screenRoot.transform, "用手指或鼠标擦掉棕色污渍", 30, FontStyle.Normal, TextAnchor.MiddleCenter,
                new Color(0.72f, 0.79f, 0.83f), new Vector2(0.08f, 0.10f), new Vector2(0.92f, 0.16f));
            AddText(_screenRoot.transform, "●  超细纤维布", 28, FontStyle.Bold, TextAnchor.MiddleCenter,
                new Color(0.34f, 0.92f, 0.76f), new Vector2(0.29f, 0.035f), new Vector2(0.71f, 0.09f));
            _debugText = AddText(_screenRoot.transform, string.Empty, 18, FontStyle.Normal, TextAnchor.MiddleLeft,
                new Color(0.38f, 0.55f, 0.58f), new Vector2(0.02f, 0.005f), new Vector2(0.6f, 0.035f));

            _session = new LevelSession("level_01", () => Time.realtimeSinceStartupAsDouble);
            _session.Completed += OnLevelCompleted;
            _session.Start();

            _inputDriver = gameObject.AddComponent<GameplayInputDriver>();
            _inputDriver.SampleReceived += OnRawPointerSample;
            return true;
        }

        private void OnRawPointerSample(RawPointerSample sample)
        {
            if (_session == null || _session.State != LevelSessionState.Running ||
                _cleaningSurface == null || !_cleaningSurface.IsCoverageReady)
            {
                return;
            }

            var insideSurface = TryProjectToSurface(sample.ScreenPositionPx, out var uv);
            if (sample.Phase == PointerPhase.Began)
            {
                if (!_pointerTracker.TryAccept(sample, !insideSurface))
                {
                    return;
                }

                _previousUv = uv;
                _previousTime = sample.TimeSeconds;
                _kinematics.Begin(sample.TimeSeconds);
                EnqueueStamp(uv, Vector2.zero, 1f / 60f);
                return;
            }

            if (!_pointerTracker.TryAccept(sample, false))
            {
                return;
            }

            uv = new Vector2(Mathf.Clamp01(uv.x), Mathf.Clamp01(uv.y));
            if (sample.TimeSeconds < _previousTime)
            {
                return;
            }

            _interpolatedPoints.Clear();
            var count = _interpolator.Append(_previousUv, uv, 0.07f, _interpolatedPoints);
            var totalDelta = Mathf.Clamp((float)(sample.TimeSeconds - _previousTime), 0f, 1f / 15f);
            var stampDelta = count > 0 ? totalDelta / count : 0f;
            var pointBefore = _previousUv;
            for (var i = 0; i < count; i++)
            {
                var point = _interpolatedPoints[i];
                var time = _previousTime + (sample.TimeSeconds - _previousTime) * ((i + 1f) / count);
                var stroke = _kinematics.Evaluate(pointBefore, point, _previousTime, time,
                    false, sample.Phase == PointerPhase.Ended && i == count - 1);
                EnqueueStamp(stroke.SurfaceUv, stroke.DeltaUv, stampDelta);
                pointBefore = point;
            }

            _previousUv = uv;
            _previousTime = sample.TimeSeconds;
        }

        private void EnqueueStamp(Vector2 uv, Vector2 direction, float deltaTime)
        {
            var command = new BrushCommand(0, BrushOperation.Clean, uv, direction, 0.07f, 0.72f,
                4.8f, 0f, 0f, Mathf.Max(deltaTime, 1f / 240f), ++_sequence);
            _cleaningSurface.Enqueue(command);
        }

        private bool TryProjectToSurface(Vector2 screenPosition, out Vector2 uv)
        {
            uv = Vector2.zero;
            if (_surfaceRect == null || !RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _surfaceRect, screenPosition, null, out var local))
            {
                return false;
            }

            var rect = _surfaceRect.rect;
            uv = new Vector2((local.x - rect.xMin) / rect.width, (local.y - rect.yMin) / rect.height);
            return uv.x >= 0f && uv.x <= 1f && uv.y >= 0f && uv.y <= 1f;
        }

        private void OnCleanlinessChanged(float cleanliness01)
        {
            if (_progressFill != null)
            {
                var anchors = _progressFill.rectTransform.anchorMax;
                anchors.x = cleanliness01;
                _progressFill.rectTransform.anchorMax = anchors;
            }

            if (_progressText != null)
            {
                _progressText.text = $"清洁度 {Mathf.FloorToInt(cleanliness01 * 100f)}%";
            }

            _session?.RecordCleanliness(cleanliness01);
        }

        private void OnLevelCompleted(LevelResult result)
        {
            _inputDriver.enabled = false;
            GameBootstrap.Current.StateMachine.TryTransition(AppState.Completed);
            StartCoroutine(ShowResultsAfterDelay(result));
        }

        private IEnumerator ShowResultsAfterDelay(LevelResult result)
        {
            yield return new WaitForSecondsRealtime(0.75f);
            GameBootstrap.Current.StateMachine.TryTransition(AppState.Results);
            ShowResults(result);
        }

        private void ShowResults(LevelResult result)
        {
            DisposeLevelRuntime();
            RebuildScreen("Results");
            AddPanel(_screenRoot.transform, "Background", new Color(0.045f, 0.075f, 0.09f, 1f), Vector2.zero, Vector2.one);
            AddText(_screenRoot.transform, "清洁完成！", 76, FontStyle.Bold, TextAnchor.MiddleCenter,
                new Color(0.42f, 0.95f, 0.78f), new Vector2(0.08f, 0.71f), new Vector2(0.92f, 0.84f));
            AddText(_screenRoot.transform, new string('★', result.Stars) + new string('☆', 3 - result.Stars), 84,
                FontStyle.Bold, TextAnchor.MiddleCenter, new Color(1f, 0.78f, 0.25f),
                new Vector2(0.1f, 0.59f), new Vector2(0.9f, 0.70f));
            AddText(_screenRoot.transform,
                $"清洁度  {result.Cleanliness01:P0}\n得分  {result.Score:N0}\n奖励  {result.CoinsEarned} 金币\n用时  {result.DurationSeconds:0.0} 秒",
                38, FontStyle.Normal, TextAnchor.MiddleCenter, Color.white,
                new Vector2(0.15f, 0.34f), new Vector2(0.85f, 0.58f));

            var replay = AddButton(_screenRoot.transform, "再擦一次", new Vector2(0.18f, 0.20f), new Vector2(0.82f, 0.29f));
            replay.onClick.AddListener(StartFirstLevel);
            var home = AddButton(_screenRoot.transform, "返回首页", new Vector2(0.18f, 0.09f), new Vector2(0.82f, 0.17f), 30, false);
            home.onClick.AddListener(ReturnHomeFromResults);
        }

        private void Pause()
        {
            if (_session == null || _session.State != LevelSessionState.Running)
            {
                return;
            }

            GameBootstrap.Current.StateMachine.TryTransition(AppState.Paused);
            _session.SetPaused(true);
            _inputDriver.enabled = false;
            Time.timeScale = 0f;

            var overlay = AddPanel(_screenRoot.transform, "PauseOverlay", new Color(0.02f, 0.03f, 0.04f, 0.94f),
                Vector2.zero, Vector2.one);
            AddText(overlay.transform, "已暂停", 68, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white,
                new Vector2(0.1f, 0.62f), new Vector2(0.9f, 0.75f));
            var resume = AddButton(overlay.transform, "继续", new Vector2(0.2f, 0.43f), new Vector2(0.8f, 0.53f));
            resume.onClick.AddListener(() => Resume(overlay));
            var exit = AddButton(overlay.transform, "退出关卡", new Vector2(0.2f, 0.31f), new Vector2(0.8f, 0.40f), 30, false);
            exit.onClick.AddListener(ExitLevel);
        }

        private void Resume(GameObject overlay)
        {
            Time.timeScale = 1f;
            _session.SetPaused(false);
            _inputDriver.enabled = true;
            GameBootstrap.Current.StateMachine.TryTransition(AppState.Playing);
            Destroy(overlay);
        }

        private void ExitLevel()
        {
            Time.timeScale = 1f;
            GameBootstrap.Current.StateMachine.TryTransition(AppState.LevelSelect);
            GameBootstrap.Current.StateMachine.TryTransition(AppState.Home);
            ShowHome();
        }

        private void ReturnHomeFromResults()
        {
            GameBootstrap.Current.StateMachine.TryTransition(AppState.LevelSelect);
            GameBootstrap.Current.StateMachine.TryTransition(AppState.Home);
            ShowHome();
        }

        private void ShowFatal(string message)
        {
            RebuildScreen("FatalError");
            AddPanel(_screenRoot.transform, "Background", new Color(0.12f, 0.025f, 0.035f, 1f), Vector2.zero, Vector2.one);
            AddText(_screenRoot.transform, message, 48, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white,
                new Vector2(0.1f, 0.35f), new Vector2(0.9f, 0.65f));
        }

        private void DisposeLevelRuntime()
        {
            _pointerTracker.Cancel();
            if (_inputDriver != null)
            {
                _inputDriver.SampleReceived -= OnRawPointerSample;
                Destroy(_inputDriver);
                _inputDriver = null;
            }

            if (_cleaningSurface != null)
            {
                _cleaningSurface.CleanlinessChanged -= OnCleanlinessChanged;
                _cleaningSurface.Dispose();
                Destroy(_cleaningSurface);
                _cleaningSurface = null;
            }

            if (_session != null)
            {
                _session.Completed -= OnLevelCompleted;
                _session.Dispose();
                _session = null;
            }
        }

        private void RebuildScreen(string screenName)
        {
            if (_screenRoot != null)
            {
                Destroy(_screenRoot);
            }

            _surfaceRect = null;
            _progressFill = null;
            _progressText = null;
            _debugText = null;
            _screenRoot = new GameObject(screenName, typeof(RectTransform));
            _screenRoot.transform.SetParent(_safeRoot, false);
            Stretch(_screenRoot.GetComponent<RectTransform>());
        }

        private static GameObject AddPanel(Transform parent, string name, Color color, Vector2 anchorMin, Vector2 anchorMax)
        {
            var panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            panel.GetComponent<Image>().color = color;
            return panel;
        }

        private static Text AddText(Transform parent, string value, int fontSize, FontStyle style, TextAnchor alignment,
            Color color, Vector2 anchorMin, Vector2 anchorMax)
        {
            var textObject = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);
            var rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var text = textObject.GetComponent<Text>();
            text.text = value;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.alignment = alignment;
            text.color = color;
            text.raycastTarget = false;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = Mathf.Max(12, fontSize / 2);
            text.resizeTextMaxSize = fontSize;
            return text;
        }

        private static Button AddButton(Transform parent, string label, Vector2 anchorMin, Vector2 anchorMax,
            int fontSize = 38, bool primary = true)
        {
            var buttonObject = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            var rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var image = buttonObject.GetComponent<Image>();
            image.color = primary ? new Color(0.20f, 0.78f, 0.62f, 1f) : new Color(0.15f, 0.21f, 0.25f, 1f);
            var button = buttonObject.GetComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.highlightedColor = primary ? new Color(0.28f, 0.90f, 0.72f, 1f) : new Color(0.22f, 0.29f, 0.34f, 1f);
            colors.pressedColor = new Color(0.12f, 0.60f, 0.48f, 1f);
            button.colors = colors;
            AddText(buttonObject.transform, label, fontSize, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white,
                Vector2.zero, Vector2.one);
            return button;
        }

        private static void Stretch(RectTransform rect, float inset = 0f)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(inset, inset);
            rect.offsetMax = new Vector2(-inset, -inset);
        }

        private void OnDestroy()
        {
            if (_instance != this)
            {
                return;
            }

            DisposeLevelRuntime();
            _instance = null;
        }
    }
}
