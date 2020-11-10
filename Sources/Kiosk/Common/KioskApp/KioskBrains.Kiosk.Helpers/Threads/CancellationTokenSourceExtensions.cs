using System;
using System.Threading;
using KioskBrains.Common.Logging;

namespace KioskBrains.Kiosk.Helpers.Threads
{
    public static class CancellationTokenSourceExtensions
    {
        public static void CancelAndDisposeSafe(this CancellationTokenSource cancellationTokenSource)
        {
            try
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
            }
            catch (Exception ex)
            {
                Log.Error(LogContextEnum.Application, "Cancelling of cancellation token source failed.", ex);
            }
        }

        public static void DisposeSafe(this CancellationTokenSource cancellationTokenSource)
        {
            try
            {
                cancellationTokenSource.Dispose();
            }
            catch (Exception ex)
            {
                Log.Error(LogContextEnum.Application, "Disposing of cancellation token source failed.", ex);
            }
        }
    }
}