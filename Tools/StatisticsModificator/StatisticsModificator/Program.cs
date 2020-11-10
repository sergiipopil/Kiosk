using System;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;

namespace StatisticsModificator
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            const int NewOrderCount = 170;
            const int PriceFrom = 200;
            const int PriceTo = 12_000;

            Record[] records;

            using (var reader = new StreamReader("Statistics.csv", Encoding.UTF8))
            {
                using (var csvReader = new CsvReader(reader))
                {
                    records = csvReader.GetRecords<Record>()
                        .ToArray();
                }
            }

            var nonOrderRecords = records
                .Where(x => x.Status != "Order" && x.ProductsSelected != "Y")
                .ToList();

            var random = new Random();
            for (var i = 0; i < NewOrderCount; i++)
            {
                var price = random.Next(PriceFrom, PriceTo + 1);
                var index = random.Next(nonOrderRecords.Count);
                var record = nonOrderRecords[index];
                record.Status = "Order";
                record.ProductsSelected = "Y";
                record.TotalPrice = $"{price}.00";
                nonOrderRecords.Remove(record);
            }

            using (var fileStream = new FileStream("Result.csv", FileMode.Create, FileAccess.Write))
            {
                using (var writer = new StreamWriter(fileStream, Encoding.UTF8))
                {
                    using (var csvWriter = new CsvWriter(writer))
                    {
                        csvWriter.WriteRecords(records);
                    }
                }
            }
        }
    }
}