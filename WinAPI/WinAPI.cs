using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace XrConsoleProject
{
    [SuppressUnmanagedCodeSecurity]
    public static unsafe partial class WinAPI
    {
        [DllImport(ExternDll.GDI32, SetLastError = true)]
        public static extern VoidPtr CreateDC(string lpszDriver, string lpszDevice, string lpszOutput,
            VoidPtr lpInitData);

        [DllImport(ExternDll.GDI32, SetLastError = true)]
        public static extern VoidPtr GetStockObject(StockObjects fnObject);

        [DllImport(ExternDll.GDI32, SetLastError = true)]
        public static extern VoidPtr CreateCompatibleDC(VoidPtr hdc);

        [DllImport(ExternDll.GDI32, SetLastError = true)]
        public static extern VoidPtr CreateCompatibleBitmap(VoidPtr hdc, int nWidth, int nHeight);

        [DllImport(ExternDll.GDI32, SetLastError = true)]
        public static extern VoidPtr SelectObject(VoidPtr hdc, VoidPtr hObject);

        [DllImport(ExternDll.GDI32, SetLastError = true)]
        public static extern bool DeleteDC(VoidPtr hdc);

        [DllImport(ExternDll.GDI32, SetLastError = true)]
        public static extern bool DeleteObject(VoidPtr hObject);

        [DllImport(ExternDll.GDI32, SetLastError = true)]
        public static extern VoidPtr CreateSolidBrush(uint color);

        /// <summary>
        ///    Performs a bit-block transfer of the color data corresponding to a
        ///    rectangle of pixels from the specified source device context into
        ///    a destination device context.
        /// </summary>
        /// <param name="hdc">Handle to the destination device context.</param>
        /// <param name="nXDest">The leftmost x-coordinate of the destination rectangle (in pixels).</param>
        /// <param name="nYDest">The topmost y-coordinate of the destination rectangle (in pixels).</param>
        /// <param name="nWidth">The width of the source and destination rectangles (in pixels).</param>
        /// <param name="nHeight">The height of the source and the destination rectangles (in pixels).</param>
        /// <param name="hdcSrc">Handle to the source device context.</param>
        /// <param name="nXSrc">The leftmost x-coordinate of the source rectangle (in pixels).</param>
        /// <param name="nYSrc">The topmost y-coordinate of the source rectangle (in pixels).</param>
        /// <param name="dwRop">A raster-operation code.</param>
        /// <returns>
        ///    <c>true</c> if the operation succeeded, <c>false</c> otherwise.
        /// </returns>
        [DllImport(ExternDll.GDI32, SetLastError = true)]
        public static extern bool BitBlt(VoidPtr hObject, int nXDest, int nYDest, int nWidth,
            int nHeight, VoidPtr hObjSource, int nXSrc, int nYSrc, TernaryRasterOperations dwRop);

        public enum TernaryRasterOperations
        {
            SRCCOPY     = 0x00CC0020,   // dest = source
            SRCPAINT    = 0x00EE0086,   // dest = source OR dest
            SRCAND      = 0x008800C6,   // dest = source AND dest
            SRCINVERT   = 0x00660046,   // dest = source XOR dest
            SRCERASE    = 0x00440328,   // dest = source AND (NOT dest )
            NOTSRCCOPY  = 0x00330008,   // dest = (NOT source)
            NOTSRCERASE = 0x001100A6,   // dest = (NOT src) AND (NOT dest)
            MERGECOPY   = 0x00C000CA,   // dest = (source AND pattern)
            MERGEPAINT  = 0x00BB0226,   // dest = (NOT source) OR dest
            PATCOPY     = 0x00F00021,   // dest = pattern
            PATPAINT    = 0x00FB0A09,   // dest = DPSnoo
            PATINVERT   = 0x005A0049,   // dest = pattern XOR dest
            DSTINVERT   = 0x00550009,   // dest = (NOT dest)
            BLACKNESS   = 0x00000042,   // dest = BLACK
            WHITENESS   = 0x00FF0062,   // dest = WHITE
        };

        [DllImport(ExternDll.USER32, SetLastError = true)]
        public static extern int FillRect(VoidPtr hDC, ref RECT lprc, VoidPtr hbr);

        public enum StockObjects
        {
            WHITE_BRUSH         = 0,
            LTGRAY_BRUSH        = 1,
            GRAY_BRUSH          = 2,
            DKGRAY_BRUSH        = 3,
            BLACK_BRUSH         = 4,
            NULL_BRUSH          = 5,
            HOLLOW_BRUSH        = NULL_BRUSH,
            WHITE_PEN           = 6,
            BLACK_PEN           = 7,
            NULL_PEN            = 8,
            OEM_FIXED_FONT      = 10,
            ANSI_FIXED_FONT     = 11,
            ANSI_VAR_FONT       = 12,
            SYSTEM_FONT         = 13,
            DEVICE_DEFAULT_FONT = 14,
            DEFAULT_PALETTE     = 15,
            SYSTEM_FIXED_FONT   = 16,
            DEFAULT_GUI_FONT    = 17,
            DC_BRUSH            = 18,
            DC_PEN              = 19
        }

        [Serializable, StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct TEXTMETRIC
        {
            public int tmHeight;
            public int tmAscent;
            public int tmDescent;
            public int tmInternalLeading;
            public int tmExternalLeading;
            public int tmAveCharWidth;
            public int tmMaxCharWidth;
            public int tmWeight;
            public int tmOverhang;
            public int tmDigitizedAspectX;
            public int tmDigitizedAspectY;
            public char tmFirstChar;
            public char tmLastChar;
            public char tmDefaultChar;
            public char tmBreakChar;
            public byte tmItalic;
            public byte tmUnderlined;
            public byte tmStruckOut;
            public byte tmPitchAndFamily;
            public byte tmCharSet;
        }

        [Flags]
        public enum TextAlignTypes : int
        {
            TA_NOUPDATECP   = 0,
            TA_UPDATECP     = 1,
            TA_LEFT         = 0,
            TA_RIGHT        = 2,
            TA_CENTER       = 6,
            TA_TOP          = 0,
            TA_BOTTOM       = 8,
            TA_BASELINE     = 24,
            TA_RTLREADING   = 256,
            TA_MASK         = TA_BASELINE + TA_CENTER + TA_UPDATECP + TA_RTLREADING
        }

        [Flags]
        public enum VTextAlignTypes : int
        {
            VTA_BASELINE    = TextAlignTypes.TA_BASELINE,
            VTA_LEFT        = TextAlignTypes.TA_BOTTOM,
            VTA_RIGHT       = TextAlignTypes.TA_TOP,
            VTA_CENTER      = TextAlignTypes.TA_CENTER,
            VTA_BOTTOM      = TextAlignTypes.TA_RIGHT,
            VTA_TOP         = TextAlignTypes.TA_LEFT
        }

        [DllImport(ExternDll.GDI32, SetLastError = true)]
        public static extern bool SetTextAlign(VoidPtr hdc, uint fmode);

        [DllImport(ExternDll.GDI32, SetLastError = true)]
        public static extern bool SetTextAlign(VoidPtr hdc, TextAlignTypes fmode);

        [DllImport(ExternDll.GDI32, SetLastError = true)]
        public static extern uint SetTextColor(VoidPtr hdc, uint crColor);

        [DllImport(ExternDll.GDI32, SetLastError = true)]
        public static extern uint SetBkColor(VoidPtr hdc, uint crColor);

        public enum BkModeTypes : int
        {
            NONE = 0,
            TRANSPARENT = 1,
            OPAQUE = 2
        }

        [DllImport(ExternDll.GDI32, SetLastError = true)]
        public static extern BkModeTypes GetBkMode(VoidPtr hdc);

        [DllImport(ExternDll.GDI32, SetLastError = true)]
        public static extern int SetBkMode(VoidPtr hdc, BkModeTypes iBkMode);
        
        [DllImport(ExternDll.GDI32, CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool ExtTextOut(VoidPtr hdc, int X, int Y, uint fuOptions,
            ref RECT lprc, string lpString, uint cbCount, int[] lpDx);

        [DllImport(ExternDll.GDI32, CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool TextOut(VoidPtr hdc, int nXStart, int nYStart, string lpString, int cbString);

        [DllImport(ExternDll.GDI32, CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool TextOut(VoidPtr hdc, int nXStart, int nYStart, StringBuilder lpString, int cbString);

        [DllImport(ExternDll.GDI32, CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool TextOut(VoidPtr hdc, int nXStart, int nYStart, char* lpString, int cbString);

        [DllImport(ExternDll.GDI32, CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool GetTextMetrics(VoidPtr hdc, out TEXTMETRIC lptm);
        
        [DllImport(ExternDll.GDI32, CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool GetTextExtentPoint32(VoidPtr hdc, char* lpString, int cbString, out Size lpSize);

        [DllImport(ExternDll.GDI32, CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool GetTextExtentPoint32(VoidPtr hdc, string lpString, int cbString, out Size lpSize);

        [DllImport(ExternDll.USER32, SetLastError = true)]
        public static extern VoidPtr GetDC(VoidPtr hWnd);

        [DllImport(ExternDll.USER32, SetLastError = true)]
        public static extern VoidPtr GetDCEx(VoidPtr hwnd, VoidPtr hrgnclip, uint fdwOptions);

        [DllImport(ExternDll.USER32, SetLastError = true)]
        public static extern int ReleaseDC(VoidPtr hwnd, VoidPtr hDC_Screen);

        [DllImport(ExternDll.USER32, SetLastError = true)]
        public static extern bool ScrollDC(VoidPtr hDC, int dx, int dy, RECT* lprcScroll, RECT* lprcClip, VoidPtr hrgnUpdate, RECT* lprcUpdate);

        [DllImport(ExternDll.USER32, CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern VoidPtr DefWindowProc(VoidPtr hWnd, uint uMsg, VoidPtr wParam, VoidPtr lParam);
    }
}