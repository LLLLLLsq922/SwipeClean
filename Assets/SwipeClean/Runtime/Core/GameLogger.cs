using System;
using UnityEngine;

namespace SwipeClean.Core
{
    public interface IGameLogger
    {
        void Info(string message);
        void Warning(string message);
        void Error(string message, Exception exception = null);
    }

    public sealed class UnityGameLogger : IGameLogger
    {
        private const string Prefix = "[SwipeClean] ";

        public void Info(string message) => Debug.Log(Prefix + message);
        public void Warning(string message) => Debug.LogWarning(Prefix + message);

        public void Error(string message, Exception exception = null)
        {
            Debug.LogError(Prefix + message);
            if (exception != null)
            {
                Debug.LogException(exception);
            }
        }
    }
}

