using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using KioskBrains.Waf.Managers.Common;
using Newtonsoft.Json;

namespace KioskBrains.Server.Domain.Managers.Integration.Rates.UaCentralBank
{
    public class UaCentralBankApiProxy : IWafManager
    {
        public const string DateFormat = "dd.MM.yyyy";

        public async Task<List<CurrencyPairDateRate>> GetRatesAsync()
        {
            using (var httpClient = new HttpClient())
            {
                var ratesJson = await httpClient.GetStringAsync(new Uri("https://bank.gov.ua/NBUStatService/v1/statdirectory/exchange?json"));
                var rateRecords = JsonConvert.DeserializeObject<ExchangeRecord[]>(ratesJson);

                var rates = new List<CurrencyPairDateRate>();
                foreach (var rateRecord in rateRecords)
                {
                    rates.Add(new CurrencyPairDateRate()
                        {
                            LocalCurrencyCode = "UAH",
                            ForeignCurrencyCode = rateRecord.cc,
                            Date = DateTime.ParseExact(rateRecord.exchangedate, DateFormat, null),
                            Rate = rateRecord.rate,
                        });
                }

                return rates;
            }
        }
    }
}