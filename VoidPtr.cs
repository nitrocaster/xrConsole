//#define VoidPtr_IncludeLogicalOperators
using System.Globalization;

namespace System
{
    /// <summary>
    ///     A platform-specific type that is used to represent a pointer or a handle.
    /// </summary>
    public struct VoidPtr : IFormattable
    {
        public static readonly VoidPtr Invalid = (VoidPtr)(-1);
        public static readonly VoidPtr Null = (VoidPtr)(0);
        public static readonly unsafe int Size = sizeof(void*);
        private IntPtr value;
        
        #region [ logical operators ]
#if VoidPtr_IncludeLogicalOperators
        public static VoidPtr operator &(VoidPtr pv1, VoidPtr pv2)
        {
            if (Size > 4)
            {
                return (VoidPtr)((ulong)pv1 & (ulong)pv2);
            }
            else
            {
                return (VoidPtr)((uint)pv1 & (uint)pv2);
            }
        }

        public static VoidPtr operator |(VoidPtr pv1, VoidPtr pv2)
        {
            if (Size > 4)
            {
                return (VoidPtr)((ulong)pv1 & (ulong)pv2);
            }
            else
            {
                return (VoidPtr)((uint)pv1 & (uint)pv2);
            }
        }

        public static VoidPtr operator ^(VoidPtr pv1, VoidPtr pv2)
        {
            if (Size > 4)
            {
                return (VoidPtr)((ulong)pv1 ^ (ulong)pv2);
            }
            else
            {
                return (VoidPtr)((uint)pv1 ^ (uint)pv2);
            }
        }

        public static VoidPtr operator ~(VoidPtr pv)
        {
            if (Size > 4)
            {
                return (VoidPtr)(~(ulong)pv);
            }
            else
            {
                return (VoidPtr)(~(uint)pv);
            }
        }
#endif
        #endregion

        #region [ add/substract operators ]

        public static unsafe VoidPtr operator +(VoidPtr pv, long arg)
        {
            return (byte*)pv.value + arg;
        }

        public static unsafe VoidPtr operator +(VoidPtr pv, ulong arg)
        {
            return (byte*)pv.value + arg;
        }

        public static VoidPtr operator +(VoidPtr pv, int arg)
        {
            return pv.value + arg;
        }

        public static unsafe VoidPtr operator +(VoidPtr pv, uint arg)
        {
            return (byte*)pv.value + arg;
        }

        public static unsafe VoidPtr operator +(VoidPtr pv, short arg)
        {
            return (byte*)pv.value + arg;
        }

        public static unsafe VoidPtr operator +(VoidPtr pv, ushort arg)
        {
            return (byte*)pv.value + arg;
        }

        public static unsafe VoidPtr operator +(VoidPtr pv, sbyte arg)
        {
            return (byte*)pv.value + arg;
        }

        public static unsafe VoidPtr operator +(VoidPtr pv, byte arg)
        {
            return (byte*)pv.value + arg;
        }

        //

        public static unsafe VoidPtr operator -(VoidPtr pv, long arg)
        {
            return (byte*)pv.value - arg;
        }

        public static unsafe VoidPtr operator -(VoidPtr pv, ulong arg)
        {
            return (byte*)pv.value - arg;
        }

        public static VoidPtr operator -(VoidPtr pv, int arg)
        {
            return pv.value - arg;
        }

        public static unsafe VoidPtr operator -(VoidPtr pv, uint arg)
        {
            return (byte*)pv.value - arg;
        }

        public static unsafe VoidPtr operator -(VoidPtr pv, short arg)
        {
            return (byte*)pv.value - arg;
        }

        public static unsafe VoidPtr operator -(VoidPtr pv, ushort arg)
        {
            return (byte*)pv.value - arg;
        }

        public static unsafe VoidPtr operator -(VoidPtr pv, sbyte arg)
        {
            return (byte*)pv.value - arg;
        }

        public static unsafe VoidPtr operator -(VoidPtr pv, byte arg)
        {
            return (byte*)pv.value - arg;
        }

        #endregion

        #region [ comparison operators ]

        public static implicit operator bool(VoidPtr pv)
        {
            return pv != Null;
        }

        public static bool operator ==(VoidPtr pv1, VoidPtr pv2)
        {
            return pv1.value == pv2.value;
        }

        public static bool operator !=(VoidPtr pv1, VoidPtr pv2)
        {
            return pv1.value != pv2.value;
        }

        public static unsafe bool operator <(VoidPtr pv1, VoidPtr pv2)
        {
            return (void*)pv1.value < (void*)pv2.value;
        }

        public static unsafe bool operator >(VoidPtr pv1, VoidPtr pv2)
        {
            return (void*)pv1.value > (void*)pv2.value;
        }

        public static unsafe bool operator <=(VoidPtr pv1, VoidPtr pv2)
        {
            return (void*)pv1.value <= (void*)pv2.value;
        }

        public static unsafe bool operator >=(VoidPtr pv1, VoidPtr pv2)
        {
            return (void*)pv1.value >= (void*)pv2.value;
        }

        #endregion

        #region [ implicit castings to pointers to basic types ]

        public static implicit operator IntPtr(VoidPtr pv)
        {
            return pv.value;
        }

        public static unsafe implicit operator void*(VoidPtr pv)
        {
            return (void*)pv.value;
        }

        public static unsafe implicit operator long*(VoidPtr pv)
        {
            return (long*)pv.value;
        }

        public static unsafe implicit operator ulong*(VoidPtr pv)
        {
            return (ulong*)pv.value;
        }

        public static unsafe implicit operator int*(VoidPtr pv)
        {
            return (int*)pv.value;
        }

        public static unsafe implicit operator uint*(VoidPtr pv)
        {
            return (uint*)pv.value;
        }

        public static unsafe implicit operator short*(VoidPtr pv)
        {
            return (short*)pv.value;
        }

        public static unsafe implicit operator ushort*(VoidPtr pv)
        {
            return (ushort*)pv.value;
        }

        public static unsafe implicit operator char*(VoidPtr pv)
        {
            return (char*)pv.value;
        }

        public static unsafe implicit operator sbyte*(VoidPtr pv)
        {
            return (sbyte*)pv.value;
        }

        public static unsafe implicit operator byte*(VoidPtr pv)
        {
            return (byte*)pv.value;
        }

        #endregion

        #region [ implicit castings to VoidPtr ]

        public static unsafe implicit operator VoidPtr(void* ptr)
        {
            VoidPtr pv;
            pv.value = (IntPtr)ptr;
            return pv;
        }

        public static implicit operator VoidPtr(IntPtr arg)
        {
            VoidPtr pv;
            pv.value = arg;
            return pv;
        }

        #endregion

        #region [ explicit castings to VoidPtr ]

        public static explicit operator VoidPtr(long arg)
        {
            return unchecked((IntPtr)arg);
        }

        public static explicit operator VoidPtr(ulong arg)
        {
            return unchecked((IntPtr)arg);
        }

        public static explicit operator VoidPtr(int arg)
        {
            return (IntPtr)arg;
        }

        public static unsafe explicit operator VoidPtr(uint arg)
        {
            return new IntPtr((void*)arg);
        }

        public static explicit operator VoidPtr(short arg)
        {
            return (IntPtr)arg;
        }

        public static explicit operator VoidPtr(ushort arg)
        {
            return (IntPtr)arg;
        }

        public static explicit operator VoidPtr(char arg)
        {
            return (IntPtr)arg;
        }

        public static explicit operator VoidPtr(sbyte arg)
        {
            return (IntPtr)arg;
        }

        public static explicit operator VoidPtr(byte arg)
        {
            return (IntPtr)arg;
        }

        #endregion

        #region [ explicit castings to basic types ]

        public static explicit operator long(VoidPtr pv)
        {
            return (long)pv.value;
        }

        public static explicit operator ulong(VoidPtr pv)
        {
            return (ulong)pv.value;
        }

        public static explicit operator int(VoidPtr pv)
        {
            return (int)pv.value;
        }

        public static explicit operator uint(VoidPtr pv)
        {
            return (uint)pv.value;
        }

        public static explicit operator short(VoidPtr pv)
        {
            return (short)pv.value;
        }

        public static explicit operator ushort(VoidPtr pv)
        {
            return (ushort)pv.value;
        }

        public static explicit operator char(VoidPtr pv)
        {
            return (char)pv.value;
        }

        public static explicit operator sbyte(VoidPtr pv)
        {
            return (sbyte)pv.value;
        }

        public static explicit operator byte(VoidPtr pv)
        {
            return (byte)pv.value;
        }

        #endregion

        #region [ Object methods ]

        private const string Format32 = "0x{0:x8}";
        private const string Format64 = "0x{0:x16}";

        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null)
            {
                format = (Size == 4) ? Format32 : Format64;
            }
            return String.Format(formatProvider, format, ((ulong)value));
        }

        public override string ToString()
        {
            return String.Format(CultureInfo.CurrentCulture, (Size == 4) ? Format32 : Format64, (ulong)value);
        }

        public override bool Equals(object obj)
        {
            if (obj is VoidPtr)
            {
                return value == ((VoidPtr)obj).value;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return (int)value;
        }

        #endregion
    }
}
