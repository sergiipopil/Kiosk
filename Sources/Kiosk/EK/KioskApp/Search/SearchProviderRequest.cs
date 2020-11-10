using System;
using System.Threading;
using System.Threading.Tasks;
using KioskBrains.Common.Contracts;
using KioskBrains.Common.Logging;
using KioskBrains.Kiosk.Helpers.Threads;

namespace KioskApp.Search
{
    public class SearchProviderRequest
    {
        public delegate Task RequestHandler(DateTime termTime, string term, CancellationToken cancellationToken);

        public SearchProviderRequest(string requestLogName, RequestHandler requestHandler)
        {
            Assure.ArgumentNotNull(requestHandler, nameof(requestHandler));

            _requestLogName = requestLogName;
            _requestHandler = requestHandler;
        }

        private readonly string _requestLogName;

        private readonly RequestHandler _requestHandler;

        private State? _state;

        private CancellationTokenSource _requestCancellationSource;

        private readonly object _stateLocker = new object();

        public void RunRequest(DateTime termTime, string term)
        {
            lock (_stateLocker)
            {
                if (_state != null)
                {
                    return;
                }

                _state = State.Request;
            }

            _requestCancellationSource = new CancellationTokenSource();
            var cancellationToken = _requestCancellationSource.Token;

            // ReSharper disable MethodSupportsCancellation
            Task.Run(async () =>
                // ReSharper restore MethodSupportsCancellation
                {
                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        await _requestHandler(termTime, term, cancellationToken);

                        lock (_stateLocker)
                        {
                            _state = State.Completed;
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        lock (_stateLocker)
                        {
                            _state = State.Canceled;
                        }
                    }
                    catch (Exception ex)
                    {
                        lock (_stateLocker)
                        {
                            _state = State.Faulted;
                        }

                        Log.Error(LogContextEnum.Component, $"Request '{_requestLogName}' for term '{term}' failed.", ex);
                    }
                    finally
                    {
                        _requestCancellationSource.DisposeSafe();
                    }
                });
        }

        public void Cancel()
        {
            lock (_stateLocker)
            {
                if (_state == State.Request)
                {
                    _state = State.CancelRequested;
                    _requestCancellationSource.Cancel();
                }
            }
        }

        public bool IsFinished
        {
            get
            {
                lock (_stateLocker)
                {
                    return _state == null // not run
                           || _state == State.Completed
                           || _state == State.Canceled
                           || _state == State.Faulted;
                }
            }
        }

        private enum State
        {
            Request,
            Completed,
            CancelRequested,
            Canceled,
            Faulted,
        }
    }
}