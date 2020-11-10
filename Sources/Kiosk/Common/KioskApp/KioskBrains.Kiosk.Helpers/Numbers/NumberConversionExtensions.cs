using System;

namespace KioskBrains.Kiosk.Helpers.Numbers
{
    public static class NumberConversionExtensions
    {
        public static double ToRadians(this double value)
        {
            return (Math.PI/180)*value;
        }
    }
}