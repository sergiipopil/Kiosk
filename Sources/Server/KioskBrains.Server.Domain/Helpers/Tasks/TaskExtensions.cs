using System.Threading.Tasks;

namespace KioskBrains.Server.Domain.Helpers.Tasks
{
    public static class TaskExtensions
    {
        /// <summary>
        /// Use to handle the exception re-throw from Task.WhenAll 
        /// </summary>
        /// <param name="originalTask">The original task</param>
        /// <returns></returns>
        public static Task WithoutRethrow(this Task originalTask)
        {
            var tcs = new TaskCompletionSource<object>();
            originalTask.ContinueWith(t =>
                {
                    tcs.SetResult(null);
                }, TaskContinuationOptions.ExecuteSynchronously);
            return tcs.Task;
        }
    }
}
