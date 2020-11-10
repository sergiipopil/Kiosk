using System.Collections.Generic;
using KioskBrains.Common.Search;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;

namespace KioskBrains.Server.EK.Common.Search.Models
{
    [SerializePropertyNamesAsCamelCase]
    public class IndexProduct
    {
        [System.ComponentModel.DataAnnotations.Key]
        [IsFilterable]
        [IsSortable]
        public string Key { get; set; }

        /// <summary>
        /// Total UTC minutes of last update.
        /// </summary>
        [IsFilterable]
        [IsSortable]
        public long UpdatedOnUtcTimestamp { get; set; }

        [IsFilterable]
        public int Source { get; set; }

        public string SourceId { get; set; }

        public string PartNumber { get; set; }

        [IsSearchable]
        public string CleanedPartNumber { get; set; }

        public string BrandName { get; set; }

        [IsSearchable]
        public string CleanedBrandPartNumber { get; set; }

        [IsSearchable]
        public string Name_ru { get; set; }

        [IsSearchable]
        public string Name_uk { get; set; }

        [IsSearchable]
        public string Description_ru { get; set; }

        [IsSearchable]
        public string Description_uk { get; set; }

        public string SpecificationsJson_ru { get; set; }

        public string SpecificationsJson_uk { get; set; }

        [IsSortable]
        public double? Price { get; set; }

        [IsFilterable]
        public string ThumbnailUrl { get; set; }

        [IsFilterable]
        public bool? IsThumbnailSearchProvided { get; set; }

        public string PhotosJson { get; set; }

        public string ProductionYear { get; set; }

        public static Suggester[] GetSuggesters()
        {
            return new[]
                {
                    // camel-case field names
                    new Suggester(SearchConstants.SuggesterName, "name_ru", "description_ru", "name_uk", "description_uk"),
                };
        }

        public static ScoringProfile[] GetScoringProfiles()
        {
            const double NameWeight = 5;
            const double DescriptionWeight = 1;

            return new[]
                {
                    // camel-case field names
                    new ScoringProfile(
                        SearchConstants.BoostNameScoringProfileName,
                        new TextWeights(new Dictionary<string, double>()
                            {
                                ["name_ru"] = NameWeight,
                                ["description_ru"] = DescriptionWeight,
                                ["name_uk"] = NameWeight,
                                ["description_uk"] = DescriptionWeight,
                            })),
                };
        }
    }
}