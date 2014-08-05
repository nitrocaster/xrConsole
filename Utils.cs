using System;
using System.Drawing.Text;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace xr
{
    // already exist in System
    //public delegate TResult Func<out TResult>();
    public delegate TResult Func<out TResult, in T>(T arg);
    public delegate TResult Func<out TResult, in T1, in T2>(T1 arg1, T2 arg2);
    public delegate TResult Func<out TResult, in T1, in T2, in T3>(T1 arg1, T2 arg2, T3 arg3);
    public delegate TResult Func<out TResult, in T1, in T2, in T3, in T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);

    // already exist in System
    //public delegate void Action();
    //public delegate void Action<in T>(T arg);
    public delegate void Action<in T1, in T2>(T1 arg1, T2 arg2);
    public delegate void Action<in T1, in T2, in T3>(T1 arg1, T2 arg2, T3 arg3);
    public delegate void Action<in T1, in T2, in T3, in T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);

    public struct Accessor<T>
    {
        public Accessor(Func<T> getter, Action<T> setter)
            : this()
        {
            Get = getter;
            Set = setter;
        }

        public Func<T> Get { get; private set; }

        public Action<T> Set { get; private set; }
    }

    /// <summary>
    /// Represents [a, b) interval.
    /// </summary>
    [DebuggerDisplay("[{Min}, {Max})")]
    public struct IntRange
    {
        public static readonly IntRange InvalidRange = new IntRange(1, 0);

        public int Max;
        public int Min;

        public IntRange(int min, int max)
        {
            Min = min;
            Max = max;
        }

        public bool IsValid
        {
            get { return Size >= 0; }
        }

        public int Size
        {
            get { return Max - Min; }
        }

        public bool IsEmpty
        {
            get { return Size == 0; }
        }

        public bool Contains(int arg)
        {
            return Min <= arg && arg < Max;
        }

        public static IntRange Intersect(IntRange first, IntRange other)
        {
            var testMin = other.Contains(first.Min);
            var testMax = other.Contains(first.Max);
            if (testMin && testMax)
            {
                return first;
            }
            if (testMin)
            {
                return new IntRange(first.Min, other.Max);
            }
            if (testMax)
            {
                return new IntRange(other.Min, first.Max);
            }
            return InvalidRange;
        }

        public IntRange Intersect(IntRange other)
        {
            return Intersect(this, other);
        }

        public static bool operator ==(IntRange a, IntRange b)
        {
            return a.Max == b.Max && a.Min == b.Min;
        }

        public static bool operator !=(IntRange a, IntRange b)
        {
            return a.Max != b.Max || a.Min != b.Min;
        }
    }
}

public static class Utils
{
    public static readonly char[] WhitespaceChars = { ' ', '\t' };

    public static Thread CreateThread(ThreadStart target, string name,
        bool background = true, bool suspended = false)
    {
        var trd = new Thread(target)
        {
            Name = name,
            IsBackground = background,
            CurrentCulture = CultureInfo.InvariantCulture
        };
        if (!suspended)
        {
            trd.Start();
        }
        return trd;
    }

    public static Thread CreateThread(ParameterizedThreadStart target, object arg, string name,
        bool background = true, bool suspended = false)
    {
        var trd = new Thread(target)
        {
            Name = name,
            IsBackground = background,
            CurrentCulture = CultureInfo.InvariantCulture
        };
        if (!suspended)
        {
            trd.Start(arg);
        }
        return trd;
    }

    public static string GetFirstArg(this string s)
    {
        var iSpace = s.IndexOfAny(WhitespaceChars);
        if (iSpace == -1)
        {
            return null;
        }
        return s.Substring(0, iSpace);
    }

    public static bool IsFontInstalled(string name)
    {
        using (var fonts = new InstalledFontCollection())
        {
            return fonts.Families.Any(x => x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}
