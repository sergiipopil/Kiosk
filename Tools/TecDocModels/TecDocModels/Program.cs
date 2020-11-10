using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using KioskBrains.Common.Contracts;
using KioskBrains.Common.EK.Api.CarTree;
using Newtonsoft.Json;

namespace TecDocModels
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            JsonDefaultSettings.Initialize();

            var manufacturerIds = JsonConvert.DeserializeObject<Dictionary<string, int>>(
                File.ReadAllText("ManufacturerIds.json", Encoding.UTF8));

            var records = new List<ModelRecord>();
            records.AddRange(ReadModelRecords("MappingCars.csv", TecDocTypeEnum.P));
            records.AddRange(ReadModelRecords("MappingTrucks.csv", TecDocTypeEnum.O));

            var carGroups = records
                .GroupBy(x => x.CarType)
                .Select(x => new EkCarGroup()
                    {
                        CarType = x.Key,
                        Manufacturers = GetManufacturers(x.Key, x.ToArray(), manufacturerIds),
                    })
                .ToArray();

            var json = JsonConvert.SerializeObject(carGroups);
        }

        private static EkCarManufacturer[] GetManufacturers(EkCarTypeEnum carType, ModelRecord[] records, Dictionary<string, int> manufacturerIds)
        {
            var popularBrands = File.ReadAllLines($"{carType}Brands.txt")
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToHashSet();

            return records
                .GroupBy(x => x.BrandName)
                .Where(x => popularBrands.Contains(x.Key))
                .Select(x => new EkCarManufacturer()
                    {
                        Id = manufacturerIds[x.Key],
                        Name = x.Key,
                        CarModels = GetCarModels(x.ToArray())
                    })
                .OrderBy(x => x.Name)
                .ToArray();
        }

        private static EkCarModel[] GetCarModels(ModelRecord[] records)
        {
            return records
                .Select(x => new EkCarModel()
                    {
                        Id = x.ModelId,
                        Name = x.ModelName,
                        TecDocType = x.TecDocType,
                    })
                .OrderBy(x => x.Name)
                .ToArray();
        }

        private static ModelRecord[] ReadModelRecords(string fileName, TecDocTypeEnum tecDocType)
        {
            using (var reader = new StreamReader(fileName, Encoding.UTF8))
            {
                using (var csvReader = new CsvReader(reader))
                {
                    var records = csvReader.GetRecords<Record>()
                        .ToArray();

                    var modelRecords = records
                        .Where(x => x.Exclude != 1
                                    && !string.IsNullOrEmpty(x.CarType))
                        .Select(x => new ModelRecord()
                            {
                                TecDocType = tecDocType,
                                CarType = ParseCarType(x.CarType),
                                ModelId = x.ModelId,
                                BrandName = x.BrandName,
                                ModelName = x.ModelName,
                            })
                        .ToArray();

                    return modelRecords;
                }
            }
        }

        // Л-легковые, Г-грузовые, А-автобусы, С-спецтехника, М-мототехника
        private static EkCarTypeEnum ParseCarType(string carTypeString)
        {
            switch (carTypeString?.ToUpper())
            {
                case "Л":
                    return EkCarTypeEnum.Car;
                case "Г":
                    return EkCarTypeEnum.Truck;
                case "А":
                case "A":
                    return EkCarTypeEnum.Bus;
                case "С":
                case "C":
                    return EkCarTypeEnum.Special;
                case "М":
                case "M":
                    return EkCarTypeEnum.Moto;
                default:
                    throw new ArgumentOutOfRangeException(nameof(carTypeString), carTypeString, null);
            }
        }
    }
}