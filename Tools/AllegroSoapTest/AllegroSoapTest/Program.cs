using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ServiceReference1;

namespace ConsoleApp15
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            TestAllegroAsync().Wait();
        }

        private static async Task TestAllegroAsync()
        {
            const int CountryId = 1; // Poland
            const string WebApiKey = "488f1691";

            var client = new servicePortClient();

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var sysStatus = await client.doQuerySysStatusAsync(new doQuerySysStatusRequest(1, CountryId, WebApiKey));

            Console.WriteLine(stopWatch.ElapsedMilliseconds);
            stopWatch.Restart();

            var loginResponse = await client.doLoginAsync(
                new doLoginRequest(
                    "nicolianick@gmail.com",
                    "Paris777",
                    CountryId,
                    WebApiKey,
                    sysStatus.verKey));

            Console.WriteLine(stopWatch.ElapsedMilliseconds);
            stopWatch.Restart();

            var sessionHandle = loginResponse.sessionHandlePart;
            var items = await client.doGetItemsInfoAsync(
                new doGetItemsInfoRequest(
                    sessionHandle,
                    new[] { 7855602159 },
                    1, // description
                    1, 1, 1, 1, 1, 1, 1, 1));

            Console.WriteLine(stopWatch.ElapsedMilliseconds);
            stopWatch.Restart();

            var shipmentData = await client.doGetShipmentDataAsync(new doGetShipmentDataRequest(CountryId, WebApiKey));
            var shipmentDataById = shipmentData.shipmentDataList.ToDictionary(x => x.shipmentId);

            Console.WriteLine(stopWatch.ElapsedMilliseconds);
            stopWatch.Restart();

            foreach (var p in items.arrayItemListInfo
                .SelectMany(x => x.itemPostageOptions))
            {
                var shipmentInfo = shipmentDataById.GetValueOrDefault(p.postageId);

                Console.WriteLine($"{p.postageId} ({shipmentInfo?.shipmentType} {shipmentInfo?.shipmentName}) {p.postageAmount} {p.postageFulfillmentTime.fulfillmentTimeFrom}-{p.postageFulfillmentTime.fulfillmentTimeTo}");
            }
        }
    }
}