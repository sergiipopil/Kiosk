using System.Threading;
using System.Threading.Tasks;

namespace KioskBrains.Kiosk.Helpers.Threads
{
    public static class CancellationTokenExtensions
    {
        public static Task AsTask(this CancellationToken cancellationToken)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();
            cancellationToken.Register(
                () => { taskCompletionSource.SetCanceled(); },
                useSynchronizationContext: false);
            return taskCompletionSource.Task;
        }
    }
}