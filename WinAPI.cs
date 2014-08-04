
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ClassNeverInstantiated.Global

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32.SafeHandles;
//using ComTypes = System.Runtime.InteropServices.ComTypes;
using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

// RECT
[SuppressUnmanagedCodeSecurity]
public static unsafe partial class WinAPI
{
    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool SetRect(out RECT lprc, int xLeft, int yTop, int xRight, int yBottom);

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public static RECT Create(int left, int top, int right, int bottom)
        {
            RECT rect;
            rect.Left = left;
            rect.Top = top;
            rect.Right = right;
            rect.Bottom = bottom;
            return rect;
        }

        public static RECT Empty
        {
            get
            {
                return Create(0, 0, 0, 0);
            }
        }
        
        public int Height
        {
            get
            {
                return Bottom - Top;
            }
        }

        public int Width
        {
            get
            {
                return Right - Left;
            }
        }

        public Size Size
        {
            get
            {
                var size = Size.Empty;
                size.Width = Right - Left;
                size.Height = Bottom - Top;
                return size;
            }
        }

        public Point Location
        {
            get
            {
                var point = Point.Empty;
                point.X = Left;
                point.Y = Top;
                return point;
            }
        }

        public static implicit operator Rectangle(RECT src)
        {
            var rect = Rectangle.Empty;
            rect.X = src.Left;
            rect.Y = src.Top;
            rect.Width = src.Right - src.Left;
            rect.Height = src.Bottom - src.Top;
            return rect;
        }

        public static implicit operator RECT(Rectangle src)
        {
            return Create(src.Left, src.Top, src.Right, src.Bottom);
        }

        public static RECT FromXYWH(int x, int y, int width, int height)
        {
            return Create(x, y, x + width, y + height);
        }
    }
}

[SuppressUnmanagedCodeSecurity]
public static unsafe partial class WinAPI
{
    #region Imports
    // [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    //public static extern int ShowWindow(IntPtr hWnd, int cmd);

    // public const int SW_HIDE = 0;
    // public const int SW_SHOW = 5;

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern unsafe bool GetExitCodeProcess([In]IntPtr hProcess, [Out]int* lpExitCode);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern unsafe bool GetProcessTimes(
            [In]IntPtr hProcess,
            [Out]FILETIME* lpCreationTime,
            [Out]FILETIME* lpExitTime,
            [Out]FILETIME* lpKernelTime,
            [Out]FILETIME* lpUserTime
            );

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool GetSystemTimes(out FILETIME lpIdleTime, out FILETIME lpKernelTime, out FILETIME lpUserTime);



    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern unsafe int GetWindowThreadProcessId(IntPtr hWnd, int* lpdwProcessId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool IsHungAppWindow(IntPtr hWnd);

    /// <summary>
    /// Enumerates all top-level windows associated with the specified desktop. It passes the handle to each window, in turn, to an application-defined callback function.
    /// </summary> xrProcessEnumerator
    /// <param name="hDesktop">Передать NULL</param>
    /// <param name="lpEnumCallbackFunction">Указатель на функцию (xrProcessEnumerator)</param>
    /// <param name="lParam">Передать NULL</param>
    /// <returns></returns>
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool EnumDesktopWindows(IntPtr hDesktop, EnumDelegate lpEnumCallbackFunction, IntPtr lParam);
    public delegate bool EnumDelegate(IntPtr hWnd, int lParam);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern IntPtr CreateToolhelp32Snapshot(UInt32 dwFlags, UInt32 th32ProcessID);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern bool Process32First([In]IntPtr hSnapshot, [In, Out]PROCESSENTRY32 lppe);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern bool Process32Next([In]IntPtr hSnapshot, [In, Out]WinAPI.PROCESSENTRY32 lppe);

    public const UInt32 TH32CS_SNAPPROCESS = 0x00000002;
    public const int MAX_PATH = 260;

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [Flags]
    public enum ProcessAccessFlags : uint
    {
        PROCESS_ALL_ACCESS = 0x001F0FFF,
        PROCESS_CREATE_PROCESS = 0x00000080,
        PROCESS_CREATE_THREAD = 0x00000002,
        PROCESS_DUP_HANDLE = 0x00000040,
        PROCESS_QUERY_INFORMATION = 0x00000400,
        PROCESS_SET_QUOTA = 0x00000100,
        PROCESS_SET_INFORMATION = 0x00000200,
        PROCESS_TERMINATE = 0x00000001,
        PROCESS_VM_OPERATION = 0x00000008,
        PROCESS_VM_READ = 0x00000010,
        PROCESS_VM_WRITE = 0x00000020,
        SYNCHRONIZE = 0x00100000
    }

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern IntPtr VirtualAllocEx(
            IntPtr hProcess,
            IntPtr lpAddress,
            uint dwSize,
            AllocationType flAllocationType,
            MemoryProtection flProtect
            );

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, int dwFreeType);

    [Flags]
    public enum AllocationType
    {
        MEM_COMMIT = 0x00001000,
        MEM_RESERVE = 0x00002000,
        MEM_DECOMMIT = 0x00004000,
        MEM_RELEASE = 0x00008000,
        MEM_RESET = 0x00080000,
        MEM_PHYSICAL = 0x00400000,
        MEM_TOPDOWN = 0x00100000,
        MEM_WRITEWATCH = 0x00200000,
        MEM_LARGEPAGES = 0x20000000
    }
    [Flags]
    public enum MemoryProtection
    {
        PAGE_EXECUTE = 0x0010,
        PAGE_EXECUTE_READ = 0x0020,
        PAGE_EXECUTE_READWRITE = 0x0040,
        PAGE_EXECUTE_WRITECOPY = 0x0080,
        PAGE_NOACCESS = 0x0001,
        PAGE_READONLY = 0x0002,
        PAGE_READWRITE = 0x0004,
        PAGE_WRITECOPY = 0x0008,
        PAGE_GUARD = 0x0100,
        PAGE_NOCACHE = 0x0200,
        PAGE_WRITECOMBINE = 0x0400
    }

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern bool WriteProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            Array lpBuffer,
            uint nSize,
            [Out] int lpNumberOfBytesWritten
            );

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern UIntPtr GetProcAddress(IntPtr hModule, string procName);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern IntPtr CreateRemoteThread(
            IntPtr hProcess,
            IntPtr lpThreadAttributes,
            uint dwStackSize,
            ulong lpStartAddress,
            IntPtr lpParameter,
            uint dwCreationFlags,
            IntPtr lpThreadId
            );

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern UInt32 WaitForSingleObject(IntPtr hHandle, UInt32 dwMilliseconds);

    public const int INFINITE = -1;
    public const int WAIT_ABANDONED = 0x00000080;
    public const int WAIT_OBJECT_0 = 0x00000000;
    //public const UInt32 WAIT_TIMEOUT   = 0x00000102;

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern bool CloseHandle(IntPtr hObject);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern bool CloseHandle(uint hObject);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern int FormatMessage(
        int dwFlags,
        IntPtr lpSource,
        int dwMessageId,
        int dwLanguageId,
        [Out] StringBuilder lpBuffer,
        int nSize,
        IntPtr pArguments
        );

    public const int FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x00000100;
    public const int FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200;
    public const int FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;
    /////////////////////////////////////////////////////////////////////////
    public const int MAX_MODULE_NAME32 = 255;
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct MODULEENTRY32
    {
        uint dwSize;
        uint th32ModuleID;
        uint th32ProcessID;
        uint GlblcntUsage;
        uint ProccntUsage;
        IntPtr modBaseAddr;
        uint modBaseSize;
        IntPtr hModule;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_MODULE_NAME32 + 1)]
        char[] szModule;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
        char[] szExePath;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PROCESSENTRY32
    {
        static UInt32 dwSize = (UInt32)Marshal.SizeOf(typeof(PROCESSENTRY32));
        static UInt32 cntUsage = 0;
        static UInt32 th32ProcessID = 0;
        static UIntPtr th32DefaultHeapID = UIntPtr.Zero;
        static UInt32 th32ModuleID = 0;
        static UInt32 cntThreads = 0;
        static UInt32 th32ParentProcessID = 0;
        static UInt32 pcPriClassBase = 0;
        static UInt32 dwFlags = 0;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH * sizeof(char))]
        static string szExeFile;

        public UInt32 ProcID
        {
            get
            {
                return th32ProcessID;
            }
        }
        public UInt32 ParentProcID
        {
            get
            {
                return th32ParentProcessID;
            }
        }
    }

    [DllImport("psapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern uint GetModuleFileNameEx(
            IntPtr hProcess,
            IntPtr hModule,
            [Out] StringBuilder lpBaseName,
            [In] int nSize
            );

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool ConnectNamedPipe(uint hNamedPipe, [In] ref System.Threading.NativeOverlapped lpOverlapped);

    #region [Flags]
    /// <summary>
    /// 
    /// </summary>
    public const uint GENERIC_READ = 0x80000000;
    /// <summary>
    /// 
    /// </summary>
    public const uint GENERIC_WRITE = 0x40000000;
    /// <summary>
    /// 
    /// </summary>
    public const uint GENERIC_EXECUTE = 0x20000000;
    /// <summary>
    /// 
    /// </summary>
    public const uint GENERIC_ALL = 0x10000000;
    /// <summary>
    /// 
    /// </summary>
    public const uint FILE_SHARE_NONE = 0x00000000;
    /// <summary>
    /// Enables subsequent open operations on an object to request read access. 
    /// Otherwise, other processes cannot open the object if they request read access. 
    /// If this flag is not specified, but the object has been opened for read access, the function fails.
    /// </summary>
    public const uint FILE_SHARE_READ = 0x00000001;
    /// <summary>
    /// Enables subsequent open operations on an object to request write access. 
    /// Otherwise, other processes cannot open the object if they request write access. 
    /// If this flag is not specified, but the object has been opened for write access, the function fails.
    /// </summary>
    public const uint FILE_SHARE_WRITE = 0x00000002;
    /// <summary>
    /// Enables subsequent open operations on an object to request delete access. 
    /// Otherwise, other processes cannot open the object if they request delete access.
    /// If this flag is not specified, but the object has been opened for delete access, the function fails.
    /// </summary>
    public const uint FILE_SHARE_DELETE = 0x00000004;
    /// <summary>
    /// Creates a new file. The function fails if a specified file exists.
    /// </summary>
    public const uint CREATE_NEW = 1;
    /// <summary>
    /// Creates a new file, always. 
    /// If a file exists, the function overwrites the file, clears the existing attributes, combines the specified file attributes, 
    /// and flags with FILE_ATTRIBUTE_ARCHIVE, but does not set the security descriptor that the SECURITY_ATTRIBUTES structure specifies.
    /// </summary>
    public const uint CREATE_ALWAYS = 2;
    /// <summary>
    /// Opens a file. The function fails if the file does not exist. 
    /// </summary>
    public const uint OPEN_EXISTING = 3;
    /// <summary>
    /// Opens a file, always. 
    /// If a file does not exist, the function creates a file as if dwCreationDisposition is CREATE_NEW.
    /// </summary>
    public const uint OPEN_ALWAYS = 4;
    /// <summary>
    /// Opens a file and truncates it so that its size is 0 (zero) bytes. The function fails if the file does not exist.
    /// The calling process must open the file with the GENERIC_WRITE access right. 
    /// </summary>
    public const uint TRUNCATE_EXISTING = 5;
    public const uint FILE_ATTRIBUTE_READONLY = 0x00000001;
    public const uint FILE_ATTRIBUTE_HIDDEN = 0x00000002;
    public const uint FILE_ATTRIBUTE_SYSTEM = 0x00000004;
    //const uint Directory                     = 0x00000010;
    public const uint FILE_ATTRIBUTE_ARCHIVE = 0x00000020;
    //const uint Device                          = 0x00000040;
    public const uint FILE_ATTRIBUTE_NORMAL = 0x00000080;
    public const uint FILE_ATTRIBUTE_TEMPORARY = 0x00000100;
    //const uint SparseFile                      = 0x00000200;
    //const uint ReparsePoint                    = 0x00000400;
    //const uint Compressed                      = 0x00000800;
    public const uint FILE_ATTRIBUTE_OFFLINE = 0x00001000;
    //const uint NotContentIndexed               = 0x00002000;
    public const uint FILE_ATTRIBUTE_ENCRYPTED = 0x00004000;
    public const uint FILE_FLAG_WRITE_THROUGH = 0x80000000;
    public const uint FILE_FLAG_OVERLAPPED = 0x40000000;
    public const uint FILE_FLAG_NO_BUFFERING = 0x20000000;
    public const uint FILE_FLAG_RANDOM_ACCESS = 0x10000000;
    public const uint FILE_FLAG_SEQUENTIAL_SCAN = 0x08000000;
    public const uint FILE_FLAG_DELETE_ON_CLOSE = 0x04000000;
    public const uint FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;
    public const uint FILE_FLAG_POSIX_SEMANTICS = 0x01000000;
    public const uint FILE_FLAG_OPEN_REPARSE_POINT = 0x00200000;
    public const uint FILE_FLAG_OPEN_NO_RECALL = 0x00100000;
    //const uint FirstPipeInstance               = 0x00080000;
    #endregion

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern IntPtr LoadLibrary(string lpFileName);
    #endregion

    #region StaticStyle
    [Flags]
    public enum StaticStyle
    {
        SS_ETCHEDHORZ = 0x00000010,
        SS_ETCHEDVERT = 0x00000011,
    }
    #endregion

    #region Console

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern Boolean AllocConsole();
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern Boolean FreeConsole();

    [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern int GetConsoleOutputCP();

    [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern bool SetConsoleTextAttribute(
        IntPtr consoleHandle,
        ushort attributes);

    [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern bool GetConsoleScreenBufferInfo(
        IntPtr consoleHandle,
        out CONSOLE_SCREEN_BUFFER_INFO bufferInfo);

    [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern bool WriteConsoleW(
        IntPtr hConsoleHandle,
        [MarshalAs(UnmanagedType.LPWStr)] string strBuffer,
        UInt32 bufferLen,
        out UInt32 written,
        IntPtr reserved);

    //private const UInt32 STD_INPUT_HANDLE = unchecked((UInt32)(-10));
    public const UInt32 STD_OUTPUT_HANDLE = unchecked((UInt32)(-11));
    public const UInt32 STD_ERROR_HANDLE = unchecked((UInt32)(-12));

    [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern IntPtr GetStdHandle(
        UInt32 type);

    [StructLayout(LayoutKind.Sequential)]
    public struct COORD
    {
        public UInt16 x;
        public UInt16 y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SMALL_RECT
    {
        public UInt16 Left;
        public UInt16 Top;
        public UInt16 Right;
        public UInt16 Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CONSOLE_SCREEN_BUFFER_INFO
    {
        public COORD dwSize;
        public COORD dwCursorPosition;
        public ushort wAttributes;
        public SMALL_RECT srWindow;
        public COORD dwMaximumWindowSize;
    }

    #endregion // Console

    #region Cursor

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool GetCursorPos(ref Point lpPoint);

    #endregion


    public enum ConsoleColors : int
    {
        Blue = 0x0001,
        Green = 0x0002,
        Red = 0x0004,
        White = Blue | Green | Red,
        Yellow = Red | Green,
        Purple = Red | Blue,
        Cyan = Green | Blue,
        HighIntensity = 0x0008,
    }


    #region SystemCommands

    public enum SystemCommands
    {
        SC_SIZE = 0xF000,
        SC_MOVE = 0xF010,
        SC_MINIMIZE = 0xF020,
        SC_MAXIMIZE = 0xF030,
        SC_MAXIMIZE2 = 0xF032,	// fired from double-click on caption
        SC_NEXTWINDOW = 0xF040,
        SC_PREVWINDOW = 0xF050,
        SC_CLOSE = 0xF060,
        SC_VSCROLL = 0xF070,
        SC_HSCROLL = 0xF080,
        SC_MOUSEMENU = 0xF090,
        SC_KEYMENU = 0xF100,
        SC_ARRANGE = 0xF110,
        SC_RESTORE = 0xF120,
        SC_RESTORE2 = 0xF122,	// fired from double-click on caption
        SC_TASKLIST = 0xF130,
        SC_SCREENSAVE = 0xF140,
        SC_HOTKEY = 0xF150,

        SC_DEFAULT = 0xF160,
        SC_MONITORPOWER = 0xF170,
        SC_CONTEXTHELP = 0xF180,
        SC_SEPARATOR = 0xF00F
    }

    #endregion // SystemCommands

    #region PeekMessageOptions
    [Flags]
    public enum PeekMessageOptions
    {
        PM_NOREMOVE = 0x0000,
        PM_REMOVE = 0x0001,
        PM_NOYIELD = 0x0002
    }
    #endregion // PeekMessageOptions

    #region NCHITTEST enum
    /// <summary>
    /// Location of cursorPosition hot spot returnet in WM_NCHITTEST.
    /// </summary>
    public enum NCHITTEST
    {
        /// <summary>
        /// On the screen background or on a dividing line between windows 
        /// (same as HTNOWHERE, except that the DefWindowProc function produces a system beep to indicate an error).
        /// </summary>
        HTERROR = (-2),
        /// <summary>
        /// In a window currently covered by another window in the same thread 
        /// (the message will be sent to underlying windows in the same thread until one of them returns a code that is not HTTRANSPARENT).
        /// </summary>
        HTTRANSPARENT = (-1),
        /// <summary>
        /// On the screen background or on a dividing line between windows.
        /// </summary>
        HTNOWHERE = 0,
        /// <summary>In a client area.</summary>
        HTCLIENT = 1,
        /// <summary>In a title bar.</summary>
        HTCAPTION = 2,
        /// <summary>In a window menu or in a Close button in a child window.</summary>
        HTSYSMENU = 3,
        /// <summary>In a size box (same as HTSIZE).</summary>
        HTGROWBOX = 4,
        /// <summary>In a menu.</summary>
        HTMENU = 5,
        /// <summary>In a horizontal scroll bar.</summary>
        HTHSCROLL = 6,
        /// <summary>In the vertical scroll bar.</summary>
        HTVSCROLL = 7,
        /// <summary>In a Minimize button.</summary>
        HTMINBUTTON = 8,
        /// <summary>In a Maximize button.</summary>
        HTMAXBUTTON = 9,
        /// <summary>In the left border of a resizable window 
        /// (the user can click the mouse to resize the window horizontally).</summary>
        HTLEFT = 10,
        /// <summary>
        /// In the right border of a resizable window 
        /// (the user can click the mouse to resize the window horizontally).
        /// </summary>
        HTRIGHT = 11,
        /// <summary>In the upper-horizontal border of a window.</summary>
        HTTOP = 12,
        /// <summary>In the upper-left corner of a window border.</summary>
        HTTOPLEFT = 13,
        /// <summary>In the upper-right corner of a window border.</summary>
        HTTOPRIGHT = 14,
        /// <summary>	In the lower-horizontal border of a resizable window 
        /// (the user can click the mouse to resize the window vertically).</summary>
        HTBOTTOM = 15,
        /// <summary>In the lower-left corner of a border of a resizable window 
        /// (the user can click the mouse to resize the window diagonally).</summary>
        HTBOTTOMLEFT = 16,
        /// <summary>	In the lower-right corner of a border of a resizable window 
        /// (the user can click the mouse to resize the window diagonally).</summary>
        HTBOTTOMRIGHT = 17,
        /// <summary>In the border of a window that does not have a sizing border.</summary>
        HTBORDER = 18,

        HTOBJECT = 19,
        /// <summary>In a Close button.</summary>
        HTCLOSE = 20,
        /// <summary>In a Help button.</summary>
        HTHELP = 21,
    }

    #endregion //NCHITTEST

    #region DCX enum
    [Flags()]
    internal enum DCX
    {
        DCX_CACHE = 0x2,
        DCX_CLIPCHILDREN = 0x8,
        DCX_CLIPSIBLINGS = 0x10,
        DCX_EXCLUDERGN = 0x40,
        DCX_EXCLUDEUPDATE = 0x100,
        DCX_INTERSECTRGN = 0x80,
        DCX_INTERSECTUPDATE = 0x200,
        DCX_LOCKWINDOWUPDATE = 0x400,
        DCX_NORECOMPUTE = 0x100000,
        DCX_NORESETATTRS = 0x4,
        DCX_PARENTCLIP = 0x20,
        DCX_VALIDATE = 0x200000,
        DCX_WINDOW = 0x1,
    }
    #endregion //DCX


    #region SetWindowPosition flags
    [Flags]
    public enum SetWindowPosOptions
    {
        SWP_NOSIZE = 0x0001,
        SWP_NOMOVE = 0x0002,
        SWP_NOZORDER = 0x0004,
        SWP_NOACTIVATE = 0x0010,
        SWP_FRAMECHANGED = 0x0020,	/* The frame changed: send WM_NCCALCSIZE */
        SWP_SHOWWINDOW = 0x0040,
        SWP_HIDEWINDOW = 0x0080,
        SWP_NOCOPYBITS = 0x0100,
        SWP_NOOWNERZORDER = 0x0200,	/* Don't do owner Z ordering */
        SWP_NOSENDCHANGING = 0x0400		/* Don't send WM_WINDOWPOSCHANGING */
    }
    #endregion

    #region RedrawWindow flags
    [Flags]
    public enum RDW
    {
        RDW_INVALIDATE = 0x0001,
        RDW_INTERNALPAINT = 0x0002,
        RDW_ERASE = 0x0004,
        RDW_VALIDATE = 0x0008,
        RDW_NOINTERNALPAINT = 0x0010,
        RDW_NOERASE = 0x0020,
        RDW_NOCHILDREN = 0x0040,
        RDW_ALLCHILDREN = 0x0080,
        RDW_UPDATENOW = 0x0100,
        RDW_ERASENOW = 0x0200,
        RDW_FRAME = 0x0400,
        RDW_NOFRAME = 0x0800
    }
    #endregion

    #region WINDOWPOS
    [StructLayout(LayoutKind.Sequential)]
    public struct WINDOWPOS
    {
        public IntPtr hwnd;
        public IntPtr hWndInsertAfter;
        public int x;
        public int y;
        public int cx;
        public int cy;
        public uint flags;
    }
    #endregion //WINDOWPOS

    #region NCCALCSIZE_PARAMS
    //http://msdn.microsoft.com/library/default.asp?url=/library/en-us/winui/winui/windowsuserinterface/windowing/windows/windowreference/windowstructures/nccalcsize_params.asp
    [StructLayout(LayoutKind.Sequential)]
    public struct NCCALCSIZE_PARAMS
    {
        /// <summary>
        /// Contains the new coordinates of a window that has been moved or resized, that is, it is the proposed new window coordinates.
        /// </summary>
        public RECT rectProposed;
        /// <summary>
        /// Contains the coordinates of the window before it was moved or resized.
        /// </summary>
        public RECT rectBeforeMove;
        /// <summary>
        /// Contains the coordinates of the window's client area before the window was moved or resized.
        /// </summary>
        public RECT rectClientBeforeMove;
        /// <summary>
        /// Pointer to a WINDOWPOS structure that contains the size and position values specified in the operation that moved or resized the window.
        /// </summary>
        public WINDOWPOS lpPos;
    }
    #endregion //NCCALCSIZE_PARAMS

    #region TRACKMOUSEEVENT structure

    [StructLayout(LayoutKind.Sequential)]
    public class TRACKMOUSEEVENT
    {
        public TRACKMOUSEEVENT()
        {
            this.cbSize = Marshal.SizeOf(typeof(WinAPI.TRACKMOUSEEVENT));
            this.dwHoverTime = 100;
        }

        public int cbSize;
        public int dwFlags;
        public IntPtr hwndTrack;
        public int dwHoverTime;
    }

    #endregion

    #region TrackMouseEventFalgs enum

    [Flags]
    public enum TrackMouseEventFalgs
    {
        TME_HOVER = 1,
        TME_LEAVE = 2,
        TME_NONCLIENT = 0x00000010
    }

    #endregion

    public enum TernaryRasterOperations
    {
        SRCCOPY = 0x00CC0020,     /* dest = source*/
        SRCPAINT = 0x00EE0086,    /* dest = source OR dest*/
        SRCAND = 0x008800C6,      /* dest = source AND dest*/
        SRCINVERT = 0x00660046,   /* dest = source XOR dest*/
        SRCERASE = 0x00440328,    /* dest = source AND (NOT dest )*/
        NOTSRCCOPY = 0x00330008,  /* dest = (NOT source)*/
        NOTSRCERASE = 0x001100A6, /* dest = (NOT src) AND (NOT dest) */
        MERGECOPY = 0x00C000CA,   /* dest = (source AND pattern)*/
        MERGEPAINT = 0x00BB0226,  /* dest = (NOT source) OR dest*/
        PATCOPY = 0x00F00021,     /* dest = pattern*/
        PATPAINT = 0x00FB0A09,    /* dest = DPSnoo*/
        PATINVERT = 0x005A0049,   /* dest = pattern XOR dest*/
        DSTINVERT = 0x00550009,   /* dest = (NOT dest)*/
        BLACKNESS = 0x00000042,   /* dest = BLACK*/
        WHITENESS = 0x00FF0062,   /* dest = WHITE*/
    };

    public const int TRUE = 1;
    public const int FALSE = 0;


    [DllImport("user32.dll", SetLastError = true)]
    public static extern PVOID GetDCEx(PVOID hwnd, PVOID hrgnclip, uint fdwOptions);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern int ReleaseDC(PVOID hwnd, PVOID hDC_Screen);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern PVOID GetDC(PVOID hWnd);

    [DllImport("gdi32.dll", SetLastError = true)]
    public static extern PVOID CreateDC(string lpszDriver, string lpszDevice, string lpszOutput, PVOID lpInitData);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool GetWindowRect(PVOID hwnd, out RECT lpRect);
    public static RECT GetWindowRect(PVOID hWnd)
    {
        RECT result;
        GetWindowRect(hWnd, out result);
        return result;
    }

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool GetClientRect(PVOID hWnd, out RECT lpRect);
    public static RECT GetClientRect(PVOID hWnd)
    {
        RECT result;
        GetClientRect(hWnd, out result);
        return result;
    }

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool AdjustWindowRect(ref RECT lpRect, uint dwStyle, bool bMenu);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool AdjustWindowRectEx(ref RECT lpRect, uint dwStyle, bool bMenu, uint dwExStyle);


    [DllImport("user32.dll", SetLastError = true)]
    public static extern PVOID SetCapture(PVOID hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern int ReleaseCapture(PVOID hwnd);

    public struct RGNDATA
    {
        public RGNDATAHEADER rdh;
        public PVOID Buffer;
    }

    public struct RGNDATAHEADER
    {
        [MarshalAs(UnmanagedType.U4)]
        public int dwSize;

        [MarshalAs(UnmanagedType.U4)]
        public RegionDataHeaderTypes iType;

        [MarshalAs(UnmanagedType.U4)]
        public int nCount;

        [MarshalAs(UnmanagedType.U4)]
        public int nRgnSize;

        public Rectangle rcBound;

        public static RGNDATAHEADER Create(Rectangle region, int rectangleCount)
        {
            RGNDATAHEADER header = new RGNDATAHEADER();
            header.dwSize = Marshal.SizeOf(typeof(RGNDATAHEADER));
            header.iType = RegionDataHeaderTypes.Rectangles;
            header.rcBound = region;
            header.nCount = rectangleCount;
            return header;
        }
    }

    public enum RegionCombineMode : uint
    {
        And = 1,
        Or = 2,
        Xor = 3,
        Diff = 4,
        Copy = 5,
        Min = And,
        Max = Copy,
    }

    public enum RegionTypes
    {
        Error = 0,
        Null = 1,
        Simple = 2,
        Complex = 3
    }

    public enum RegionDataHeaderTypes : uint
    {
        Rectangles = 1
    }

    [DllImport("gdi32.dll", SetLastError = true)]
    public static extern IntPtr ExtCreateRegion(IntPtr lpXform, [MarshalAs(UnmanagedType.U4)]int nCount, ref RGNDATA lpRgnData);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern int SetWindowRgn(IntPtr hWnd, IntPtr hRgn, bool bRedraw);

    [DllImport("gdi32.dll", SetLastError = true)]
    public static extern IntPtr CreateRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);

    [DllImport("gdi32.dll", SetLastError = true)]
    public static extern RegionTypes CombineRgn(IntPtr hrgnDest, IntPtr hrgnSrc1, IntPtr hrgnSrc2, RegionCombineMode fnCombineMode);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern void DisableProcessWindowsGhosting();

    [DllImport("user32.dll", SetLastError = true)]
    public static extern short GetAsyncKeyState(int nVirtKey);

    public const int VK_LBUTTON = 0x01;
    public const int VK_RBUTTON = 0x02;

    [DllImport("uxtheme.dll", SetLastError = true)]
    public static extern int SetWindowTheme(IntPtr hwnd, string pszSubAppName, string pszSubIdList);

    [DllImport("comctl32.dll", SetLastError = true)]
    public static extern bool _TrackMouseEvent(TRACKMOUSEEVENT tme);

    public static bool TrackMouseEvent(TRACKMOUSEEVENT tme)
    {
        return _TrackMouseEvent(tme);
    }

    public static int GetLastError()
    {
        return Marshal.GetLastWin32Error();
    }

    [DllImport("gdi32.dll", SetLastError = true)]
    public static extern IntPtr CreateCompatibleDC(IntPtr hDC_Screen);

    [DllImport("gdi32.dll", SetLastError = true)]
    public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC_Screen, int nWidth, int nHeight);

    [DllImport("gdi32.dll", SetLastError = true)]
    public static extern IntPtr SelectObject(IntPtr hDC_Screen, IntPtr hObject);

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
    [DllImport("gdi32.dll", SetLastError = true)]
    public static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest, int nWidth,
        int nHeight, IntPtr hObjSource, int nXSrc, int nYSrc, TernaryRasterOperations dwRop);

    [DllImport("gdi32.dll", SetLastError = true)]
    static extern bool TextOut(IntPtr hdc, int nXStart, int nYStart,
        string lpString, int cbString);

    [DllImport("gdi32.dll", SetLastError = true)]
    static extern bool GetTextExtentPoint(IntPtr hdc, string lpString,
        int cbString, ref Size lpSize);

    [DllImport("gdi32.dll", EntryPoint = "GetTextExtentPointW", SetLastError = true)]
    public static extern bool GetTextExtentPointW(IntPtr hdc,
        [MarshalAs(UnmanagedType.LPWStr)]string lpString,
        int cbString, ref Size lpSize);

    public enum TextAlignTypes : int
    {
        TA_NOUPDATECP = 0,
        TA_UPDATECP = 1,

        TA_LEFT = 0,
        TA_RIGHT = 2,
        TA_CENTER = 6,

        TA_TOP = 0,
        TA_BOTTOM = 8,
        TA_BASELINE = 24,
        TA_RTLREADING = 256,
        TA_MASK = (TA_BASELINE + TA_CENTER + TA_UPDATECP + TA_RTLREADING)
    }

    public enum VTextAlignTypes : int
    {
        // These are used with the text layout is vertical
        VTA_BASELINE = TextAlignTypes.TA_BASELINE,
        VTA_LEFT = TextAlignTypes.TA_BOTTOM,
        VTA_RIGHT = TextAlignTypes.TA_TOP,
        VTA_CENTER = TextAlignTypes.TA_CENTER,
        VTA_BOTTOM = TextAlignTypes.TA_RIGHT,
        VTA_TOP = TextAlignTypes.TA_LEFT
    }

    [DllImport("gdi32.dll", SetLastError = true)]
    public static extern bool SetTextAlign(IntPtr hdc, uint fmode);

    [StructLayout(LayoutKind.Sequential)]
    public struct COLORREF
    {
        public byte R;
        public byte G;
        public byte B;

        public COLORREF(Color colorIn)
        {
            R = colorIn.R;
            G = colorIn.G;
            B = colorIn.B;
        }
        public COLORREF(byte red, byte green, byte blue)
        {
            R = red;
            G = green;
            B = blue;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RGB
    {
        byte byRed, byGreen, byBlue, RESERVED;

        public RGB(Color colorIn)
        {
            byRed = colorIn.R;
            byGreen = colorIn.G;
            byBlue = colorIn.B;
            RESERVED = 0;
        }
        public RGB(byte red, byte green, byte blue)
        {
            byRed = red;
            byGreen = green;
            byBlue = blue;
            RESERVED = 0;
        }

        public Int32 ToInt32()
        {
            byte[] RGBCOLORS = new byte[4];
            RGBCOLORS[0] = byRed;
            RGBCOLORS[1] = byGreen;
            RGBCOLORS[2] = byBlue;
            RGBCOLORS[3] = RESERVED;
            return BitConverter.ToInt32(RGBCOLORS, 0);
        }
    }

    public static UInt32 GetRGB(byte r, byte g, byte b)
    {
        return ((UInt32)(r | ((UInt16)g << 8)) | (((UInt32)b << 16)));
    }

    [DllImport("gdi32.dll", SetLastError = true)]
    public static extern uint SetTextColor(IntPtr hdc, uint crColor);

    [DllImport("gdi32.dll", SetLastError = true)]
    public static extern uint SetBkColor(IntPtr hdc, uint crColor);

    public enum BkModeTypes : int
    {
        TRANSPARENT = 1,
        OPAQUE = 2
    }

    [DllImport("gdi32.dll", SetLastError = true)]
    public static extern int SetBkMode(IntPtr hdc, int iBkMode);

    [Flags]
    public enum ETOOptions : uint
    {
        ETO_CLIPPED = 0x4,
        ETO_GLYPH_INDEX = 0x10,
        ETO_IGNORELANGUAGE = 0x1000,
        ETO_NUMERICSLATIN = 0x800,
        ETO_NUMERICSLOCAL = 0x400,
        ETO_OPAQUE = 0x2,
        ETO_PDY = 0x2000,
        ETO_RTLREADING = 0x800,
    }

    [DllImport("gdi32.dll", SetLastError = true)]
    public static extern bool ExtTextOut(IntPtr hdc, int X, int Y, uint fuOptions,
        [In] ref RECT lprc, string lpString, uint cbCount, [In] int[] lpDx);

    [DllImport("gdi32.dll", EntryPoint = "TextOutW", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern bool TextOutW(IntPtr hdc, int nXStart, int nYStart, string lpString, int cbString);

    [DllImport("gdi32.dll", EntryPoint = "TextOutW", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern bool TextOutW(IntPtr hdc, int nXStart, int nYStart, StringBuilder lpString, int cbString);

    [DllImport("gdi32.dll", EntryPoint = "TextOutW", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern bool TextOutW(IntPtr hdc, int nXStart, int nYStart,
        //[MarshalAs(UnmanagedType.LPWStr)]
        char* lpString,
        int cbString);

    [DllImport("gdi32.dll", EntryPoint = "ExtTextOutW", SetLastError = true)]
    public static extern bool ExtTextOutW(IntPtr hdc, int X, int Y, uint fuOptions,
        [In] ref RECT lprc,
        [MarshalAs(UnmanagedType.LPWStr)] string lpString,
        uint cbCount, int[] lpDx);



    [DllImport("gdi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern bool GetTextMetrics(IntPtr hdc, out TEXTMETRIC lptm);

    [DllImport("gdi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern bool GetTextExtentPoint32W(IntPtr hdc, char* lpString, int cbString, out Size lpSize);

    [DllImport("gdi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern bool GetTextExtentPoint32W(IntPtr hdc, string lpString, int cbString, out Size lpSize);

    [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern IntPtr CreateFont(
        int nHeight,
        int nWidth,
        int nEscapement,
        int nOrientation,
        FontWeight fnWeight,
        uint fdwItalic,
        uint fdwUnderline,
        uint fdwStrikeOut,
        FontCharSet fdwCharSet,
        FontPrecision fdwOutputPrecision,
        FontClipPrecision fdwClipPrecision,
        FontQuality fdwQuality,
        FontPitchAndFamily fdwPitchAndFamily,
        string lpszFace);

    [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern IntPtr CreateFontIndirect([In, MarshalAs(UnmanagedType.LPStruct)] LOGFONT lplf);

    //[DllImport("kernel32.dll", SetLastError = true)]
    //static extern bool GetCPInfo(uint CodePage, out CPINFO lpCPInfo);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern int MultiByteToWideChar(uint CodePage, uint dwFlags, string
        lpMultiByteStr, int cbMultiByte, [Out, MarshalAs(UnmanagedType.LPWStr)]
    StringBuilder lpWideCharStr, int cchWideChar);

    [DllImport("gdi32.dll", SetLastError = true)]
    public static extern bool DeleteObject(PVOID hObject);

    [DllImport("gdi32.dll", SetLastError = true)]
    public static extern bool DeleteDC(PVOID hDC_Screen);

    #region AppBarInfo

    [StructLayout(LayoutKind.Sequential)]
    public struct APPBARDATA
    {
        public UInt32 cbSize;
        public IntPtr hWnd;
        public UInt32 uCallbackMessage;
        public UInt32 uEdge;
        public RECT rc;
        public Int32 lParam;
    }

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("shell32.dll", SetLastError = true)]
    public static extern int SHAppBarMessage(int dwMessage, ref APPBARDATA data);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern int SystemParametersInfo(uint uiAction, uint uiParam, void* pvParam, uint fWinIni);


    public class AppBarInfo
    {
        private APPBARDATA m_data;

        // Appbar messages
        private const int ABM_NEW = 0x00000000;
        private const int ABM_REMOVE = 0x00000001;
        private const int ABM_QUERYPOS = 0x00000002;
        private const int ABM_SETPOS = 0x00000003;
        private const int ABM_GETSTATE = 0x00000004;
        private const int ABM_GETTASKBARPOS = 0x00000005;
        private const int ABM_ACTIVATE = 0x00000006;  // lParam == TRUE/FALSE means activate/deactivate
        private const int ABM_GETAUTOHIDEBAR = 0x00000007;
        private const int ABM_SETAUTOHIDEBAR = 0x00000008;

        // Appbar edge constants
        private const int ABE_LEFT = 0;
        private const int ABE_TOP = 1;
        private const int ABE_RIGHT = 2;
        private const int ABE_BOTTOM = 3;

        // SystemParametersInfo constants
        private const UInt32 SPI_GETWORKAREA = 0x0030;

        public enum ScreenEdge
        {
            Undefined = -1,
            Left = ABE_LEFT,
            Top = ABE_TOP,
            Right = ABE_RIGHT,
            Bottom = ABE_BOTTOM
        }

        public ScreenEdge Edge
        {
            get
            {
                return (ScreenEdge)m_data.uEdge;
            }
        }

        public Rectangle WorkArea
        {
            get
            {
                int bResult = 0;
                RECT rc = RECT.Empty;
                bResult = SystemParametersInfo(SPI_GETWORKAREA, 0, &rc, 0);

                if (bResult == 1)
                    return rc;
                else
                    return RECT.Empty;
            }
        }

        public void GetPosition(string strClassName, string strWindowName)
        {
            m_data = new APPBARDATA();
            m_data.cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(m_data.GetType());

            IntPtr hWnd = FindWindow(strClassName, strWindowName);

            if (hWnd != IntPtr.Zero)
            {
                int uResult = SHAppBarMessage(ABM_GETTASKBARPOS, ref m_data);

                if (uResult == 1)
                {
                }
                else
                {
                    throw new Exception("Failed to communicate with the given AppBar");
                }
            }
            else
            {
                throw new Exception("Failed to find an AppBar that matched the given criteria");
            }
        }

        public void GetSystemTaskBarPosition()
        {
            GetPosition("Shell_TrayWnd", null);
        }
    }

    #endregion

    #region NAMED_PIPES

    //
    // Define the dwOpenMode values for CreateNamedPipe
    //
    public const int PIPE_ACCESS_INBOUND = 0x00000001;
    public const int PIPE_ACCESS_OUTBOUND = 0x00000002;
    public const int PIPE_ACCESS_DUPLEX = 0x00000003;

    //
    // Define the Named Pipe End flags for GetNamedPipeInfo
    //

    public const int PIPE_CLIENT_END = 0x00000000;
    public const int PIPE_SERVER_END = 0x00000001;

    //
    // Define the dwPipeMode values for CreateNamedPipe
    //

    public const int PIPE_WAIT = 0x00000000;
    public const int PIPE_NOWAIT = 0x00000001;
    public const int PIPE_READMODE_BYTE = 0x00000000;
    public const int PIPE_READMODE_MESSAGE = 0x00000002;
    public const int PIPE_TYPE_BYTE = 0x00000000;
    public const int PIPE_TYPE_MESSAGE = 0x00000004;

    //
    // Define the well known values for CreateNamedPipe nMaxInstances
    //

    public const int PIPE_UNLIMITED_INSTANCES = 255;

    public const uint NMPWAIT_WAIT_FOREVER = 0xffffffff;
    public const uint NMPWAIT_NOWAIT = 0x00000001;
    public const uint NMPWAIT_USE_DEFAULT_WAIT = 0x00000000;

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern uint CreateNamedPipe(
        string pipeName,
        uint dwOpenMode,
        uint dwPipeMode,
        uint nMaxInstances,
        uint nOutBufferSize,
        uint nInBufferSize,
        uint nDefaultTimeOut,
        IntPtr lpSecurityAttributes);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern int ConnectNamedPipe(
        SafeFileHandle hNamedPipe,
        IntPtr lpOverlapped);

    public const uint PIPE_DUPLEX = (0x00000003);
    //public const uint FILE_FLAG_OVERLAPPED = (0x40000000);
    //public const uint GENERIC_READ = 0x80000000;
    //public const uint GENERIC_WRITE = 0x40000000;
    //public const uint GENERIC_EXECUTE = 0x20000000;
    //public const uint GENERIC_ALL = 0x10000000;
    //public const uint OPEN_EXISTING = 0x00000003;
    //public const uint FILE_ATTRIBUTE_NORMAL = 0x00000080;


    //[DllImport("kernel32.dll", SetLastError = true)]
    //public static extern SafeFileHandle CreateFile(
    //    String pipeName,
    //    uint dwDesiredAccess,
    //    uint dwShareMode,
    //    IntPtr lpSecurityAttributes,
    //    uint dwCreationDisposition,
    //    uint dwFlagsAndAttributes,
    //    IntPtr hTemplate);

    #endregion

    #region FILES

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern int WriteFile(uint handle, byte* buffer, int numBytesToWrite, int* numBytesWritten, NativeOverlapped* lpOverlapped);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern int ReadFile(
        uint hFile,
        byte* lpBuffer,
        int nNumberOfBytesToRead,
        [Out][Optional] int* lpNumberOfBytesRead,
        [Out][Optional] NativeOverlapped* lpOverlapped
        );

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern uint CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr SecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile
            );

    #endregion
}

/*
remove close (X) button from window

using System.Runtime.InteropServices;
Declare the following as class level variable
const int MF_BYPOSITION = 0x400;
[DllImport("User32")]
public static extern int RemoveMenu(IntPtr hMenu, int nPosition, int wFlags);
[DllImport("User32")]
public static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
[DllImport("User32")]
public static extern int GetMenuItemCount(IntPtr hWnd);
In the Form_Load() event, write the following code:
private void Form1_Load(object sender, EventArgs e)
{
        IntPtr hMenu = GetSystemMenu(this.Handle, false);
        int menuItemCount = GetMenuItemCount(hMenu);
        RemoveMenu(hMenu, menuItemCount - 1, MF_BYPOSITION);
}
*/
