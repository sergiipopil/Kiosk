using System;
using System.Linq;
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

            SetStateString(ekProduct.State);            

            ProductionYear = ekProduct.ProductionYear;
            PartNumber = ekProduct.PartNumber;
            IsNotAvailable = Price <= 0;

            // description
            _descriptionValue = ekProduct.Description;
            UpdateParametersByValue();
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

        private void SetStateString(EkProductStateEnum state)
        {
            switch (state)
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
                    StateString = "";
                    break;
            }
        }

        #endregion

        public string ProductionYear { get; set; }

        public string PartNumber { get; set; }

        public bool IsNotAvailable { get; }




        #region Parameters
        private static string[] ParametersToIgnore => new string[] { "stan", "faktura" };
        private OfferParameter[] _parameters;        
        private string _Parameters;

        public string Parameters
        {
            get => _Parameters;
            set => SetProperty(ref _Parameters, value);
        }



        #endregion

        private void UpdateParametersByValue()
        {
            if (_parameters==null)
            {
                return;
            }
            _parameters = _parameters.Where(x => !ParametersToIgnore.Contains(x.Name[Languages.PolishCode].ToLower())).ToArray();
            var paramsStrPl = String.Join("\n", _parameters.Select(x => $"{x.Name[Languages.PolishCode]}: {x.Value[Languages.PolishCode]}"));
            var paramsStrRu = String.Join("\n", _parameters.Select(x => $"{x.Name[Languages.RussianCode]}: {x.Value[Languages.RussianCode]}"));
            Parameters = String.IsNullOrEmpty(paramsStrRu) ? paramsStrPl : paramsStrRu + "\n";
        }

        #region Description

        // DESCRIPTION
        private MultiLanguageString _descriptionValue;
        private string _Description;

        public string Description
        {
            get => _Description;
            set => SetProperty(ref _Description, value);
        }        

        private void UpdateDescriptionByValue()
        {
            Description = _descriptionValue?.GetValue(Languages.RussianCode);
        }
        public bool IsDescriptionRequestRequired { get; private set; }

        private readonly object _descriptionLocker = new object();
        #endregion
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

        public void RequestDescriptionTranslate()
        { 
            Task.Run(async () =>
            {
                try
                {                   
                    var productKey = EkProductKey.FromKey(Key);
                    var response = await ServerApiHelper.TranslateTermAsync(
                     new EkKioskTranslateTermGetRequest()
                     {
                         Term = GetDescription()[Languages.PolishCode],
                     }, CancellationToken.None);

                    lock (_descriptionLocker)
                    {
                         Description = response.Translation;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(LogContextEnum.Communication, $"Description translation failed ({Key}).", ex);
                }
                
            }).Wait();                   
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
                            _parameters = new OfferParameter[response.Parameters.Count];
                            response.Parameters.CopyTo(_parameters, 0);
                            UpdateParametersByValue();

                            _descriptionValue = response.Description;
                            UpdateDescriptionByValue();                            
                            SetStateString(response.State);
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