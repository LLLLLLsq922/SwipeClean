using SwipeClean.Application;
using UnityEngine;

namespace SwipeClean.Core
{
    public sealed class ApplicationLifecycleBridge : MonoBehaviour
    {
        private void OnApplicationPause(bool isPaused)
        {
            if (!isPaused || GameBootstrap.Current == null)
            {
                return;
            }

            PauseActiveGameplay();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && GameBootstrap.Current != null)
            {
                PauseActiveGameplay();
            }
        }

        private static void PauseActiveGameplay()
        {
            var stateMachine = GameBootstrap.Current.StateMachine;
            if (stateMachine.Current != AppState.Playing)
            {
                return;
            }

            stateMachine.TryTransition(AppState.Paused);
            Time.timeScale = 0f;
        }

        private void OnLowMemory()
        {
            Debug.LogWarning("[SwipeClean] Platform reported low memory.");
        }
    }
}

