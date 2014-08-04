using System;
using System.Globalization;

namespace XrConsoleProject
{
    public static class XrConverter
    {
        public static string ToString(bool b)
        {
            return b ? "1" : "0";
        }

        public static bool ParseBool(string a)
        {
            if (a == "1" || a == "on")
            {
                return true;
            }
            if (a == "0" || a == "off")
            {
                return false;
            }
            throw new ArgumentException();
        }

        public static bool ParseBool(string a, out bool result)
        {
            if (a == "1" || a == "on")
            {
                result = true;
                return true;
            }
            if (a == "0" || a == "off")
            {
                result = false;
                return true;
            }
            result = false;
            return false;
        }

        public static string ToString(int i)
        {
            return i.ToString(CultureInfo.InvariantCulture);
        }

        public static int ParseInt32(string a)
        {
            return Int32.Parse(a, NumberStyles.Integer, CultureInfo.InvariantCulture);
        }

        public static bool ParseInt32(string a, out int result)
        {
            return Int32.TryParse(a, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
        }

        public static string ToString(long i)
        {
            return i.ToString(CultureInfo.InvariantCulture);
        }

        public static long ParseInt64(string a)
        {
            return Int64.Parse(a, NumberStyles.Integer, CultureInfo.InvariantCulture);
        }

        public static bool ParseInt64(string a, out long result)
        {
            return Int64.TryParse(a, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
        }

        public static string ToString(float f)
        {
            return f.ToString("#0.000###", CultureInfo.InvariantCulture);
        }

        public static float ParseSingle(string a)
        {
            return Single.Parse(a, NumberStyles.Float, CultureInfo.InvariantCulture);
        }

        public static bool ParseSingle(string a, out float result)
        {
            return Single.TryParse(a, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
        }

        public static string ToString(double d)
        {
            return d.ToString("#0.000000", CultureInfo.InvariantCulture);
        }

        public static double ParseDouble(string a)
        {
            return Double.Parse(a, NumberStyles.Float, CultureInfo.InvariantCulture);
        }

        public static bool ParseDouble(string a, out double result)
        {
            return Double.TryParse(a, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
        }
    }
}
