using System;

namespace xr
{
    public interface ILogger : IDisposable
    {
        event Action<string> MessageLogged;
        event Action LogCleared;
        int LineCount { get; }
        void Log(string message);
        void Clear();
    }
}
