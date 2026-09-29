using SwipeClean.Application;
using UnityEngine;

namespace SwipeClean.Core
{
    public sealed class GameClock : MonoBehaviour, IGameClock
    {
        public float ScaledDeltaTime => Time.deltaTime;
        public float UnscaledDeltaTime => Time.unscaledDeltaTime;
        public double Realtime => Time.realtimeSinceStartupAsDouble;
        public bool IsPaused => Time.timeScale <= 0f;
    }
}

