using System.ComponentModel;
using System.IO;
using System.Text;
using System;

namespace XrConsoleProject
{
    public interface ILogger : IDisposable
    {
        event Action<string> MessageLogged;
        event Action LogCleared;
        int LineCount { get; }
        void Log(string message);
        void Clear();
    }

    public sealed class PlainLogger : ILogger
    {
        #region Events

        public event Action<string> MessageLogged;

        private void OnMessageLogged(string msg)
        {
            if (MessageLogged != null)
            {
                MessageLogged(msg);
            }
        }

        public event Action LogCleared;

        private void OnLogCleaned()
        {
            if (LogCleared != null)
            {
                LogCleared();
            }
        }

        #endregion

        private object sync = 0;
        private FileStream fs;
        private StreamWriter fsw;
        private readonly string logFilePath;

        public PlainLogger(string filename)
        {
            logFilePath = filename;
            fs = new FileStream(logFilePath, FileMode.Create);
            fsw = new StreamWriter(fs, Encoding.Default);
            fsw.AutoFlush = true;
        }

        public int LineCount
        {
            get { return lineCount; }
            private set { lineCount = value; }
        }

        private volatile int lineCount;

        public void Log(string message)
        {
            lock (sync)
            {
                fsw.WriteLine(message);
                LineCount++;
            }
            OnMessageLogged(message);
        }

        public void Clear()
        {
            lock (sync)
            {
                fs.SetLength(0);
                LineCount = 0;
            }
            OnLogCleaned();
        }

        [DefaultValue(false)]
        public bool IsDisposed
        {
            get;
            private set;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    fsw.Close();
                }
                DisposeHelper.OnDispose(disposing, "PlainLogger");
                IsDisposed = true;
            }
        }

        ~PlainLogger()
        {
            Dispose(false);
        }
    }
}
