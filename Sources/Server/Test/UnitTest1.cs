using Microsoft.VisualStudio.TestTools.UnitTesting;
using KioskBrains.Clients.AllegroPl.Rest;
using System.Threading;
using System.Linq;
using KioskBrains.Common.EK.Api;
using KioskBrains.Clients.AllegroPl.Models;
using KioskBrains.Common.Constants;
using System.Collections.Generic;

namespace Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var rc = new KioskBrains.Clients.AllegroPl.Rest.RestClient("f85d6f413201484fbc1fc008b1f0403d", "cmA13WEVhsIMSp0CPhEqee2m1ssq0aThxDMSFxzCFVkvmwur3MTHZM332oqNvBNj");
            var categoriesResponse = rc.GetCategoriesByModel("kia picanto", CancellationToken.None).Result;

            var categories = categoriesResponse.Matching_Categories.ToList();
            var newColl = new List<Category>();
            foreach (var c in categories)
            {
                ProcessCategories(newColl, c);
            }
            categories = newColl;

            if (categories == null
                || !categories.Any())
            {
                 GetEmptyResponse();
            }

            const string RootCategoryParentNodeId = "3";
            var categoriesByParentId = categories
                .GroupBy(x => x.Parent.Id)
                .ToDictionary(
                    x => x.Key,
                    x => x
                        .ToArray());

            var ekProductCategories = GetEkProductCategoryChildren(RootCategoryParentNodeId, categoriesByParentId);
        }

       
        private void ProcessCategories(IList<Category> categories, Category category)
        {
            if (category.Parent != null)
            {
                ProcessCategories(categories, category.Parent);
            }

            if (categories.FirstOrDefault(x=>x.Id == category.Id) != null)
            {
                return;
            }
            categories.Add(category);            
        }

        private EkProductCategory[] GetEkProductCategoryChildren(string parentId, Dictionary<string, Category[]> categoriesByParentId)
        {
            var childCategories = categoriesByParentId.GetValueOrDefault(parentId);
            if (childCategories == null
                || childCategories.Length == 0)
            {
                return null;
            }

            return childCategories
                .Select(x => new EkProductCategory()
                {
                    CategoryId = x.Id.ToString(),
                    Name = new MultiLanguageString()
                    {
                        // sometimes TecDoc names start from lower case
                        [Languages.RussianCode] = FirstLetterToUpperCase(x.Name),
                    },
                    Children = GetEkProductCategoryChildren(x.Id, categoriesByParentId),
                })
                .ToArray();
        }

        private string FirstLetterToUpperCase(string categoryName)
        {
            if (string.IsNullOrEmpty(categoryName)
                || char.IsUpper(categoryName[0]))
            {
                return categoryName;
            }

            return categoryName[0].ToString().ToUpper() + categoryName.Substring(1);
        }

        private EkKioskProductCategoriesByCarModelModificationGetResponse GetEmptyResponse()
        {
            return new EkKioskProductCategoriesByCarModelModificationGetResponse()
            {
                Categories = new EkProductCategory[0],
            };
        }
    }
}
