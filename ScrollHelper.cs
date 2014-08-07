using System;
using System.ComponentModel;
using System.Timers;
using xr.ConsoleCommands;

namespace xr
{
    public sealed class ScrollHelper : IDisposable
    {
        public enum ScrollState
        {
            None,
            Up,
            Down
        }

        private const int ScrollIntervalInit = 200;
        private const int ScrollIntervalAccel = 1;
        private const int ScrollIntervalMin = 5;
        private Action scrollAction;
        private Action scrollDown;
        private Action scrollUp;
        private long accelStepTicks;
        private int[] intervals;
        private Timer timer;
        private int timerInterval;
        private int timerIntervalIndex;

        public ScrollHelper(Action scrollUp, Action scrollDown)
        {
            if (scrollUp == null)
            {
                throw new ArgumentNullException("scrollUp");
            }
            if (scrollDown == null)
            {
                throw new ArgumentNullException("scrollDown");
            }
            this.scrollUp = scrollUp;
            this.scrollDown = scrollDown;
            timer = new Timer();
            timer.AutoReset = true;
            timer.Elapsed += OnTick;
            GC.KeepAlive(timer);
            ComputeAccelerationTable();
        }

        public ScrollState State
        {
            get;
            private set;
        }

        private void ComputeAccelerationTable()
        {
            intervals = new int[16];
            for (var i = 0; i < intervals.Length; ++i)
            {
                var log = (1.0 - 0.5*Math.Log(i + 1))*ScrollIntervalInit;
                if (log < ScrollIntervalMin)
                {
                    for (var j = i; j < intervals.Length; ++j)
                    {
                        intervals[j] = ScrollIntervalMin;
                    }
                    break;
                }
                intervals[i] = (int)Math.Ceiling(log);
            }
        }
        
        public void BeginScrollUp()
        {
            if (State == ScrollState.Down)
            {
                EndScroll();
            }
            State = ScrollState.Up;
            scrollAction = scrollUp;
            scrollAction();
            StartTimer();
        }

        public void BeginScrollDown()
        {
            if (State == ScrollState.Up)
            {
                EndScroll();
            }
            State = ScrollState.Down;
            scrollAction = scrollDown;
            scrollAction();
            StartTimer();
        }

        public void EndScroll()
        {
            if (State == ScrollState.None)
            {
                return;
            }
            StopTimer();
            scrollAction = DefaultScrollAction;
            State = ScrollState.None;
            timerInterval = 0;
        }

        private void OnTick(object sender, EventArgs e)
        {
            UpdateScrollingSpeed();
            scrollAction();
        }

        private void StartTimer()
        {
            timerIntervalIndex = 0;
            timerInterval = intervals[timerIntervalIndex];
            timer.Interval = timerInterval;
            timer.Start();
        }

        private void StopTimer()
        {
            timer.Stop();
        }

        private void DefaultScrollAction()
        {
        }

        private void UpdateScrollingSpeed()
        {
            if (State == ScrollState.None)
            {
                return;
            }
            if (accelStepTicks >= ScrollIntervalAccel)
            {
                accelStepTicks = 0;
                timerInterval = intervals[timerIntervalIndex];
                if (timerIntervalIndex + 1 < intervals.Length)
                {
                    timerIntervalIndex++;
                }
                timer.Interval = timerInterval;
                timer.Start();
            }
            else
            {
                accelStepTicks += timerInterval;
            }
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
                    timer.Stop();
                    timer.Dispose();
                }
                DisposeHelper.OnDispose(disposing, "ScrollHelper");
                IsDisposed = true;
            }
        }

        ~ScrollHelper()
        {
            Dispose(false);
        }
    }
}
