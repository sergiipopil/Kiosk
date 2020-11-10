namespace KioskBrains.Common.Logging
{
    public delegate void LogWriteDelegate(LogTypeEnum type, LogContextEnum context, string message, object additionalData);
}