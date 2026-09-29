using SwipeClean.Application;
using UnityEngine;

namespace SwipeClean.Core
{
    [DefaultExecutionOrder(-10000)]
    public sealed class GameBootstrap : MonoBehaviour
    {
        private static GameBootstrap _instance;

        public static AppContext Current { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureCreated()
        {
            if (_instance != null)
            {
                return;
            }

            var root = new GameObject("[SwipeClean] AppRoot");
            root.AddComponent<GameBootstrap>();
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

            var logger = new UnityGameLogger();
            var clock = gameObject.AddComponent<GameClock>();
            var stateMachine = new AppStateMachine();
            var eventBus = new EventBus(exception => logger.Error("Event subscriber failed.", exception));
            Current = new AppContext(eventBus, stateMachine, clock);
            gameObject.AddComponent<ApplicationLifecycleBridge>();

            UnityEngine.Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            stateMachine.TryTransition(AppState.Home);
            logger.Info("Application services initialized.");
        }

        private void OnDestroy()
        {
            if (_instance != this)
            {
                return;
            }

            Current?.Dispose();
            Current = null;
            _instance = null;
        }
    }
}
