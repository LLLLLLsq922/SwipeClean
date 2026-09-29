namespace SwipeClean.Application
{
    public enum AppErrorCode
    {
        None = 0,
        Busy,
        InvalidTransition,
        InitializationFailed,
        ContentInvalid,
        SceneLoadFailed,
        StorageFailed,
        Unknown
    }

    public readonly struct AppError
    {
        public static AppError None => new AppError(AppErrorCode.None, string.Empty);

        public AppErrorCode Code { get; }
        public string Message { get; }

        public AppError(AppErrorCode code, string message)
        {
            Code = code;
            Message = message ?? string.Empty;
        }

        public override string ToString() => Code == AppErrorCode.None ? "None" : $"{Code}: {Message}";
    }
}

