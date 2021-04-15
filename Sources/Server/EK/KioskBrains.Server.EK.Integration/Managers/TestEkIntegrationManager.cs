﻿using System;
using System.Linq;
using System.Threading.Tasks;
using KioskBrains.Clients.Ek4Car.Models;
using KioskBrains.Common.Constants;
using KioskBrains.Common.EK.Api;
using KioskBrains.Common.EK.Helpers;
using KioskBrains.Common.EK.Transactions;
using KioskBrains.Server.Common.Services;
using KioskBrains.Waf.Helpers.Exceptions;

namespace KioskBrains.Server.EK.Integration.Managers
{
    public class TestEkIntegrationManager : IEkIntegrationManager
    {
        public async Task<EmptyData> ApplyUpdatesAsync(Update[] updates, IIntegrationLogManager integrationLogManager)
        {
            var now = DateTime.Now;
            if (updates?.Length == 2)
            {
                throw new InvalidOperationException("Update fail.");
            }

            await Task.Delay(TimeSpan.FromSeconds(3));

            return new EmptyData();
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<Kiosk> GetKioskAsync(int kioskId)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (kioskId%2 == 0)
            {
                throw EntityNotFoundException.Create<Kiosk>(kioskId);
            }

            return new Kiosk()
                {
                    Id = kioskId,
                    Address = new Address()
                        {
                            CountryCode = "UKR",
                            City = "Киев",
                            AddressLine1 = "ул. Предславинская 30, оф. 25",
                            AddressLine2 = "этаж 5",
                        }
                };
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<Order> GetOrderAsync(int orderId)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (orderId%2 == 0)
            {
                throw EntityNotFoundException.Create<Order>(orderId);
            }

            var now = DateTime.Now;
            var createdOn = new DateTime(now.Year, now.Month, now.Day, 12, 05, 35);

            var order = new Order()
                {
                    id = orderId,
                    kioskId = 1,
                    createdOnLocalTime = createdOn,
                    preferableLanguageCode = "ru",
                    products = Enumerable.Range(1, 3)
                        .Select(x =>
                            {
                                var source = x%2 == 0
                                    ? EkProductSourceEnum.OmegaAutoBiz
                                    : EkProductSourceEnum.AllegroPl;
                                var product = new EkTransactionProduct()
                                    {
                                        key = new EkProductKey(source, x.ToString()).ToKey(),
                                        source = source,
                                        name = new MultiLanguageString()
                                            {
                                                [Languages.RussianCode] = $"Продукт {x}",
                                            },
                                        description = new MultiLanguageString()
                                            {
                                                [Languages.RussianCode] = $"Описание продукта {x}",
                                            },
                                        basePrice = 125*x,
                                        basePriceCurrencyCode = source == EkProductSourceEnum.AllegroPl
                                            ? "PLN"
                                            : "UAH",
                                        quantity = x,
                                    };

                                if (source == EkProductSourceEnum.AllegroPl)
                                {
                                    product.price = Math.Ceiling(product.basePrice*7.93m*1.5m);
                                    product.priceCurrencyCode = "UAH";
                                    product.priceCalculationInfo = "BasePrice x 7.93 (exchange rate) x 1.2";
                                }
                                else
                                {
                                    product.price = Math.Ceiling(product.basePrice*1.2m);
                                    product.priceCurrencyCode = product.basePriceCurrencyCode;
                                    product.priceCalculationInfo = "BasePrice x 1.2";
                                }

                                return product;
                            })
                        .ToArray(),
                    customer = new EkCustomerInfo()
                        {
                            fullName = "Тимчик Вадим",
                            phone = "+380977861272",
                        },
                    delivery = new EkDeliveryInfo()
                        {
                            type = EkDeliveryTypeEnum.Courier,
                            address = new EkTransactionAddress()
                                {
                                    city = "Киев",
                                    addressLine1 = "ул. Предславинская 30, оф. 25",
                                },
                        },
                    receiptNumber = $"XXX-{orderId}",
                };

            order.totalPrice = order.products.Sum(x => x.price*x.quantity);
            order.totalPriceCurrencyCode = "UAH";

            return order;
        }
    }
}