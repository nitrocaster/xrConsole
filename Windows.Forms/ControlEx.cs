using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace xr.Windows.Forms
{
    /// <summary>
    /// Defines the base class for controls, which are components with visual representation.
    /// </summary>
    public class ControlEx : Control
    {
        private readonly ConcurrentQueue<Action> messageQueue;
        private IntPtr handle;

        public ControlEx()
        {
            messageQueue = new ConcurrentQueue<Action>();
        }

        /// <summary>
        /// Gets a value indicating whether the control is active.
        /// </summary>
        public bool IsActive
        {
            get;
            private set;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            handle = Handle;
            base.OnHandleCreated(e);
        }

        protected override void WndProc(ref Message m)
        {
            switch (unchecked((uint)m.Msg))
            {
                case WM_NCACTIVATE:
                {
                    IsActive = m.WParam != IntPtr.Zero;
                    break;
                }
            }
            Action callback;
            while (messageQueue.Count > 0 && messageQueue.TryDequeue(out callback))
            {
                callback();
            }
            base.WndProc(ref m);
        }

        /// <summary>
        /// Adds a callback to the queue to be invoked from WndProc.
        /// </summary>
        public void InvokeAsync(Action callback)
        {
            messageQueue.Enqueue(callback);
            PostMessage(handle, 0, 0, 0);
        }

        /// <summary>
        /// Same as Invoke, but closes created handle.
        /// </summary>
        public void InvokeSync(Delegate method)
        {
            var result = BeginInvoke(method);
            using (result.AsyncWaitHandle)
            {
                EndInvoke(result);
            }
        }

        /// <summary>
        /// Same as Invoke, but closes created handle.
        /// </summary>
        public void InvokeSync(Delegate method, params object[] args)
        {
            var result = BeginInvoke(method, args);
            using (result.AsyncWaitHandle)
            {
                EndInvoke(result);
            }
        }

        const uint WM_NCACTIVATE = 0x0086;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool PostMessage(VoidPtr hWnd, uint Msg, int wParam, int lParam);
    }
}
