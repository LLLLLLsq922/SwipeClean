namespace SwipeClean.Application
{
    public interface IGameClock
    {
        float ScaledDeltaTime { get; }
        float UnscaledDeltaTime { get; }
        double Realtime { get; }
        bool IsPaused { get; }
    }
}

