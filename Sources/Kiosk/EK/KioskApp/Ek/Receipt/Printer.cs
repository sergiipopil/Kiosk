using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.SerialCommunication;
using KioskBrains.Common.Contracts;
using KioskBrains.Common.EK.Transactions;
using KioskBrains.Common.Logging;
using KioskBrains.Kiosk.Core.Components;
using KioskBrains.Kiosk.Core.Devices.Common;
using KioskBrains.Kiosk.Core.Devices.Common.SerialPort;
using KioskBrains.Kiosk.Core.Devices.Helpers;
using KioskBrains.Kiosk.Core.Settings;
using KioskBrains.Kiosk.Helpers.Text;
using KioskBrains.Kiosk.Helpers.Threads;

namespace KioskApp.Ek.Receipt
{
    public static class Printer
    {
        public static Task PrintReceiptAsync(ReceiptData receiptData)
        {
            return ThreadHelper.RunInNewThreadAsync(async () =>
                {
                    try
                    {
                        await PrintReceiptImplementationAsync(receiptData);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(LogContextEnum.Device, "Receipt printing failed.", ex);
                    }
                });
        }

        private const string ReceiptPrinterComPort = "COM7";

        private const string INIT = "\x1B\x40";
        private const string SET_B_MODE = "\x1B\x21\x01";
        private const string SET_LEFT_MARGIN = "\x1D\x4C\x12\x00";
        private const string ALIGN_LEFT = "\x1B\x61\x00";
        private const string ALIGN_CENTER = "\x1B\x61\x01";
        private const string ALIGN_RIGHT = "\x1B\x61\x02";
        private const string CLEAR_BOLD = "\x1B\x45\x00";
        private const string SET_BOLD = "\x1B\x45\x01";
        private const string TOTAL_CUT = "\x1B\x69";
        private const string FORM_FEED = "\x0C";

        private static async Task PrintReceiptImplementationAsync(ReceiptData receiptData)
        {
            if (!await SerialPortHelper.IsSerialPortPresentedAsync(ReceiptPrinterComPort))
            {
                return;
            }

            var deviceIoPortProvider = new UwpSerialPortProvider(new SerialPortSettings(ReceiptPrinterComPort, 115200, SerialStopBitCount.One, SerialParity.None));
            using (var devicePort = new DeviceIoPortDriver(deviceIoPortProvider))
            {
                await devicePort.OpenAsync();

                var messageBytes = GetReceiptBytes(receiptData);

                await devicePort.WriteAsync(messageBytes, CancellationToken.None);
            }
        }

        private static byte[] GetReceiptBytes(ReceiptData receiptData)
        {
            Assure.ArgumentNotNull(receiptData, nameof(receiptData));

            var kioskAppSettings = ComponentManager.Current.GetComponent<IKioskAppSettingsContract>().State;
            var now = DateTime.Now;
            var kioskAddress = kioskAppSettings.KioskAddress;
            var kioskAddressParts = new[] { kioskAddress?.AddressLine2, kioskAddress?.City, kioskAddress?.AddressLine1 };
            var kioskAddressString = string.Join(", ", kioskAddressParts.Where(x => !string.IsNullOrEmpty(x)));

            string deliveryTypeString;
            string[] deliveryAddressParts;
            switch (receiptData.DeliveryInfo?.Type)
            {
                case EkDeliveryTypeEnum.DeliveryServiceStore:
                    deliveryTypeString = "Самовивіз з Нової Пошти";
                    deliveryAddressParts = new[]
                        {
                            receiptData.DeliveryInfo?.Address?.City,
                            // address line contains branch number
                            receiptData.DeliveryInfo?.Address?.AddressLine1
                        };
                    break;
                default:
                case EkDeliveryTypeEnum.Courier:
                    deliveryTypeString = "Курьєр";
                    deliveryAddressParts = new[]
                        {
                            receiptData.DeliveryInfo?.Address?.City,
                            receiptData.DeliveryInfo?.Address?.AddressLine1
                        };
                    break;
            }

            var deliveryAddressString = string.Join(", ", deliveryAddressParts.Where(x => !string.IsNullOrEmpty(x)));

            var productsStringBuilder = new StringBuilder();
            if (receiptData.Products?.Length > 0)
            {
                for (var i = 0; i < receiptData.Products.Length; i++)
                {
                    var product = receiptData.Products[i];
                    var totalProductPrice = product.Price*product.Quantity;
                    productsStringBuilder.AppendLine(
                        $@"{EscP.ALIGN_LEFT}{i + 1}. {product.Name.FirstNSymbols(100)}
   {product.Quantity} x {product.Price.ToAmountString(false)} = {totalProductPrice.ToAmountString(false)} {product.PriceCurrencyCode}");
                }
            }

            var message = $@"{EscP.INIT}{EscP.SET_LEFT_MARGIN}{EscP.ALIGN_LEFT}{EscP.SET_B_MODE}{EscP.SET_BOLD}Номер термінала: {EscP.CLEAR_BOLD}{kioskAppSettings.KioskId}
{EscP.SET_BOLD}Адреса терміналу: {EscP.CLEAR_BOLD}{kioskAddressString}
{EscP.SET_BOLD}Дата: {EscP.CLEAR_BOLD}{now:dd.MM.yyyy} {EscP.SET_BOLD}Час: {EscP.CLEAR_BOLD}{now:HH:mm:ss}
{EscP.SET_BOLD}Номер операції: {EscP.CLEAR_BOLD}{receiptData.ReceiptNumber}

{EscP.SET_A_MODE}{EscP.ALIGN_CENTER}{EscP.SET_BOLD}ЗАМОВЛЕННЯ ТОВАРІВ/ПОСЛУГ{EscP.CLEAR_BOLD}{EscP.SET_B_MODE}
{productsStringBuilder}{EscP.ALIGN_LEFT}{EscP.SET_BOLD}ЗАГАЛОМ: {EscP.CLEAR_BOLD}{EscP.ALIGN_RIGHT}{receiptData.TotalPrice.ToAmountString(false)} {receiptData.TotalPriceCurrencyCode}

{EscP.ALIGN_LEFT}{EscP.SET_BOLD}Замовник: {EscP.CLEAR_BOLD}{receiptData.CustomerInfo?.FullName}
{EscP.SET_BOLD}Телефон: {EscP.CLEAR_BOLD}{receiptData.CustomerInfo?.Phone}
{EscP.SET_BOLD}Доставка: {EscP.CLEAR_BOLD}{deliveryTypeString}
{EscP.SET_BOLD}Адрес: {EscP.CLEAR_BOLD}{deliveryAddressString}

{EscP.SET_A_MODE}{EscP.ALIGN_CENTER}{EscP.SET_BOLD}ІНСТРУКЦІЯ{EscP.CLEAR_BOLD}{EscP.SET_B_MODE}
{EscP.ALIGN_LEFT}1. Протягом 1 хвилини Ви отримаєте СМС з інструкціями по оплаті.
2. Виберіть і оплатіть замовлення зручним способом оплати.
3. Слідкуйте за статусом замовлення і доставки з допомогою СМС.

{EscP.SET_A_MODE}{EscP.ALIGN_CENTER}{EscP.SET_BOLD}АБО ОПЛАТІТЬ В БАНКУ ПО РЕКВІЗИТАМ{EscP.CLEAR_BOLD}{EscP.SET_B_MODE}
{EscP.ALIGN_LEFT}ФОП ""Дубина Л. В.""
14000, Чернігівська обл., м. Чернігів,
вул. Проспект Перемоги, 90/78
UA383052990000026009016303944
в АТ КБ ""ПРИВАТБАНК""
МФО 353586
ЄДРПОУ 2092012427
Платник єдиного податку на 
спрощеній системі оподаткування.
Сума {receiptData.TotalPrice.ToAmountString(false)} {receiptData.TotalPriceCurrencyCode}


{EscP.SET_A_MODE}{EscP.ALIGN_CENTER}{EscP.SET_BOLD}ДЯКУЄМО!{EscP.CLEAR_BOLD}{EscP.SET_B_MODE}

{EscP.SET_BOLD}Телефон контакт-центра:
{kioskAppSettings.SupportPhone}

{EscP.FORM_FEED}";

            message = message.Replace("\r", "");

            // todo: REMOVE AND FIX UKR LANGUAGE
            message = message
                .Replace('І', 'I')
                .Replace('і', 'i')
                .Replace('Ї', 'I')
                .Replace('ї', 'i')
                .Replace('Є', 'E')
                .Replace('є', 'e');

            // todo: should we make this only once?
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var cyrillicEncoding = Encoding.GetEncoding(866);
            var messageBytes = cyrillicEncoding.GetBytes(message);
            return messageBytes;
        }

        private static string GetPrintQrCodeDataCommand(string qrData)
        {
            var commandBuilder = new StringBuilder();

            // specify encoding scheme - QRCode (x00)
            commandBuilder.Append("\x1D\x28\x6B\x03\x00\x31\x41\x00");

            // specify dot size - 12 (0x1C)
            commandBuilder.Append("\x1D\x28\x6B\x03\x00\x31\x42\x0C");

            // specify size - AUTO (x00)
            commandBuilder.Append("\x1D\x28\x6B\x03\x00\x31\x43\x00");

            // store QR code data
            var storeSize = qrData.Length + 3;
            var store_pL = (char)(storeSize%256);
            var store_pH = (char)(storeSize/256);
            commandBuilder.Append($"\x1D\x28\x6B{store_pL}{store_pH}\x31\x50\x31{qrData}");

            // print QR code
            commandBuilder.Append("\x1D\x28\x6B\x03\x00\x31\x51\x31");

            return commandBuilder.ToString();
        }
    }
}