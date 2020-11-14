using System;
using System.Collections.Generic;
using System.Linq;
using KioskBrains.Clients.AllegroPl.Models;
using KioskBrains.Common.Constants;
using KioskBrains.Common.Contracts;
using KioskBrains.Common.EK.Api;
using KioskBrains.Common.EK.Helpers;
using KioskBrains.Server.EK.Common.Search.Models;

namespace KioskBrains.Server.Domain.Actions.EkKiosk
{
    public static class EkConvertHelper
    {
        public static EkProduct EkNewIndexProductToProduct(IndexProduct indexProduct)
        {
            Assure.ArgumentNotNull(indexProduct, nameof(indexProduct));

            var photos = string.IsNullOrEmpty(indexProduct.ThumbnailUrl)
                ? null
                : new[]
                {
                    new EkProductPhoto()
                    {
                        Url = indexProduct.ThumbnailUrl
                    },
                };

            var product = new EkProduct()
            {
                Key = indexProduct.Key,
                PartNumber = indexProduct.PartNumber,
                BrandName = indexProduct.BrandName,
                Source = (EkProductSourceEnum)indexProduct.Source,
                SourceId = indexProduct.SourceId,
                Name = new MultiLanguageString()
                {
                    [Languages.RussianCode] = indexProduct.Name_ru,
                },
                Description = new MultiLanguageString()
                {
                    [Languages.RussianCode] = indexProduct.Description_ru,
                },
                SpecificationsJson = new MultiLanguageString()
                {
                    [Languages.RussianCode] = indexProduct.SpecificationsJson_ru,
                },
                Photos = photos,
                BasePrice = (decimal)(indexProduct.Price ?? 0),
                BasePriceCurrencyCode = "UAH",
                DeliveryPrice = 0,
                State = EkProductStateEnum.New,
                ProductionYear = indexProduct.ProductionYear,
            };

            switch (product.Source)
            {
                case EkProductSourceEnum.OmegaAutoBiz:
                case EkProductSourceEnum.ElitUa:
                {
                    var A_Price = product.BasePrice;
                    // fixed markup 
                    const decimal C_Markup = 0.17m; // 17%
                    var price = A_Price * (1 + C_Markup);
                    product.Price = RoundPrice(price);
                    product.PriceCalculationInfo = $"Formula=A+C%, A={A_Price}, C={C_Markup:P}, Source={product.Source}";
                    break;
                }

                default:
                {
                    product.Price = 0;
                    product.PriceCalculationInfo = $"Forbidden (source '{product.Source}' is not supported)";
                    break;
                }
            }

            product.PriceCurrencyCode = product.BasePriceCurrencyCode;

            return product;
        }

        public static EkProduct EkOmegaPartNumberBrandToProduct(EkPartNumberBrand partNumberBrand)
        {
            var product = new EkProduct()
            {
                Key = partNumberBrand.ProductKey,
                PartNumber = partNumberBrand.PartNumber,
                BrandName = partNumberBrand.BrandName,
                Source = EkProductSourceEnum.OmegaAutoBiz,
                Name = partNumberBrand.Name ?? new MultiLanguageString(),
                Description = new MultiLanguageString(),
                SpecificationsJson = new MultiLanguageString(),
                Photos = null,
                BasePrice = 0,
                BasePriceCurrencyCode = "UAH",
                DeliveryPrice = 0,
                State = EkProductStateEnum.New,
                ProductionYear = null,
            };

            product.Price = RoundPrice(product.BasePrice);
            product.PriceCurrencyCode = product.BasePriceCurrencyCode;

            return product;
        }

        public static EkProduct EkAllegroPlOfferToProduct(Offer offer, decimal exchangeRate)
        {
            Assure.ArgumentNotNull(offer, nameof(offer));

            var productKey = new EkProductKey(EkProductSourceEnum.AllegroPl, offer.Id);
            EkProductStateEnum state;
            switch (offer.State)
            {
                case OfferStateEnum.Used:
                    state = EkProductStateEnum.Used;
                    break;
                case OfferStateEnum.Recovered:
                    state = EkProductStateEnum.Recovered;
                    break;
                case OfferStateEnum.Broken:
                    state = EkProductStateEnum.Broken;
                    break;
                default:
                    state = EkProductStateEnum.Unknown;
                    break;
                case OfferStateEnum.New:
                    state = EkProductStateEnum.New;
                    break;
            }

            decimal deliveryPrice;
            if (offer.DeliveryOptions?.Length > 0)
            {
                deliveryPrice = offer.DeliveryOptions
                    .Max(x => x.Price);
            }
            else
            {
                deliveryPrice = 0;
            }

            var product = new EkProduct()
            {
                Key = productKey.ToKey(),
                PartNumber = null,
                BrandName = null,
                Source = EkProductSourceEnum.AllegroPl,
                SourceId = offer.Id,
                CategoryId = offer.CategoryId,
                Name = offer.Name,
                Description = offer.Description,
                SpecificationsJson = new MultiLanguageString(),
                Photos = offer.Images?
                    .Select(x => new EkProductPhoto()
                    {
                        Url = x.Url
                    })
                    .ToArray(),
                BasePrice = offer.Price,
                BasePriceCurrencyCode = offer.PriceCurrencyCode,
                DeliveryPrice = deliveryPrice,
                State = state,
                ProductionYear = null,
            };

            var isSpecialEngineTransmissionProduct = IsSpecialEngineTransmissionProduct(product);

            if (isSpecialEngineTransmissionProduct
                && product.State == EkProductStateEnum.Broken)
            {
                product.Price = 0;
                product.PriceCurrencyCode = "UAH";
                product.PriceCalculationInfo = "Forbidden (broken Engine/Transmission)";
            }
            else
            {
                var P_Price = product.BasePrice;
                var D_Price = product.DeliveryPrice;
                var M_Markup = GetC_Markup(product, isSpecialEngineTransmissionProduct);
                var T_Taxes = GetB_Taxes(product, isSpecialEngineTransmissionProduct);
                var R_Rate = exchangeRate;

                var price = (P_Price + D_Price) * (1 + T_Taxes) * (1 + M_Markup) * R_Rate;

                product.Price = RoundPrice(price);
                product.PriceCurrencyCode = "UAH";
                product.PriceCalculationInfo = $"Formula=((P+D)+M%+T%)*R, P={P_Price}, D={D_Price}, M={M_Markup:P}, T={T_Taxes:P}, R={R_Rate}, Category={(isSpecialEngineTransmissionProduct ? "Engine/Transmission" : "Regular")}";
            }

            return product;
        }

        private static decimal GetB_Taxes(EkProduct product, bool isSpecialEngineTransmissionProduct)
        {
            int percentage;
            if (isSpecialEngineTransmissionProduct)
            {
                percentage = 15;
            }
            else
            {
                percentage = 0;
            }

            return ((decimal)percentage) / 100;
        }

        private static readonly Dictionary<long, int> RegularMarkupPercentages = new Dictionary<long, int>()
        {
            [600] = 40,
            [1_000] = 30,
            [2_000] = 25,
            [3_500] = 25,
            [5_000] = 20,
            [7_000] = 15,
            [9_000] = 15,
            [11_000] = 15,
            [13_000] = 15,
            [20_000] = 15,
            [50_000] = 15,
            [70_000] = 15,
            [100_000] = 15,
            [long.MaxValue] = 15,
        };

        private static decimal GetC_Markup(EkProduct product, bool isSpecialEngineTransmissionProduct)
        {
            var basePrice = product.BasePrice + product.DeliveryPrice;

            // fails if price is greater than long.MaxValue
            var percentage = RegularMarkupPercentages
                .Where(x => basePrice <= x.Key)
                .Select(x => x.Value)
                .First();

            return ((decimal)percentage) / 100;
        }

        private const string EnginesCategoryId = "312565";

        private const string EnginesPetrolCategoryId = "50850";

        private const string EnginesDieselCategoryId = "50851";

        private const string TransmissionCategoryId = "50873";

        private static readonly string[] SpecialCategoryIds = new[]
        {
            EnginesCategoryId,
            EnginesPetrolCategoryId,
            EnginesDieselCategoryId,
            TransmissionCategoryId,
        };

        private static bool IsSpecialEngineTransmissionProduct(EkProduct product)
        {
            return SpecialCategoryIds.Contains(product.CategoryId);
        }

        public static decimal RoundPrice(decimal price)
        {
            return Math.Ceiling(price);
        }

        public static string GetValueOrFallback(string value, string fallbackValue)
        {
            return !string.IsNullOrEmpty(value)
                ? value
                : fallbackValue;
        }
    }
}