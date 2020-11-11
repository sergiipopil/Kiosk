namespace KioskBrains.Server.Common.Log
{
    public static class LoggingEvents
    {
        public const int Api_BadRequest = 400;

        public const int Api_Unauthorized = 401;

        public const int Api_Forbidden = 403;

        public const int Api_NotFound = 404;

        public const int Api_MethodNotAllowed = 405;

        public const int Api_InternalServerError = 500;

        public const int UnhandledException = 1_000;

        public const int NotSupported = 1_001;

        public const int DataNotFound = 1_002;

        public const int DataProcessingError = 1_003;

        public const int NotificationError = 1_004;

        public const int ResponseProcessingError = 1_005;

        public const int ExternalApiError = 1_006;

        public const int Synchronization = 2_000;
    }
}
