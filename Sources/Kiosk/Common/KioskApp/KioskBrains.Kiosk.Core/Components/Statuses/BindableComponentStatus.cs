using System;
using System.Text;
using KioskBrains.Common.KioskState;
using KioskBrains.Common.Logging;
using KioskBrains.Kiosk.Helpers.Ui.Binding;

namespace KioskBrains.Kiosk.Core.Components.Statuses
{
    // todo: add status hierarchy component/contract status (when it's needed) - so far the single status is used for component and its contracts
    public class BindableComponentStatus : UiBindableObject
    {
        #region Code

        private ComponentStatusCodeEnum _Code = ComponentStatusCodeEnum.Undefined;

        public ComponentStatusCodeEnum Code
        {
            get => _Code;
            private set
            {
                var previousValue = _Code;
                SetProperty(ref _Code, value);
                if (value != previousValue)
                {
                    try
                    {
                        StatusCodeChanged?.Invoke(this, new ComponentStatusCodeChangedEventArgs(previousValue, value));
                    }
                    catch (Exception ex)
                    {
                        Log.Error(LogContextEnum.Component, $"{nameof(StatusCodeChanged)} handler failed.", ex);
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Handler is invoked is the same thread and it's safe (exceptions are handled).
        /// </summary>
        public event EventHandler<ComponentStatusCodeChangedEventArgs> StatusCodeChanged;

        public bool IsOperational()
        {
            return Code.IsOperational();
        }

        #region Message

        private string _Message = "Not Initialized.";

        public string Message
        {
            get => _Message;
            private set => SetProperty(ref _Message, value);
        }

        #endregion

        public DateTime StatusTime { get; private set; } = DateTime.Now;

        private readonly object _componentStatusLock = new object();

        private ComponentStatusCodeEnum _selfStatusCode = ComponentStatusCodeEnum.Undefined;

        private string _selfStatusMessage;

        public void SetSelfStatus(ComponentStatusCodeEnum statusCode, string statusMessage)
        {
            lock (_componentStatusLock)
            {
                _selfStatusCode = statusCode;
                _selfStatusMessage = statusMessage;
                UpdateStatus();
            }
        }

        /// <summary>
        /// For usage in service scenarios.
        /// </summary>
        public ComponentStatus GetSelfStatus()
        {
            lock (_componentStatusLock)
            {
                return new ComponentStatus()
                    {
                        Code = _selfStatusCode,
                        Message = _selfStatusMessage,
                        StatusLocalTime = StatusTime,
                    };
            }
        }

        private ComponentStatusCodeEnum _dependencyBasedStatusCode = ComponentStatusCodeEnum.Undefined;

        private string _dependencyBasedStatusMessage;

        public void SetDependencyBasedStatus(ComponentStatusCodeEnum statusCode, string statusMessage)
        {
            lock (_componentStatusLock)
            {
                _dependencyBasedStatusCode = statusCode;
                _dependencyBasedStatusMessage = statusMessage;
                UpdateStatus();
            }
        }

        private void UpdateStatus()
        {
            ComponentStatusCodeEnum statusCode;
            string statusMessage;
            // if dependency based status is not set or less severe than self status
            if (_dependencyBasedStatusCode == ComponentStatusCodeEnum.Undefined
                || (int)_dependencyBasedStatusCode < (int)_selfStatusCode)
            {
                // set self status
                statusCode = _selfStatusCode;
                statusMessage = _selfStatusMessage;
            }
            else
            {
                // set dependency based status (with self status inside the message)
                statusCode = _dependencyBasedStatusCode;
                var messageBuilder = new StringBuilder();
                messageBuilder.Append("Dependency Based");
                if (!string.IsNullOrEmpty(_dependencyBasedStatusMessage))
                {
                    messageBuilder.Append($": {_dependencyBasedStatusMessage}");
                }
                messageBuilder.Append($" (Self: {_selfStatusCode}");
                if (!string.IsNullOrEmpty(_selfStatusMessage))
                {
                    messageBuilder.Append($" - {_selfStatusMessage}");
                }
                messageBuilder.Append(")");
                statusMessage = messageBuilder.ToString();
            }
            Code = statusCode;
            StatusTime = DateTime.Now;
            Message = statusCode == ComponentStatusCodeEnum.Error
                      || statusCode == ComponentStatusCodeEnum.Disabled
                // add time to error/disabled message in order to know when the error occurred
                ? $"[{StatusTime:yyyy-MM-dd HH:mm:ss}] {statusMessage}"
                : statusMessage;
        }

        public override string ToString()
        {
            if (Message == null)
            {
                return $"{Code}";
            }
            else
            {
                return $"{Code}, {Message}";
            }
        }
    }
}