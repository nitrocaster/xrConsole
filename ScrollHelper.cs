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

        private static int scrollIntervalInit = 200;
        private static int scrollIntervalAccel = 1;
        private static int scrollIntervalMin = 5;
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

        internal void ComputeAccelerationTable()
        {
            intervals = new int[16];
            for (var i = 0; i < intervals.Length; ++i)
            {
                var log = (1.0 - 0.5*Math.Log(i + 1))*scrollIntervalInit;
                if (log < scrollIntervalMin)
                {
                    for (var j = i; j < intervals.Length; ++j)
                    {
                        intervals[j] = scrollIntervalMin;
                    }
                    break;
                }
                intervals[i] = (int)Math.Ceiling(log);
            }
        }

        public static void RegisterSelf()
        {
            Program.Console.AddCommand(
                new IntegerVar(
                    "scroll_interval_init",
                    new Accessor<int>(() => scrollIntervalInit, i => scrollIntervalInit = i),
                    1, 1000));
            Program.Console.AddCommand(
                new IntegerVar(
                    "scroll_interval_accel",
                    new Accessor<int>(() => scrollIntervalAccel, i => scrollIntervalAccel = i),
                    1, 1000));
            Program.Console.AddCommand(
                new IntegerVar(
                    "scroll_interval_min",
                    new Accessor<int>(() => scrollIntervalMin, i => scrollIntervalMin = i),
                    1, 1000));
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
            if (accelStepTicks >= scrollIntervalAccel)
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
