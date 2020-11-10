using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using KioskBrains.Common.Contracts;
using KioskBrains.Kiosk.Core.Devices.Exceptions;

namespace KioskBrains.Kiosk.Core.Devices.Common
{
    public class DeviceIoPortDriver : IDisposable
    {
        private readonly IDeviceIoPortProvider _deviceIoPortProvider;

        public DeviceIoPortDriver(IDeviceIoPortProvider deviceIoPortProvider)
        {
            Assure.ArgumentNotNull(deviceIoPortProvider, nameof(deviceIoPortProvider));

            _deviceIoPortProvider = deviceIoPortProvider;
        }

        private IDeviceIoPort _deviceIoPort;

        public bool IsOpened { get; private set; }

        public Task OpenAsync(TimeSpan timeout)
        {
            try
            {
                using (var timeoutCancellationSource = new CancellationTokenSource(timeout))
                {
                    return OpenAsync(timeoutCancellationSource.Token);
                }
            }
            catch (OperationCanceledException)
            {
                throw new DeviceIoPortTimeoutException();
            }
        }

        public async Task OpenAsync(CancellationToken? cancellationToken = null)
        {
            _deviceIoPort = await _deviceIoPortProvider.OpenDeviceIoPortAsync(cancellationToken ?? CancellationToken.None);
            IsOpened = true;
        }

        public void Dispose()
        {
            IsOpened = false;
            _deviceIoPort?.Dispose();
        }

        private void EnsureOpened()
        {
            if (!IsOpened)
            {
                throw new InvalidFlowStateException($"'{nameof(DeviceIoPortDriver)}' is not opened.");
            }
        }

        public async Task<byte[]> ReadAsync(ResponseReadingFinalizer responseReadingFinalizer, TimeSpan timeout)
        {
            using (var timeoutCancellationTokenSource = new CancellationTokenSource(timeout))
            {
                return await ReadAsync(responseReadingFinalizer, timeoutCancellationTokenSource.Token, null);
            }
        }

        public async Task<byte[]> ReadAsync(ResponseReadingFinalizer responseReadingFinalizer, CancellationToken externalCancellationToken)
        {
            return await ReadAsync(responseReadingFinalizer, null, externalCancellationToken);
        }

        public async Task<byte[]> ReadAsync(ResponseReadingFinalizer responseReadingFinalizer, TimeSpan timeout, CancellationToken externalCancellationToken)
        {
            if (timeout == TimeSpan.Zero)
            {
                return await ReadAsync(responseReadingFinalizer, null, externalCancellationToken);
            }
            else
            {
                using (var timeoutCancellationTokenSource = new CancellationTokenSource(timeout))
                {
                    return await ReadAsync(responseReadingFinalizer, timeoutCancellationTokenSource.Token, externalCancellationToken);
                }
            }
        }

        private async Task<byte[]> ReadAsync(ResponseReadingFinalizer responseReadingFinalizer, CancellationToken? timeoutCancellationToken, CancellationToken? externalCancellationToken)
        {
            EnsureOpened();

            Assure.ArgumentNotNull(responseReadingFinalizer, nameof(responseReadingFinalizer));
            if (timeoutCancellationToken == null && externalCancellationToken == null)
            {
                throw new ArgumentException($"At least either '{nameof(timeoutCancellationToken)}' or '{nameof(externalCancellationToken)}' should be passed.");
            }

            CancellationTokenSource linkedTokenSource = null;
            CancellationToken? readContinuationTimeoutCancellationToken = null;
            try
            {
                try
                {
                    CancellationToken cancellationToken;
                    if (timeoutCancellationToken != null && externalCancellationToken != null)
                    {
                        linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutCancellationToken.Value, externalCancellationToken.Value);
                        cancellationToken = linkedTokenSource.Token;
                    }
                    else
                    {
                        cancellationToken = timeoutCancellationToken ?? externalCancellationToken.Value;
                    }

                    var responseBytes = new List<byte>();
                    var isFirstRead = true;
                    // ReSharper disable LoopVariableIsNeverChangedInsideLoop
                    do
                    {
                        const int readBufferSize = 256;
                        byte[] readBytes;

                        if (isFirstRead)
                        {
                            isFirstRead = false;

                            // wait with general cancellation token
                            readBytes = await _deviceIoPort.ReadAsync(readBufferSize, InputStreamOptions.Partial, cancellationToken);
                        }
                        else
                        {
                            // after first bytes are read, wait max 1s for the following ones
                            using (var readContinuationTimeoutCancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(1)))
                            {
                                readContinuationTimeoutCancellationToken = readContinuationTimeoutCancellationSource.Token;
                                using (var linkedContinuationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, readContinuationTimeoutCancellationToken.Value))
                                {
                                    readBytes = await _deviceIoPort.ReadAsync(readBufferSize, InputStreamOptions.Partial, linkedContinuationTokenSource.Token);
                                }
                            }
                        }

                        responseBytes.AddRange(readBytes);
                    }
                    while (!responseReadingFinalizer(responseBytes));
                    // ReSharper restore LoopVariableIsNeverChangedInsideLoop

                    return responseBytes.ToArray();
                }
                catch (OperationCanceledException)
                {
                    // rethrow specific token exception (for case of linked sources)
                    externalCancellationToken?.ThrowIfCancellationRequested();
                    timeoutCancellationToken?.ThrowIfCancellationRequested();
                    readContinuationTimeoutCancellationToken?.ThrowIfCancellationRequested();

                    throw new InvalidFlowStateException($"'{nameof(ReadAsync)}' - at least one of cancellation tokens should trigger exception.");
                }
            }
            catch (OperationCanceledException ex)
            {
                if (ex.CancellationToken == timeoutCancellationToken)
                {
                    throw new DeviceIoPortTimeoutException();
                }
                else if (ex.CancellationToken == readContinuationTimeoutCancellationToken)
                {
                    throw new DeviceIoPortTimeoutException(partialRead: true);
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                linkedTokenSource?.Dispose();
            }
        }

        public async Task WriteAsync(byte[] messageBytes, CancellationToken? externalCancellationToken)
        {
            EnsureOpened();

            Assure.ArgumentNotNull(messageBytes, nameof(messageBytes));
            Assure.CheckFlowState(messageBytes.Length > 0, $"Passed '{nameof(messageBytes)}' is empty.");

            await _deviceIoPort.WriteAsync(messageBytes, externalCancellationToken);
        }
    }
}