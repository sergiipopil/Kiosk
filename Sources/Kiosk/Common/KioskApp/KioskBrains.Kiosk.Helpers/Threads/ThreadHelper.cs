using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Core;
using KioskBrains.Common.Contracts;

namespace KioskBrains.Kiosk.Helpers.Threads
{
    public static class ThreadHelper
    {
        private static CoreDispatcher _dispatcher;

        public static void Initialize(CoreDispatcher dispatcher)
        {
            Assure.ArgumentNotNull(dispatcher, nameof(dispatcher));
            _dispatcher = dispatcher;
        }

        private static CoreDispatcher GetCoreDispatcher()
        {
            if (_dispatcher == null)
            {
                throw new InvalidOperationException($"Dispatcher is not initialized. Run '{nameof(ThreadHelper)}.{nameof(Initialize)}' first.");
            }
            return _dispatcher;
        }

        private static bool IsUiThread => GetCoreDispatcher().HasThreadAccess;

        public static void EnsureUiThread()
        {
            if (!IsUiThread)
            {
                throw new InvalidOperationException("UI thread is expected.");
            }
        }

        public static Task RunInUiThreadAsync(Action code, CancellationToken? cancellationToken = null)
        {
            Assure.ArgumentNotNull(code, nameof(code));

            var dispatcher = GetCoreDispatcher();
            if (dispatcher.HasThreadAccess)
            {
                code();
                return Task.CompletedTask;
            }

            return dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { code(); })
                .AsTask(cancellationToken ?? CancellationToken.None);
        }

        public static async Task<TResult> GetFromUiThreadAsync<TResult>(Func<TResult> creator, CancellationToken? cancellationToken = null)
        {
            Assure.ArgumentNotNull(creator, nameof(creator));
            var result = default(TResult);
            await RunInUiThreadAsync(() => result = creator(), cancellationToken);
            return result;
        }

        public static Task<TResult> RunInBackgroundThreadAsync<TResult>(Func<Task<TResult>> code)
        {
            Assure.ArgumentNotNull(code, nameof(code));

            return !IsUiThread ? code() : Task.Run(code);
        }

        public static Task RunInBackgroundThreadAsync(Func<Task> code)
        {
            Assure.ArgumentNotNull(code, nameof(code));

            return !IsUiThread ? code() : Task.Run(code);
        }

        public static Task<TResult> RunInNewThreadAsync<TResult>(Func<Task<TResult>> code)
        {
            Assure.ArgumentNotNull(code, nameof(code));

            return Task.Run(code);
        }

        public static Task RunInNewThreadAsync(Func<Task> code)
        {
            Assure.ArgumentNotNull(code, nameof(code));

            return Task.Run(code);
        }
    }
}