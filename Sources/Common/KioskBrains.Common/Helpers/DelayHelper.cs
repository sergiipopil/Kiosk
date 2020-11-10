using System;
using System.Threading.Tasks;

namespace KioskBrains.Common.Helpers
{
    /// <summary>
    /// todo: find a way to sync with file writers directly. Global cancellation token?
    /// </summary>
    public static class DelayHelper
    {
        public static async Task WaitForAsyncFileIoFinished()
        {
            await Task.Delay(TimeSpan.FromSeconds(3));
        }
    }
}