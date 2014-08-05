#define XrConsole_use_optimized_rendering
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace XrConsoleProject
{
    using RECT = WinAPI.RECT;
    using StockObjects = WinAPI.StockObjects;

    public abstract unsafe class XrConsoleBase : ControlEx
    {
        protected sealed class CommandCache
        {
            private CircularBuffer<string> buffer;
            private int currentIndex = -1;

            public CommandCache(int size = 64)
            {
                buffer = new CircularBuffer<string>(size);
            }

            public string GetNext()
            {
                if (buffer.Count == 0 || currentIndex < 0)
                {
                    return null;
                }
                if (currentIndex < buffer.Count - 1)
                {
                    currentIndex++;
                    return buffer[currentIndex];
                }
                return null;
            }

            public string GetPrev()
            {
                if (buffer.Count == 0)
                {
                    return null;
                }
                if (currentIndex >= 0)
                {
                    if (currentIndex > 0)
                    {
                        currentIndex--;
                    }
                }
                else
                {
                    currentIndex = 0;
                }
                return buffer[currentIndex];
            }

            public string Current
            {
                get { return buffer[currentIndex]; }
            }

            public void Reset()
            {
                currentIndex = buffer.Count - 1;
            }

            public void Push(string command)
            {
                if (buffer.Count > 0 && buffer[buffer.Count - 1] == command)
                {
                    return;
                }
                buffer.Enqueue(command);
                Reset();
            }
        }

        private const uint CounterColor = 0xe1cdcd;
        private const uint EditorTextColor = 0x69cdcd;
        private static readonly Size MaxClientSize = Screen.PrimaryScreen.Bounds.Size;
        private static readonly Size ContentPadding = new Size(10, 6);
        WinAPI.TEXTMETRIC textMetric;
        private VoidPtr hBackBuffer;
        private VoidPtr hFont;
        private VoidPtr hWindow;
        private VoidPtr hBackBrush;
        private VoidPtr hSelectionBrush;
        private VoidPtr hdcBackBuffer;
        private VoidPtr hdcWindow;
        protected CommandCache CmdCache;
        protected readonly TextEditor Editor;
        protected readonly ScrollHelper Scroller;
        private ILogger logger;
        private CircularBuffer<string> logBuffer;
        private int lineCount;
        private volatile int lineIndex;
        private IntRange renderedLines;
        private bool forceRedraw = false;

        public XrConsoleBase(ILogger logger = null)
        {
            logBuffer = new CircularBuffer<string>(256);
            AttachLogger(logger);
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |ControlStyles.ResizeRedraw, true);
            Editor = new TextEditor(512);
            if (!DesignMode)
            {
                Scroller = new ScrollHelper(ScrollUp, ScrollDown);
            }
            CmdCache = new CommandCache();
        }
        
        public bool AttachLogger(ILogger _logger)
        {
            bool result = false;
            Action callback = () =>
            {
                if (logger != null || _logger == null)
                {
                    return;
                }
                logger = _logger;
                logger.LogCleared += LogCleared;
                logger.MessageLogged += MessageLogged;
                lineCount = logger.LineCount;
                lineIndex = lineCount - 1;
                result = true;
            };
            if (InvokeRequired)
            {
                InvokeSync(callback);
            }
            else
            {
                callback();
            }
            return result;
        }

        public ILogger DetachLogger()
        {
            ILogger _logger = null;
            Action callback = () =>
            {
                _logger = logger;
                if (logger != null)
                {
                    logger.LogCleared -= LogCleared;
                    logger.MessageLogged -= MessageLogged;
                    logger = null;
                }
            };
            if (InvokeRequired)
            {
                InvokeAsync(callback);
            }
            else
            {
                callback();
            }
            return _logger;
        }
        
        private void InitFont(Font font)
        {
            if (hFont)
            {
                WinAPI.DeleteObject(hFont);
            }
            hFont = font.ToHfont();
            if (hdcWindow)
            {
                WinAPI.SelectObject(hdcWindow, hFont);
                WinAPI.GetTextMetrics(hdcWindow, out textMetric);
                forceRedraw = true;
                Invalidate();
            }
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            InitFont(Font);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            hWindow = Handle;
            hdcWindow = WinAPI.GetDC(hWindow);
            Debug.Assert(hdcWindow, "Unable to get DC");
            InitFont(Font);
            hdcBackBuffer = WinAPI.CreateCompatibleDC(hdcWindow);
            Debug.Assert(hdcBackBuffer, "Unable to create Compatible DC");
            hBackBuffer = WinAPI.CreateCompatibleBitmap(hdcWindow, MaxClientSize.Width, MaxClientSize.Height);
            Debug.Assert(hBackBuffer, "Unable to create Compatible Bitmap");
            hBackBrush = WinAPI.GetStockObject(StockObjects.BLACK_BRUSH);
            Debug.Assert(hBackBrush, "Unable to create SolidBrush");
            hSelectionBrush = WinAPI.CreateSolidBrush(XrConsoleColors.DarkGray);
            Debug.Assert(hSelectionBrush, "Unable to create SolidBrush");
            WinAPI.SelectObject(hdcBackBuffer, hBackBuffer);
            ClearRect(hdcBackBuffer, RECT.FromSize(MaxClientSize));
            WinAPI.SetBkMode(hdcBackBuffer, WinAPI.BkModeTypes.OPAQUE);
            WinAPI.SetBkColor(hdcBackBuffer, XrConsoleColors.Black);
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (!IsDisposed)
                {
                    if (disposing)
                    {
                        DetachLogger();
                        if (Scroller != null)
                        {
                            Scroller.Dispose();
                        }
                        WinAPI.DeleteObject(hFont);
                        WinAPI.DeleteObject(hBackBuffer);
                        WinAPI.DeleteObject(hBackBrush);
                        WinAPI.DeleteObject(hSelectionBrush);
                        WinAPI.DeleteDC(hdcBackBuffer);
                        WinAPI.ReleaseDC(hWindow, hdcWindow);
                    }
                    DisposeHelper.OnDispose(disposing, "XrConsoleBase");
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
        
        private void MessageLogged(string msg)
        {
            Action callback = () =>
            {
                logBuffer.Enqueue(msg);
                lineIndex++;
                lineCount = logger != null ? logger.LineCount : 0;
                Invalidate();
            };
            if (InvokeRequired)
            {
                InvokeAsync(callback);
            }
            else
            {
                callback();
            }
        }

        private void LogCleared()
        {
            Action callback = () =>
            {
                logBuffer.Clear();
                lineIndex = -1;
                lineCount = logger != null ? logger.LineCount : 0;
                forceRedraw = true;
                Invalidate();
            };
            if (InvokeRequired)
            {
                InvokeAsync(callback);
            }
            else
            {
                callback();
            }
        }
        
        protected void ScrollUp()
        {
            ScrollUp(1);
        }

        protected void ScrollDown()
        {
            ScrollDown(1);
        }

        protected void ScrollDown(int amount)
        {
            Action callback = () =>
            {
                if (logger == null)
                {
                    return;
                }
                var availableIncrement = lineCount - (lineIndex + 1);
                if (availableIncrement <= 0)
                {
                    return;
                }
                if (amount <= availableIncrement)
                {
                    lineIndex += amount;
                }
                else
                {
                    lineIndex += availableIncrement;
                }
            };
            if (InvokeRequired)
            {
                InvokeSync(callback);
            }
            else
            {
                callback();
            }
        }

        protected void ScrollUp(int amount)
        {
            Action callback = () =>
            {
                if (logger == null)
                {
                    return;
                }
                var availableDecrement = lineIndex - (lineCount - logBuffer.Count);
                if (availableDecrement <= 0)
                {
                    return;
                }
                if (amount <= availableDecrement)
                {
                    lineIndex -= amount;
                }
                else
                {
                    lineIndex -= availableDecrement;
                }
            };
            if (InvokeRequired)
            {
                InvokeSync(callback);
            }
            else
            {
                callback();
            }
        }
        
        private void ClearRect(VoidPtr hdc, RECT rect)
        {
            WinAPI.FillRect(hdc, ref rect, hBackBrush);
        }
        
        protected override void WndProc(ref Message m)
        {
            const uint WM_ERASEBKGND = 0x0014;
            const uint WM_PAINT = 0x000F;
            const uint WM_SETFOCUS = 0x0007;
            switch ((uint)m.Msg)
            {
                case WM_ERASEBKGND:
                    m.Result = (IntPtr)1;
                    return;
        
                case WM_PAINT:
                    OnPaint();
                    m.Result = (IntPtr)0;
                    WinAPI.DefWindowProc(m.HWnd, (uint)m.Msg, m.WParam, m.LParam);
                    return;
        
                case WM_SETFOCUS:
                    m.Result = (IntPtr)0;
                    return;
            }
            base.WndProc(ref m);
        }

        protected override void OnResize(EventArgs e)
        {
            forceRedraw = true;
            base.OnResize(e);
        }
        
        private void OnPaint()
        {
            const WinAPI.TextAlignTypes alignRight = 
                WinAPI.TextAlignTypes.TA_TOP |
                WinAPI.TextAlignTypes.TA_RIGHT |
                WinAPI.TextAlignTypes.TA_NOUPDATECP;
            const WinAPI.TextAlignTypes alignLeft =
                WinAPI.TextAlignTypes.TA_TOP |
                WinAPI.TextAlignTypes.TA_LEFT |
                WinAPI.TextAlignTypes.TA_NOUPDATECP;
            // clear backbuffer except log rectangle
            WinAPI.SelectObject(hdcBackBuffer, hBackBuffer);
            var logRect    = GetLogRect();
            var topRect    = RECT.FromXYWH(0, 0, ClientRectangle.Width, logRect.Top);
            var bottomRect = RECT.FromXYWH(0, logRect.Bottom,
                ClientRectangle.Width, ClientRectangle.Height - logRect.Bottom);
            ClearRect(hdcBackBuffer, topRect);
            ClearRect(hdcBackBuffer, bottomRect);
            // draw log
            WinAPI.SelectObject(hdcBackBuffer, hFont);
            WinAPI.SetTextAlign(hdcBackBuffer, alignRight);
            DrawCounter();
            WinAPI.SetTextAlign(hdcBackBuffer, alignLeft);
            DrawCommandLine();
            DrawLog();
            RedrawWindow();
        }

        private void DrawCommandLine()
        {
            var prevMode = WinAPI.GetBkMode(hdcBackBuffer);
            WinAPI.SetBkMode(hdcBackBuffer, WinAPI.BkModeTypes.TRANSPARENT);
            var selection = Editor.Selection;
            var editorTextLen = Editor.Buffer.Length;
            var posY = ClientSize.Height - textMetric.tmHeight - ContentPadding.Height + 1;
            fixed (char* editorText = Editor.Buffer.ToString())
            {
                const string marker = ">>> ";
                WinAPI.SetTextColor(hdcBackBuffer, XrConsoleColors.White);
                WinAPI.SetBkColor(hdcBackBuffer, XrConsoleColors.Black);
                WinAPI.TextOut(hdcBackBuffer, 0, posY, marker, marker.Length);
                Size markerSize;
                WinAPI.GetTextExtentPoint32(hdcBackBuffer, marker, marker.Length, out markerSize);
                #region draw command line
                WinAPI.SetTextColor(hdcBackBuffer, EditorTextColor);
                if (selection.Length == 0)
                {
                    WinAPI.SetBkColor(hdcBackBuffer, XrConsoleColors.Black);
                }
                else
                {
                    int pix1 = 0;
                    var len1 = selection.StartIndex;
                    var len2 = selection.Length;
                    if (len1 > 0)
                    {
                        Size sz;
                        WinAPI.GetTextExtentPoint32(hdcBackBuffer, editorText, len1, out sz);
                        pix1 = sz.Width;
                    }
                    if (len2 > 0)
                    {
                        Size sz;
                        WinAPI.GetTextExtentPoint32(hdcBackBuffer, editorText + len1, len2, out sz);
                        RECT selectionRect = RECT.FromXYWH(markerSize.Width + pix1, posY, sz.Width, sz.Height);
                        WinAPI.FillRect(hdcBackBuffer, ref selectionRect, hSelectionBrush);
                    }
                }
                WinAPI.TextOut(hdcBackBuffer, markerSize.Width, posY, editorText, editorTextLen);
                #endregion
                #region draw cursor
                Size cursorOffset = default(Size);
                if (Editor.CursorPos != 0)
                {
                    WinAPI.GetTextExtentPoint32(hdcBackBuffer, editorText, Editor.CursorPos, out cursorOffset);
                }
                if (false) // in case if we need to highlight cursor
                {
                    bool cursorSelected = selection.StartIndex <= Editor.CursorPos &&
                        Editor.CursorPos < selection.StartIndex + selection.Length;
                    WinAPI.SetTextColor(hdcBackBuffer,
                        cursorSelected ? XrConsoleColors.Lime : XrConsoleColors.Default);
                }
                WinAPI.TextOut(hdcBackBuffer, markerSize.Width + cursorOffset.Width, posY, "_", 1);
                #endregion
            }
            WinAPI.SetBkMode(hdcBackBuffer, prevMode);
        }

        private RECT GetLogRect()
        {
            var lineHeight = textMetric.tmHeight;
            var rectHeight = ClientSize.Height - 3*lineHeight - 2*ContentPadding.Height;
            var top = ContentPadding.Height + rectHeight - rectHeight/lineHeight*lineHeight;
            var rect = new RECT
            (
                0,
                top,
                ClientSize.Width,
                ClientSize.Height - 3*lineHeight - ContentPadding.Height
            );
            return rect;
        }

        private string GetLineByIndex(int i)
        {
            int minCachedIndex = lineCount - logBuffer.Count;
            if (i < minCachedIndex || i - minCachedIndex >= logBuffer.Count)
            {
                #if DEBUG
                return "<empty>";
                #else
                return String.Empty;
                #endif
            }
            return logBuffer[i - minCachedIndex];
        }

        private void DrawLogLines(IntRange range)
        {
            if (range.Size == 0)
            {
                return;
            }
            WinAPI.SelectObject(hdcBackBuffer, hFont);
            var logRect = GetLogRect();
            var posY = logRect.Bottom - textMetric.tmHeight * (lineIndex - range.Max + 2);
            // draw lines from bottom to top
            for (var i = range.Max - 1; i >= range.Min; --i)
            {
                var line = GetLineByIndex(i);
                fixed (char* pLine = line)
                {
                    var lineColor = GetLineColor(line);
                    WinAPI.SetBkColor(hdcBackBuffer, XrConsoleColors.Black);
                    WinAPI.SetTextColor(hdcBackBuffer, lineColor);
                    if (lineColor != XrConsoleColors.Default && line.Length >= 2)
                    {
                        WinAPI.TextOut(hdcBackBuffer, logRect.Left + ContentPadding.Width, posY, pLine + 2, line.Length - 2);
                    }
                    else
                    {
                        WinAPI.TextOut(hdcBackBuffer, logRect.Left + ContentPadding.Width, posY, pLine, line.Length);
                    }
                }
                posY -= textMetric.tmHeight;
            }
        }
        
        private void DrawLog()
        {
            var logRect = GetLogRect();
            if (logger == null || logBuffer.Count == 0)
            {
                ClearRect(hdcBackBuffer, logRect);
                return;
            }
            // cut out partially visible lines
            var firstVisibleLine = lineIndex - logRect.Height / textMetric.tmHeight + 1;
            var firstBufferedLine = lineCount - logBuffer.Count;
            if (firstVisibleLine < firstBufferedLine)
            {
                firstVisibleLine = firstBufferedLine;
            }
            // number of lines that have to be shown in logRect
            var requiredLines = new IntRange(firstVisibleLine, lineIndex + 1);
            // early out if already shown
            if (!forceRedraw && renderedLines.Size != 0 && renderedLines == requiredLines ||
                renderedLines.Size == 0 && requiredLines.Size == 0)
            {
                return;
            }
            #if XrConsole_use_optimized_rendering
            if (!forceRedraw)
            {
                // lines that will be moved
                var scrolledLines = renderedLines.Intersect(requiredLines);
                var scrollDelta = renderedLines.Max - requiredLines.Max;
                // + => up | - => down
                if (scrolledLines.IsValid && !scrolledLines.IsEmpty && scrollDelta != 0)
                {
                    WinAPI.ScrollDC(hdcBackBuffer,
                        dx: 0, dy: scrollDelta * textMetric.tmHeight,
                        lprcScroll: &logRect, lprcClip: &logRect,
                        hrgnUpdate: null, lprcUpdate: null);
                    if (renderedLines.Min < firstBufferedLine)
                    {
                        var rect = logRect;
                        rect.Bottom -= (lineIndex - firstVisibleLine + 1) * textMetric.tmHeight;
                        ClearRect(hdcBackBuffer, rect);
                    }
                    // cut lines that've been already rendered
                    if (scrollDelta < 0)
                    {
                        requiredLines.Min = scrolledLines.Max;
                        // clear space below scrolled lines
                        logRect.Top = logRect.Bottom + scrollDelta * textMetric.tmHeight;
                    }
                    else
                    {
                        requiredLines.Max = scrolledLines.Min;
                        // clear space above scrolled lines
                        logRect.Bottom -= scrolledLines.Size * textMetric.tmHeight;
                    }
                }
            }
            #endif
            ClearRect(hdcBackBuffer, logRect);
            DrawLogLines(requiredLines);
            renderedLines.Min = firstVisibleLine;
            renderedLines.Max = lineIndex + 1;
            forceRedraw = false;
        }
        
        private void DrawCounter()
        {
            var posY = ClientSize.Height - textMetric.tmHeight - ContentPadding.Height;
            var formatString = DesignMode ? "[0/0]" : String.Format("[{0}/{1}]", lineIndex + 1, lineCount);
            var lineWidth = GetLineWidth();
            WinAPI.SetBkColor(hdcBackBuffer, XrConsoleColors.Black);
            WinAPI.SetTextColor(hdcBackBuffer, CounterColor);
            fixed (char* pFormatString = formatString)
            {
                WinAPI.TextOut(
                    hdcBackBuffer, lineWidth, posY - textMetric.tmHeight,
                    pFormatString, formatString.Length);
            }
        }

        private void RedrawWindow()
        {
            WinAPI.SelectObject(hdcBackBuffer, hBackBuffer);
            WinAPI.BitBlt(
                hdcWindow, 0, 0, ClientSize.Width, ClientSize.Height,
                hdcBackBuffer, 0, 0, WinAPI.TernaryRasterOperations.SRCCOPY);
        }

        private int GetLineWidth()
        {
            return ClientSize.Width - ContentPadding.Width;
        }

        private static uint GetLineColor(string line)
        {
            if (line.Length == 0)
            {
                return XrConsoleColors.White;
            }
            return GetColorByChar(line[0]);
        }

        private static uint GetColorByChar(char c)
        {
            uint result;
            switch (c)
            {
                case '!': //0x21: //'!'
                    result = XrConsoleColors.Red;
                    break;
                case '#': //0x23: //'#'
                    result = XrConsoleColors.Cyan;
                    break;
                case '$': //0x24: //'$'
                    result = XrConsoleColors.Magneta;
                    break;
                case '%': //0x25: //'%'
                    result = XrConsoleColors.DarkMagneta;
                    break;
                case '&': //0x26: //'&'
                    result = XrConsoleColors.Yellow;
                    break;
                case '*': //0x2a: //'*'
                    result = XrConsoleColors.DarkGray;
                    break;
                case '+': //0x2b: //'+'
                    result = XrConsoleColors.LightCyan;
                    break;
                case '-': //0x2d: //'-'
                    result = XrConsoleColors.Lime;
                    break;
                case '/': //0x2f: //'/'
                    result = XrConsoleColors.DarkBlue;
                    break;
                case '=': //0x3d: //'='
                    result = XrConsoleColors.LightYellow;
                    break;
                case '@': //0x40: //'@'
                    result = XrConsoleColors.Blue;
                    break;
                case '^': //0x5e: //'^'
                    result = XrConsoleColors.DarkGreen;
                    break;
                case '~': //0x7e: //'~':
                    result = XrConsoleColors.DarkYellow;
                    break;
                default:
                    result = XrConsoleColors.White;
                    break;
            }
            return result;
        }

        protected abstract void OnCommand(string command);

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (e.Delta > 0)
            {
                ScrollUp(3);
            }
            else
            {
                ScrollDown(3);
            }
            base.OnMouseWheel(e);
            Invalidate();
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            switch (e.KeyChar)
            {
                case '\t': // tab
                case '\r': // enter
                case '\b': // backspace
                case '\n':
                case (char)0x1B: // escape
                    break;

                default:
                    e.Handled = true;
                    Editor.Insert(e.KeyChar);
                    break;
            }
            base.OnKeyPress(e);
            if (e.Handled)
            {
                Invalidate();
            }
        }

        protected override bool IsInputKey(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Right:
                case Keys.Left:
                case Keys.Up:
                case Keys.Down:
                    return true;
                case Keys.Shift | Keys.Right:
                case Keys.Shift | Keys.Left:
                case Keys.Shift | Keys.Up:
                case Keys.Shift | Keys.Down:
                    return true;
            }
            return base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            var ctrlKey     = e.Modifiers.HasFlag(Keys.Control);
            var shiftKey    = e.Modifiers.HasFlag(Keys.Shift);
            var altKey      = e.Modifiers.HasFlag(Keys.Alt);
            e.Handled = true;
            switch (e.KeyCode)
            {
                case Keys.Up:
                    string prevCmd;
                    if (Editor.Text == "")
                    {
                        CmdCache.Reset();
                        prevCmd = CmdCache.Current;
                    }
                    else
                    {
                        prevCmd = CmdCache.GetPrev();
                    }
                    if (prevCmd != null)
                    {
                        Editor.Text = prevCmd;
                    }
                    break;

                case Keys.Down:
                    var nextCmd = CmdCache.GetNext();
                    if (nextCmd != null)
                    {
                        Editor.Text = nextCmd;
                    }
                    break;

                case Keys.Left:
                    Editor.MoveCaretLeft(shiftKey);
                    break;

                case Keys.Right:
                    Editor.MoveCaretRight(shiftKey);
                    break;

                case Keys.Home:
                    Editor.Home(shiftKey);
                    break;

                case Keys.End:
                    Editor.End(shiftKey);
                    break;

                case Keys.PageUp:
                    if (Scroller != null && Scroller.State != ScrollHelper.ScrollState.Up)
                    {
                        Scroller.BeginScrollUp();
                    }
                    break;

                case Keys.PageDown:
                    if (Scroller != null && Scroller.State != ScrollHelper.ScrollState.Down)
                    {
                        Scroller.BeginScrollDown();
                    }
                    break;

                case Keys.Enter:
                    var command = Editor.Text;
                    Editor.Reset();
                    if (command.Length > 0)
                    {
                        OnCommand(command);
                    }
                    else
                    {
                        e.Handled = false;
                    }
                    e.SuppressKeyPress = true;
                    break;

                case Keys.A:
                    if (!ctrlKey)
                    {
                        goto default;
                    }
                    Editor.SelectAll();
                    e.SuppressKeyPress = true;
                    break;

                case Keys.C:
                    if (!ctrlKey)
                    {
                        goto default;
                    }
                    Editor.Copy();
                    e.SuppressKeyPress = true;
                    break;

                case Keys.X:
                    if (!ctrlKey)
                    {
                        goto default;
                    }
                    Editor.Cut();
                    e.SuppressKeyPress = true;
                    break;

                case Keys.V:
                    if (!ctrlKey)
                    {
                        goto default;
                    }
                    Editor.Paste();
                    e.SuppressKeyPress = true;
                    break;

                case Keys.Escape:
                    Editor.ResetSelection();
                    e.SuppressKeyPress = true;
                    break;

                case Keys.Back:
                    Editor.Backspace();
                    e.SuppressKeyPress = true;
                    break;

                case Keys.Delete:
                    Editor.Delete();
                    e.SuppressKeyPress = true;
                    break;
                    
                default:
                    e.Handled = false;
                    break;
            }
            base.OnKeyDown(e);
            if (e.Handled)
            {
                Invalidate();
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.PageUp:
                case Keys.PageDown:
                    e.Handled = true;
                    if (Scroller != null)
                    {
                        Scroller.EndScroll();
                    }
                    break;
            }
            base.OnKeyUp(e);
            if (e.Handled)
            {
                Invalidate();
            }
        }
    }
}
