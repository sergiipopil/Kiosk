using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Globalization;
using KioskApp.Ek.Checkout.Steps.Payment;
using KioskApp.Ek.Receipt;
using KioskBrains.Common.Contracts;
using KioskBrains.Common.EK.Transactions;
using KioskBrains.Common.Logging;
using KioskBrains.Common.Transactions;
using KioskBrains.Kiosk.Core.Components;
using KioskBrains.Kiosk.Core.Devices.Documents;
using KioskBrains.Kiosk.Core.Inactivity;
using KioskBrains.Kiosk.Core.Settings;
using KioskBrains.Kiosk.Core.Transactions;
using KioskBrains.Kiosk.Helpers.Ui.Binding;

namespace KioskApp.Ek
{
    /// <summary>
    /// All methods are safe.
    /// </summary>
    public class EkProcess : UiBindableObject
    {
        public EkProcess(Action<TransactionStatusEnum> resetForNewUserAction)
        {
            Assure.ArgumentNotNull(resetForNewUserAction, nameof(resetForNewUserAction));

            _resetForNewUserAction = resetForNewUserAction;

            Cart = new Cart.Cart();
            Cart.CartChanged += OnCartChanged;
            Cart.PromoCodeChanged += OnCartPromoCodeChanged;
        }

        private readonly Action<TransactionStatusEnum> _resetForNewUserAction;

        public EkProcessStateEnum State { get; private set; } = EkProcessStateEnum.New;

        private readonly object _stateLocker = new object();

        private EkTransaction _transaction;

        /// <summary>
        /// Can be invoked multiple times - started only once and only if state is <see cref="EkProcessStateEnum.New"/>.
        /// </summary>
        private void StartIfNew()
        {
            lock (_stateLocker)
            {
                if (State != EkProcessStateEnum.New)
                {
                    return;
                }

                State = EkProcessStateEnum.Started;
            }

            try
            {
                _transaction = TransactionManager.Current.StartNewTransaction<EkTransaction>();
                _transaction.TotalPriceCurrencyCode = "UAH";

                // start inactivity tracking
                InactivityManager.Current.StartTracking(
                    new InactivityBehavior(60),
                    () => _resetForNewUserAction(TransactionStatusEnum.CancelledByTimeout));

                Log.Info(LogContextEnum.Workflow, "Process started.");
            }
            catch (Exception ex)
            {
                Log.Error(LogContextEnum.Workflow, "Process start failed.", ex);
            }
        }

        public void OnLanguageSelected(Language language)
        {
            StartIfNew();

            // trace somehow
        }

        public void OnViewChanged(string viewName, bool isMainView)
        {
            if (!isMainView)
            {
                // start process only if not main page is visited
                StartIfNew();
            }

            // trace somehow
        }

        public Cart.Cart Cart { get; }

        private void OnCartChanged(object sender, EventArgs e)
        {
            StartIfNew();

            _transaction.TotalPrice = Cart.TotalPrice;

            // set products to transaction
            var cartProducts = Cart.Products
                .Select(x => new
                    {
                        x.Product,
                        x.Quantity,
                    })
                .ToArray();

            // serialization can be slow but no new thread is created since sometimes sync issues appear (empty products in completed transaction)
            // todo: hypothesis above should be proved
            var transactionProducts = cartProducts
                .Select(x => EkTransactionProduct.FromProduct(x.Product.EkProduct, x.Product.GetDescription(), x.Quantity))
                .ToArray();
            _transaction.SetProducts(transactionProducts);
            _receiptData.Products = cartProducts
                .Select(x => new ReceiptDataProduct(x.Product, x.Quantity))
                .ToArray();
        }

        private void OnCartPromoCodeChanged(object sender, EventArgs e)
        {
            StartIfNew();

            _transaction.PromoCode = Cart.PromoCode;
        }

        public void OnCustomerInfoInput(EkCustomerInfo customerInfo)
        {
            StartIfNew();

            _transaction.SetCustomerInfo(customerInfo);
            _receiptData.CustomerInfo = customerInfo;
        }

        public void OnDeliveryInfoInput(EkDeliveryInfo deliveryInfo)
        {
            StartIfNew();

            _transaction.SetDeliveryInfo(deliveryInfo);
            _receiptData.DeliveryInfo = deliveryInfo;
        }

        public PaymentMethodInfo SelectedPaymentMethodInfo { get; set; }

        private readonly ReceiptData _receiptData = new ReceiptData();

        public async Task<ReceiptData> GenerateReceiptDataAsync()
        {
            StartIfNew();

            var kioskAppSettings = ComponentManager.Current.GetComponent<IKioskAppSettingsContract>();
            var kioskId = kioskAppSettings.State.KioskId;

            string receiptNumber;
            try
            {
                // generate receipt number
                var documentName = "EkReceipt";
                var nextReceiptId = await DocumentManager.Current.GetNextDocumentNumberAsync(documentName);
                receiptNumber = $"{kioskId}-{nextReceiptId}";
            }
            catch (Exception ex)
            {
                Log.Error(LogContextEnum.File, nameof(GenerateReceiptDataAsync), ex);
                // fallback value
                receiptNumber = $"{kioskId}-{DateTime.Now:yyyyMMddHHmm}";
            }

            _transaction.ReceiptNumber = receiptNumber;
            _receiptData.ReceiptNumber = receiptNumber;
            _receiptData.TotalPrice = Cart.TotalPrice;
            _receiptData.TotalPriceCurrencyCode = _transaction.TotalPriceCurrencyCode;

            return _receiptData;
        }

        /// <summary>
        /// Can be invoked multiple times - completed only once and only if state is <see cref="EkProcessStateEnum.Started"/>.
        /// </summary>
        public void CompleteTransaction(TransactionStatusEnum transactionStatus)
        {
            lock (_stateLocker)
            {
                if (State != EkProcessStateEnum.Started)
                {
                    return;
                }

                State = EkProcessStateEnum.TransactionCompleted;
            }

            try
            {
                if (_transaction != null)
                {
                    // transaction
                    if (transactionStatus == TransactionStatusEnum.Completed)
                    {
                        _transaction.CompletionStatus = TransactionCompletionStatusEnum.Success;
                    }

                    TransactionManager.Current.EndTransaction(_transaction, transactionStatus);
                }

                Log.Info(LogContextEnum.Workflow, "Process completed.");
            }
            catch (Exception ex)
            {
                Log.Error(LogContextEnum.Workflow, "Process completion failed.", ex);
            }
        }

        /// <summary>
        /// Can be invoked multiple times.
        /// </summary>
        public void End()
        {
            // just stop inactivity tracking
            InactivityManager.Current.StopTracking();
        }
    }
}