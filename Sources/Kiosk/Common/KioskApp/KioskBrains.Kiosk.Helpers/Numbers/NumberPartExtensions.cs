using System;

namespace KioskBrains.Kiosk.Helpers.Numbers
{
    public static class NumberPartExtensions
    {
        public static byte GetByte(this int value, int byteNumber)
        {
            return (byte)((value >> (8*byteNumber)) & 0xff);
        }

        public static byte GetByte(this uint value, int byteNumber)
        {
            return (byte)((value >> (8*byteNumber)) & 0xff);
        }

        public static byte GetByte(this ushort value, int byteNumber)
        {
            return (byte)((value >> (8*byteNumber)) & 0xff);
        }

        public static decimal GetFractionalPart(this decimal value)
        {
            return value - Math.Truncate(value);
        }
    }
}