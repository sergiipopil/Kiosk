using System;
using System.Threading;
using System.Threading.Tasks;
using KioskApp.Helpers;
using KioskBrains.Common.Constants;
using KioskBrains.Common.Contracts;
using KioskBrains.Common.EK.Api;
using KioskBrains.Common.EK.Helpers;
using KioskBrains.Common.Logging;
using KioskBrains.Kiosk.Helpers.Ui.Binding;

namespace KioskApp.Search
{
    public class Product : UiBindableObject
    {
        public Product(EkProduct ekProduct)
        {
            Assure.ArgumentNotNull(ekProduct, nameof(ekProduct));

            EkProduct = ekProduct;

            Key = ekProduct.Key;
            ThumbnailUrl = ekProduct.GetThumbnailUrl();
            Photos = ekProduct.Photos;
            Name = ekProduct.Name?.GetValue(Languages.RussianCode);
            Price = ekProduct.Price;
            PriceString = Price.ToAmountStringWithSpaces();
            PriceComment = ekProduct.Source == EkProductSourceEnum.AllegroPl
                ? "с учетом доставки\nдоставка 1-7 дней"
                : "доставка 1-3 дня";

            switch (ekProduct.State)
            {
                case EkProductStateEnum.New:
                    StateString = "новое";
                    break;
                case EkProductStateEnum.Used:
                    StateString = "б/у";
                    break;
                case EkProductStateEnum.Recovered:
                    StateString = "восстановлено";
                    break;
                case EkProductStateEnum.Broken:
                    StateString = "неисправно";
                    break;
                default:
                    StateString = "?";
                    break;
            }

            ProductionYear = ekProduct.ProductionYear;
            PartNumber = ekProduct.PartNumber;
            IsNotAvailable = Price <= 0;

            // description
            _descriptionValue = ekProduct.Description;
            UpdateDescriptionByValue();
            IsDescriptionRequestRequired = !IsNotAvailable
                                           && _descriptionValue == null
                                           && ekProduct.Source == EkProductSourceEnum.AllegroPl;
        }

        public EkProduct EkProduct { get; }

        public string Key { get; set; }

        public string ThumbnailUrl { get; set; }

        public EkProductPhoto[] Photos { get; set; }

        #region Name

        private string _Name;

        public string Name
        {
            get => _Name;
            set => SetProperty(ref _Name, value);
        }

        #endregion

        public decimal Price { get; set; }

        #region PriceString

        private string _PriceString;

        public string PriceString
        {
            get => _PriceString;
            set => SetProperty(ref _PriceString, value);
        }

        #endregion

        #region PriceComment

        private string _PriceComment;

        public string PriceComment
        {
            get => _PriceComment;
            set => SetProperty(ref _PriceComment, value);
        }

        #endregion

        #region StateString

        private string _StateString;

        public string StateString
        {
            get => _StateString;
            set => SetProperty(ref _StateString, value);
        }

        #endregion

        public string ProductionYear { get; set; }

        public string PartNumber { get; set; }

        public bool IsNotAvailable { get; }

        // DESCRIPTION
        private MultiLanguageString _descriptionValue;

        #region Description

        private string _Description;

        public string Description
        {
            get => _Description;
            set => SetProperty(ref _Description, value);
        }

        #endregion

        private void UpdateDescriptionByValue()
        {
            Description = _descriptionValue?.GetValue(Languages.RussianCode);
        }

        public bool IsDescriptionRequestRequired { get; private set; }

        private readonly object _descriptionLocker = new object();

        #region IsDescriptionRequesting

        private bool _IsDescriptionRequesting;

        public bool IsDescriptionRequesting
        {
            get => _IsDescriptionRequesting;
            set => SetProperty(ref _IsDescriptionRequesting, value);
        }

        #endregion

        public MultiLanguageString GetDescription()
        {
            lock (_descriptionLocker)
            {
                return _descriptionValue;
            }
        }

        public void RequestDescription()
        {
            lock (_descriptionLocker)
            {
                if (!IsDescriptionRequestRequired)
                {
                    return;
                }

                IsDescriptionRequestRequired = false;
            }

            Task.Run(async () =>
                {
                    try
                    {
                        IsDescriptionRequesting = true;

                        var productKey = EkProductKey.FromKey(Key);
                        var response = await ServerApiHelper.AllegroProductDescriptionAsync(
                            new EkKioskAllegroProductDescriptionGetRequest()
                                {
                                    ProductId = productKey.Id,
                                },
                            CancellationToken.None);

                        lock (_descriptionLocker)
                        {
                            _descriptionValue = response.Description;
                            UpdateDescriptionByValue();
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(LogContextEnum.Communication, $"Description request failed ({Key}).", ex);
                    }
                    finally
                    {
                        IsDescriptionRequesting = false;
                    }
                });
        }
    }
}