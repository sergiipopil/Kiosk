namespace KioskBrains.Kiosk.Helpers.Numbers
{
    public static class NumberPartHelper
    {
        public static short GetShortFromBytesLittleEndian(byte byte0, byte byte1)
        {
            return (short)((byte1 << 8) + byte0);
        }

        public static int GetIntegerFromBytesLittleEndian(byte byte0, byte byte1, byte byte2, byte byte3)
        {
            return (byte3 << 24) + (byte2 << 16) + (byte1 << 8) + byte0;
        }
    }
}