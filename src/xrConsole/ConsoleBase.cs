﻿#define XrConsole_use_optimized_rendering
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using xr.Collections.Generic;
using xr.Windows.Forms;

namespace xr
{
    using RECT = WinAPI.RECT;
    using StockObjects = WinAPI.StockObjects;

    public abstract unsafe class ConsoleBase : ControlEx
    {
        protected sealed class CommandCache
        {
            private CircularBuffer<string> buffer;
            private int currentIndex = -1;

            public CommandCache(int capacity = 64)
            {
                buffer = new CircularBuffer<string>(capacity);
            }

            public int Capacity
            {
                get { return buffer.Capacity; }
                set
                {
                    if (value == buffer.Capacity)
                    {
                        return;
                    }
                    buffer.Capacity = value;
                    Reset();
                }
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
                get
                {
                    if (currentIndex >= 0)
                    {
                        return buffer[currentIndex];
                    }
                    return null;
                }
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

        public ConsoleLabelCollection Header { get; private set; }
        private const uint CounterColor = 0xe1cdcd;
        private const uint EditorTextColor = 0x69cdcd;
        private static readonly Size ContentPadding = new Size(10, 6);
        private WinAPI.TEXTMETRIC textMetric;
        private VoidPtr hBackBuffer;
        private VoidPtr hFont;
        private VoidPtr hWindow;
        private VoidPtr hBackBrush;
        private VoidPtr hSelectionBrush;
        private VoidPtr hdcBackBuffer;
        private Size backBufferSize;
        private VoidPtr hdcWindow;
        protected CommandCache CmdCache;
        protected readonly TextEditor Editor;
        private readonly ScrollHelper scroller;
        private ILogger logger;
        private CircularBuffer<string> logBuffer;
        private int lineCount;
        private volatile int lineIndex;
        private IntRange renderedLines;
        private bool forceRedraw = false;
        private readonly ILineColorProvider colorProvider;
        private bool commandLineEnabled = true;

        public ConsoleBase(ILineColorProvider colorProvider, ILogger logger = null)
        {
            if (colorProvider == null)
                throw new ArgumentNullException("colorProvider");
            this.colorProvider = colorProvider;
            Header = new ConsoleLabelCollection();
            logBuffer = new CircularBuffer<string>(256);
            AttachLogger(logger);
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw, true);
            Editor = new TextEditor(512);
            if (!DesignMode)
            {
                scroller = new ScrollHelper(ScrollUp, ScrollDown);
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

        public bool CommandLineEnabled
        {
            get { return commandLineEnabled; }
            set
            {
                if (value == commandLineEnabled)
                    return;
                commandLineEnabled = value;
                forceRedraw = true;
                Invalidate();
            }
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
            backBufferSize = GetGranulatedClientSize();
            hBackBuffer = WinAPI.CreateCompatibleBitmap(hdcWindow, backBufferSize.Width, backBufferSize.Height);
            Debug.Assert(hBackBuffer, "Unable to create Compatible Bitmap");
            hBackBrush = WinAPI.GetStockObject(StockObjects.BLACK_BRUSH);
            Debug.Assert(hBackBrush, "Unable to create SolidBrush");
            hSelectionBrush = WinAPI.CreateSolidBrush(ConsoleColors.DarkGray);
            Debug.Assert(hSelectionBrush, "Unable to create SolidBrush");
            WinAPI.SelectObject(hdcBackBuffer, hBackBuffer);
            ClearRect(hdcBackBuffer, RECT.FromSize(backBufferSize));
            WinAPI.SetBkMode(hdcBackBuffer, WinAPI.BkModeTypes.OPAQUE);
            WinAPI.SetBkColor(hdcBackBuffer, ConsoleColors.Black);
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
                        if (scroller != null)
                        {
                            scroller.Dispose();
                        }
                        WinAPI.DeleteObject(hFont);
                        WinAPI.DeleteObject(hBackBuffer);
                        WinAPI.DeleteObject(hBackBrush);
                        WinAPI.DeleteObject(hSelectionBrush);
                        WinAPI.DeleteDC(hdcBackBuffer);
                        WinAPI.ReleaseDC(hWindow, hdcWindow);
                    }
                    DisposeHelper.OnDispose(disposing, "xr.ConsoleBase");
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        private Size GetGranulatedClientSize()
        {
            const int granularity = 32;
            var width = ClientSize.Width / granularity * granularity;
            if (width < ClientSize.Width)
            {
                width += granularity;
            }
            var height = ClientSize.Height / granularity * granularity;
            if (height < ClientSize.Height)
            {
                height += granularity;
            }
            return new Size(width, height);
        }

        private void ResizeBackBuffer(Size newSize)
        {
            WinAPI.DeleteObject(hBackBuffer);
            hBackBuffer = WinAPI.CreateCompatibleBitmap(hdcWindow, newSize.Width, newSize.Height);
            backBufferSize = newSize;
            WinAPI.SelectObject(hdcBackBuffer, hBackBuffer);
            ClearRect(hdcBackBuffer, RECT.FromSize(newSize));
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

        public int CommandHistoryCapacity
        {
            get { return CmdCache.Capacity; }
            set { CmdCache.Capacity = value; }
        }

        public int LogHistoryCapacity
        {
            get { return logBuffer.Capacity; }
            set { logBuffer.Capacity = value; }
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
            Invalidate();
        }

        protected void ScrollUp(int amount)
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
            Invalidate();
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
            var newSize = GetGranulatedClientSize();
            if (newSize != backBufferSize)
            {
                ResizeBackBuffer(newSize);
            }
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
            var logRect = GetLogRect();
            var headerRect = GetHeaderRect();
            var topRect = RECT.FromXYWH(0, 0, ClientRectangle.Width, headerRect.Top);
            var bottomRect = RECT.FromXYWH(0, logRect.Bottom,
                ClientRectangle.Width, ClientRectangle.Height - logRect.Bottom);
            ClearRect(hdcBackBuffer, topRect);
            ClearRect(hdcBackBuffer, bottomRect);
            // draw log
            WinAPI.SelectObject(hdcBackBuffer, hFont);
            WinAPI.SetTextAlign(hdcBackBuffer, alignRight);
            DrawCounter();
            WinAPI.SetTextAlign(hdcBackBuffer, alignLeft);
            DrawHeader();
            if (commandLineEnabled)
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
                WinAPI.SetTextColor(hdcBackBuffer, ConsoleColors.White);
                WinAPI.SetBkColor(hdcBackBuffer, ConsoleColors.Black);
                WinAPI.TextOut(hdcBackBuffer, 0, posY, marker, marker.Length);
                Size markerSize;
                WinAPI.GetTextExtentPoint32(hdcBackBuffer, marker, marker.Length, out markerSize);
                #region draw command line
                WinAPI.SetTextColor(hdcBackBuffer, EditorTextColor);
                if (selection.Length == 0)
                {
                    WinAPI.SetBkColor(hdcBackBuffer, ConsoleColors.Black);
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
                        cursorSelected ? ConsoleColors.Lime : ConsoleColors.Default);
                }
                WinAPI.TextOut(hdcBackBuffer, markerSize.Width + cursorOffset.Width,
                    posY + textMetric.tmDescent, "_", 1);
                #endregion
            }
            WinAPI.SetBkMode(hdcBackBuffer, prevMode);
        }

        private RECT GetHeaderRect()
        {
            var lineHeight = textMetric.tmHeight;
            var rectHeight = Header.Count * lineHeight;
            if (Header.Count > 0)
            {
                rectHeight += lineHeight;
            }
            var top = ContentPadding.Height;
            var logRectBottom = GetLogRectBottom();
            var rectBottom = Math.Min(top + rectHeight, logRectBottom);
            var rect = new RECT
            (
                0,
                top,
                ClientSize.Width,
                rectBottom
            );
            return rect;
        }

        private RECT GetLogRect()
        {
            var headerRect = GetHeaderRect();
            var top = headerRect.Bottom;
            var rect = new RECT
            (
                0,
                top,
                ClientSize.Width,
                GetLogRectBottom()
            );
            return rect;
        }

        private int GetLogRectBottom()
        {
            var lineHeight = textMetric.tmHeight;
            var result = ClientSize.Height - lineHeight - ContentPadding.Height;
            if (commandLineEnabled)
                result -= lineHeight;
            return result;
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
            var lineHeight = textMetric.tmHeight;
            var posY = logRect.Bottom - lineHeight * (lineIndex - range.Max + 2);
            // draw lines from bottom to top
            for (var i = range.Max - 1; i >= range.Min; --i)
            {
                var line = GetLineByIndex(i);
                fixed (char* pLine = line)
                {
                    int skipChars;
                    var lineColor = colorProvider.GetLineColor(line, out skipChars);
                    WinAPI.SetBkColor(hdcBackBuffer, ConsoleColors.Black);
                    WinAPI.SetTextColor(hdcBackBuffer, lineColor);
                    if (line.Length - skipChars > 0)
                    {
                        WinAPI.TextOut(hdcBackBuffer, logRect.Left + ContentPadding.Width, posY,
                            pLine + skipChars, line.Length - skipChars);
                    }
                }
                posY -= lineHeight;
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
            var lineHeight = textMetric.tmHeight;
            var visibleLineCount = logRect.Height / lineHeight;
            var firstVisibleLine = lineIndex - visibleLineCount + 1;
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
            var spacingRect = new RECT(logRect.Left, logRect.Top, logRect.Right,
                logRect.Bottom - visibleLineCount * lineHeight);
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
                        dx: 0, dy: scrollDelta * lineHeight,
                        lprcScroll: &logRect, lprcClip: &logRect,
                        hrgnUpdate: null, lprcUpdate: null);
                    if (renderedLines.Min < firstBufferedLine)
                    {
                        var rect = logRect;
                        rect.Bottom -= (lineIndex - firstVisibleLine + 1) * lineHeight;
                        ClearRect(hdcBackBuffer, rect);
                    }
                    // cut lines that've been already rendered
                    if (scrollDelta < 0)
                    {
                        requiredLines.Min = scrolledLines.Max;
                        // clear space below scrolled lines
                        logRect.Top = logRect.Bottom + scrollDelta * lineHeight;
                    }
                    else
                    {
                        requiredLines.Max = scrolledLines.Min;
                        // clear space above scrolled lines
                        logRect.Bottom -= scrolledLines.Size * lineHeight;
                    }
                }
            }
            #endif
            ClearRect(hdcBackBuffer, spacingRect);
            ClearRect(hdcBackBuffer, logRect);
            DrawLogLines(requiredLines);
            renderedLines.Min = firstVisibleLine;
            renderedLines.Max = lineIndex + 1;
            forceRedraw = false;
        }

        private void DrawHeader()
        {
            if (Header.Count == 0)
            {
                return;
            }
            var headerRect = GetHeaderRect();
            var lineHeight = textMetric.tmHeight;
            var maxVisibleLineCount = (headerRect.Height - lineHeight)/lineHeight;
            var visibleLineCount = Math.Min(Header.Count, maxVisibleLineCount);
            var posY = headerRect.Top;
            var posTextX = headerRect.Left + ContentPadding.Width;
            WinAPI.SetBkColor(hdcBackBuffer, ConsoleColors.Black);
            for (var i = 0; i < visibleLineCount; i++)
            {
                var label = Header[i];
                if (forceRedraw || label.NeedRedraw)
                {
                    var lineRect = new RECT(headerRect.Left, posY, headerRect.Right, posY + lineHeight);
                    ClearRect(hdcBackBuffer, lineRect);
                    WinAPI.SetTextColor(hdcBackBuffer, label.Color);
                    WinAPI.TextOut(hdcBackBuffer, posTextX, posY, label.Text, label.Text.Length);
                    label.NeedRedraw = false;
                }
                posY += lineHeight;
            }
            var spacingRect = new RECT(headerRect.Left, posY, headerRect.Right, headerRect.Bottom);
            if (spacingRect.Size.Height > 0)
            {
                ClearRect(hdcBackBuffer, spacingRect);
            }
        }

        private void DrawCounter()
        {
            var posY = ClientSize.Height - ContentPadding.Height;
            if (commandLineEnabled)
                posY -= textMetric.tmHeight;
            var formatString = DesignMode ? "[0/0]" : String.Format("[{0}/{1}]", lineIndex + 1, lineCount);
            var lineWidth = GetLineWidth();
            WinAPI.SetBkColor(hdcBackBuffer, ConsoleColors.Black);
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

        protected abstract void OnCommand(string command);

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (e.Delta > 0)
            {
                ScrollUp(3);
            }
            else
            {
                ScrollDown(3);
            }
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (!commandLineEnabled)
            {
                base.OnKeyPress(e);
                return;
            }
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
                    if (!commandLineEnabled)
                        goto default;
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
                    if (!commandLineEnabled)
                        goto default;
                    var nextCmd = CmdCache.GetNext();
                    if (nextCmd != null)
                    {
                        Editor.Text = nextCmd;
                    }
                    break;

                case Keys.Left:
                    if (!commandLineEnabled)
                        goto default;
                    Editor.MoveCaretLeft(shiftKey, ctrlKey);
                    break;

                case Keys.Right:
                    if (!commandLineEnabled)
                        goto default;
                    Editor.MoveCaretRight(shiftKey, ctrlKey);
                    break;

                case Keys.Home:
                    if (!commandLineEnabled)
                        goto default;
                    Editor.Home(shiftKey);
                    break;

                case Keys.End:
                    if (!commandLineEnabled)
                        goto default;
                    Editor.End(shiftKey);
                    break;

                case Keys.PageUp:
                    e.Handled = false;
                    if (scroller != null && scroller.State != ScrollHelper.ScrollState.Up)
                    {
                        scroller.BeginScrollUp();
                    }
                    break;

                case Keys.PageDown:
                    e.Handled = false;
                    if (scroller != null && scroller.State != ScrollHelper.ScrollState.Down)
                    {
                        scroller.BeginScrollDown();
                    }
                    break;

                case Keys.Enter:
                    if (!commandLineEnabled)
                        goto default;
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
                    if (!commandLineEnabled)
                        goto default;
                    if (!ctrlKey)
                    {
                        goto default;
                    }
                    Editor.SelectAll();
                    e.SuppressKeyPress = true;
                    break;

                case Keys.C:
                    if (!commandLineEnabled)
                        goto default;
                    if (!ctrlKey)
                    {
                        goto default;
                    }
                    Editor.Copy();
                    e.SuppressKeyPress = true;
                    break;

                case Keys.X:
                    if (!commandLineEnabled)
                        goto default;
                    if (!ctrlKey)
                    {
                        goto default;
                    }
                    Editor.Cut();
                    e.SuppressKeyPress = true;
                    break;

                case Keys.V:
                    if (!commandLineEnabled)
                        goto default;
                    if (!ctrlKey)
                    {
                        goto default;
                    }
                    Editor.Paste();
                    e.SuppressKeyPress = true;
                    break;

                case Keys.Escape:
                    if (!commandLineEnabled)
                        goto default;
                    Editor.ResetSelection();
                    e.SuppressKeyPress = true;
                    break;

                case Keys.Back:
                    if (!commandLineEnabled)
                        goto default;
                    Editor.Backspace(ctrlKey);
                    e.SuppressKeyPress = true;
                    break;

                case Keys.Delete:
                    if (!commandLineEnabled)
                        goto default;
                    Editor.Delete(ctrlKey);
                    e.SuppressKeyPress = true;
                    break;

                case Keys.Insert:
                    if (!commandLineEnabled)
                        goto default;
                    Editor.ToggleEditMode();
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
                    if (scroller != null)
                    {
                        scroller.EndScroll();
                    }
                    break;
            }
            base.OnKeyUp(e);
            if (e.Handled)
            {
                Invalidate();
            }
        }

        internal void Msg(string msg)
        {
            if (logger != null)
            {
                logger.Log(msg);
            }
        }

        internal void Msg(string msg, params object[] args)
        {
            if (logger != null)
            {
                logger.Log(String.Format(msg, args));
            }
        }
    }
}
