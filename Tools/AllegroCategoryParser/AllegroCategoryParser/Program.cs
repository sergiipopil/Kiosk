using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using KioskBrains.Common.Constants;
using KioskBrains.Common.Contracts;
using KioskBrains.Common.EK.Api;
using Newtonsoft.Json;

namespace AllegroCategoryParser
{
    [SuppressMessage("ReSharper", "ReplaceWithSingleCallToFirst")]
    internal class Program
    {
        private static void Main(string[] args)
        {
            JsonDefaultSettings.Initialize();

            using (var reader = new StreamReader("Categories.csv", Encoding.UTF8))
            {
                using (var csvReader = new CsvReader(reader))
                {
                    var records = csvReader.GetRecords<CsvRecord>()
                        .ToArray();
                    var parsedRecords = records
                        .Select(x => new
                            {
                                Record = x,
                                NameParts = x.Name
                                    .Split('>')
                                    .Select(p => p
                                        .Trim()
                                        .Replace(" , ", ", "))
                                    .ToArray(),
                            })
                        .ToArray();
                    var maxDepth = parsedRecords.Max(x => x.NameParts.Length);

                    var categories = new Dictionary<string, EkProductCategory>();
                    var rootCategories = new List<EkProductCategory>();
                    for (var depth = 1; depth <= maxDepth; depth++)
                    {
                        var depthCategoryRecords = parsedRecords
                            .Where(x => x.NameParts.Length == depth)
                            .ToArray();
                        foreach (var parsedRecord in depthCategoryRecords)
                        {
                            var fullName = string.Join(">", parsedRecord.NameParts);
                            var category = new EkProductCategory()
                                {
                                    CategoryId = parsedRecord.Record.Id,
                                    Name = new MultiLanguageString()
                                        {
                                            [Languages.RussianCode] = parsedRecord.NameParts.Last(),
                                        },
                                };
                            if (categories.ContainsKey(fullName))
                            {
                                throw new InvalidOperationException("Duplicate!");
                            }

                            categories[fullName] = category;
                            if (depth != 1)
                            {
                                var parentFullName = string.Join(">", parsedRecord.NameParts.Take(parsedRecord.NameParts.Length - 1));
                                var parentCategory = categories[parentFullName];
                                if (parentCategory.Children == null)
                                {
                                    parentCategory.Children = new EkProductCategory[]
                                        {
                                            category,
                                        };
                                }
                                else
                                {
                                    var newChildren = new List<EkProductCategory>(parentCategory.Children)
                                        {
                                            category
                                        };
                                    parentCategory.Children = newChildren.ToArray();
                                }
                            }
                            else
                            {
                                // root category
                                rootCategories.Add(category);
                            }
                        }
                    }

                    // order children by name
                    foreach (var category in categories.Values
                        .Where(x => x.Children?.Length > 0))
                    {
                        category.Children = category.Children
                            .OrderBy(x => x.Name.GetValue(Languages.RussianCode))
                            .ToArray();
                    }

                    // rename auto-parts
                    var autoPartsCategory = rootCategories
                        .Where(x => x.Name[Languages.RussianCode] == "Автозапчасти")
                        .First();
                    autoPartsCategory.Name[Languages.RussianCode] = "Запчасти легковые и грузовые";

                    var json = JsonConvert.SerializeObject(rootCategories);
                }
            }

            Console.WriteLine("Hello World!");
        }
    }
}