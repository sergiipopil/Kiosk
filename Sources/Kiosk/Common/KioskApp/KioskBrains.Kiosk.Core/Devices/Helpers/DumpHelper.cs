using System.Text;

namespace KioskBrains.Kiosk.Core.Devices.Helpers
{
    public static class DumpHelper
    {
        public static string DumpMessage(byte[] messageBuffer)
        {
            if (messageBuffer == null)
            {
                return "null";
            }

            return DumpMessage(messageBuffer, messageBuffer.Length);
        }

        public static string DumpMessage(byte[] messageBuffer, int messageLength)
        {
            if (messageBuffer == null)
            {
                return "null";
            }

            var messageDumpBuilder = new StringBuilder();
            for (var i = 0; i < messageLength; i++)
            {
                messageDumpBuilder.Append(messageBuffer[i].ToString("X2"));
                messageDumpBuilder.Append(" ");
            }
            return messageDumpBuilder.ToString().Trim();
        }

        public static string ToDumpMessage(this byte[] messageBuffer)
        {
            return DumpMessage(messageBuffer);
        }
    }
}