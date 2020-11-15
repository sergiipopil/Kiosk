using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KioskBrains.Clients.AllegroPl;
using KioskBrains.Clients.AllegroPl.Models;
using KioskBrains.Clients.AllegroPl.ServiceInterfaces;
using KioskBrains.Common.EK.Api;
using KioskBrains.Server.Domain.Entities;
using KioskBrains.Server.Domain.Entities.DbStorage;
using KioskBrains.Server.Domain.Helpers.Dates;
using KioskBrains.Server.Domain.Managers;
using KioskBrains.Server.Domain.Security;
using KioskBrains.Waf.Actions.Common;
using Microsoft.AspNetCore.Http;

namespace KioskBrains.Server.Domain.Actions.EkKiosk.EkKioskProductSearchInEurope
{
    [AuthorizeUser(UserRoleEnum.KioskApp)]
    public class EkKioskProductSearchInEuropeGet : WafActionGet<EkKioskProductSearchInEuropeGetRequest, EkKioskProductSearchInEuropeGetResponse>
    {
        private readonly AllegroPlClient _allegroPlClient;

        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly CentralBankExchangeRateManager _centralBankExchangeRateManager;
        private readonly CurrentUser _currentUser;
        ITranslateService _translateService;
        KioskBrainsContext _context;


        public EkKioskProductSearchInEuropeGet(
            AllegroPlClient allegroPlClient,
            IHttpContextAccessor httpContextAccessor,
            CentralBankExchangeRateManager centralBankExchangeRateManager,
            CurrentUser currentUser, ITranslateService translateService, KioskBrainsContext context)
        {
            _allegroPlClient = allegroPlClient;
            _translateService = translateService;
            _httpContextAccessor = httpContextAccessor;
            _centralBankExchangeRateManager = centralBankExchangeRateManager;
            _context = context;
            this._currentUser = currentUser;
        }

        private const string AutomotiveCategoryId = "3";

        public override async Task<EkKioskProductSearchInEuropeGetResponse> ExecuteAsync(EkKioskProductSearchInEuropeGetRequest request)
        {
            //var kioskId = _currentUser.Id;
            //if (kioskId == 75) 
            //{
            //    return await MySpecialLogicAsync();
            //}

            var categoryId = request.CategoryId;
            if (string.IsNullOrEmpty(categoryId))
            {
                categoryId = AutomotiveCategoryId;
            }

            OfferStateEnum offerState;
            switch (request.State)
            {
                case EkProductStateEnum.New:
                    offerState = OfferStateEnum.New;
                    break;
                case EkProductStateEnum.Used:
                    offerState = OfferStateEnum.Used;
                    break;
                case EkProductStateEnum.Recovered:
                    offerState = OfferStateEnum.Recovered;
                    break;
                case EkProductStateEnum.Broken:
                    offerState = OfferStateEnum.Broken;
                    break;
                default:
                    offerState = OfferStateEnum.All;
                    break;
            }

            OfferSortingEnum offerSorting;
            switch (request.Sorting)
            {
                case EkProductSearchSortingEnum.PriceAscending:
                    offerSorting = OfferSortingEnum.PriceAsc;
                    break;
                case EkProductSearchSortingEnum.PriceDescending:
                    offerSorting = OfferSortingEnum.PriceDesc;
                    break;
                default:
                case EkProductSearchSortingEnum.Default:
                    offerSorting = OfferSortingEnum.Relevance;
                    break;
            }

            // cancellation token
            var cancellationToken = _httpContextAccessor.HttpContext?.RequestAborted ?? CancellationToken.None;

            var searchOffersResponse = await _allegroPlClient.SearchOffersAsync(
                request.Term,
                request.TranslatedTerm,
                categoryId,
                offerState,
                offerSorting,
                request.From,
                request.Count,
                cancellationToken);

            await _allegroPlClient.ApplyTranslations(_translateService, searchOffersResponse.Offers, request.Term, request.TranslatedTerm, cancellationToken);

            EkProduct[] products;
            if (searchOffersResponse.Offers?.Length > 0)
            { 
                var exchangeRate = await GetExchangeRateAsync();

                products = searchOffersResponse.Offers
                    .Select(x => EkConvertHelper.EkAllegroPlOfferToProduct(x, exchangeRate))
                    .ToArray();
            }
            else
            {
                products = new EkProduct[0];
            }

            return new EkKioskProductSearchInEuropeGetResponse()
                {
                    Products = products,
                    Total = searchOffersResponse.Total,
                    TranslatedTerm = searchOffersResponse.TranslatedPhrase,
                };
        }

        private async Task<decimal> GetExchangeRateAsync()
        {
            var ukrainianNow = TimeZones.GetTimeZoneNow(TimeZones.UkrainianTime);
            const string LocalCurrencyCode = "UAH";
            const string ForeignCurrencyCode = "PLN";

            // todo: cache
            var exchangeRate = await _centralBankExchangeRateManager.GetCurrentRateAsync(LocalCurrencyCode, ForeignCurrencyCode, ukrainianNow);
            if (exchangeRate == null)
            {
                throw new InvalidOperationException($"CB exchange rate for {LocalCurrencyCode}-{ForeignCurrencyCode} is not presented.");
            }

            return exchangeRate.Value;
        }
    }
}