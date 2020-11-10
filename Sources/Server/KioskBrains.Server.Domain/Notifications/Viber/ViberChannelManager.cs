using System;
using System.Threading;
using System.Threading.Tasks;
using KioskBrains.Waf.Managers.Common;
using Viber.Bot;

namespace KioskBrains.Server.Domain.Notifications.Viber
{
    public class ViberChannelManager : IWafManager, INotificationChannelManager
    {
        public async Task<NotificationResult> SendMessageAsync(string receiverId, string message, CancellationToken cancellationToken)
        {
            string errorInfo = null;
            try
            {
                // todo: move to config
                const string ViberBotToken = "47c1eb868867d4e5-aad8db5a22f2dd7e-74c0ab55cba422c1";
                using (var viberClient = new ViberBotClient(ViberBotToken))
                {
                    await viberClient.SendTextMessageAsync(new TextMessage()
                        {
                            Receiver = receiverId,
                            Text = message,
                        });
                }
            }
            catch (ViberRequestApiException ex)
            {
                errorInfo = $"{ex.Message} | {ex.Method} | {ex.Request} | {ex.Response}";
            }
            catch (Exception ex)
            {
                errorInfo = $"Unhandled exception: {ex.Message}.";
            }

            return new NotificationResult()
                {
                    IsSuccess = errorInfo == null,
                    ErrorInfo = errorInfo,
                };
        }
    }
}