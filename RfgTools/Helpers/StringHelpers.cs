using System;

namespace RfgTools.Helpers
{
    //Todo: Change all these to use TryParse, and not just thrown an exception on failure. Error handling is nice
    public static class StringHelpers
    {
        public static uint ToUint32(this string input)
        {
            return  UInt32.Parse(input);
        }

        public static ushort ToUint16(this string input)
        {
            return UInt16.Parse(input);
        }

        public static int ToInt32(this string input)
        {
            return Int32.Parse(input);
        }

        public static short ToInt16(this string input)
        {
            return Int16.Parse(input);
        }

        public static float ToSingle(this string input)
        {
            return Single.Parse(input);
        }

        public static double ToDouble(this string input)
        {
            return Double.Parse(input);
        }

        public static bool ToBool(this string input)
        {
            return Boolean.Parse(input);
        }

        public static char ToChar(this string input)
        {
            return Char.Parse(input);
        }

        public static byte ToByte(this string input)
        {
            return Byte.Parse(input);
        }
    }
}
