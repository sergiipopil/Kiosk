namespace KioskBrains.Kiosk.Helpers.Numbers
{
    public static class MaskHelper
    {
        public static bool AreBitsSet(byte mask, byte bits)
        {
            return (mask & bits) == bits;
        }
    }
}