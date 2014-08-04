using System.Drawing;
using System.Runtime.InteropServices;

namespace XrConsoleProject
{
    public static partial class WinAPI
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left, Top, Right, Bottom;

            public static readonly RECT Empty = new RECT(0, 0, 0, 0);

            public RECT(int left, int top, int right, int bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }

            public int Height
            {
                get { return Bottom - Top; }
            }

            public int Width
            {
                get { return Right - Left; }
            }

            public Size Size
            {
                get { return new Size(Right - Left, Bottom - Top); }
            }

            public Point Location
            {
                get { return new Point(Left, Top); }
            }

            public static implicit operator Rectangle(RECT src)
            {
                return new Rectangle
                    (
                    src.Left,
                    src.Top,
                    src.Right - src.Left,
                    src.Bottom - src.Top
                    );
            }

            public static implicit operator RECT(Rectangle src)
            {
                return new RECT(src.Left, src.Top, src.Right, src.Bottom);
            }

            public static RECT FromXYWH(int x, int y, int width, int height)
            {
                return new RECT(x, y, x + width, y + height);
            }

            public static RECT FromSize(Size size)
            {
                return new RECT(0, 0, size.Width, size.Height);
            }
        }
    }
}
