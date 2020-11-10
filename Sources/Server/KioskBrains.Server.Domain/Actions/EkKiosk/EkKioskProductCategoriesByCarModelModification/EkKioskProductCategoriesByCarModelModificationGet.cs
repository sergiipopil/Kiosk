using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KioskBrains.Clients.TecDocWs;
using KioskBrains.Clients.TecDocWs.Models;
using KioskBrains.Common.Constants;
using KioskBrains.Common.EK.Api;
using KioskBrains.Server.Domain.Entities;
using KioskBrains.Server.Domain.Security;
using KioskBrains.Waf.Actions.Common;
using Microsoft.AspNetCore.Http;

namespace KioskBrains.Server.Domain.Actions.EkKiosk.EkKioskProductCategoriesByCarModelModification
{
    [AuthorizeUser(UserRoleEnum.KioskApp)]
    public class EkKioskProductCategoriesByCarModelModificationGet : WafActionGet<EkKioskProductCategoriesByCarModelModificationGetRequest, EkKioskProductCategoriesByCarModelModificationGetResponse>
    {
        private readonly TecDocWsClient _tecDocWsClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EkKioskProductCategoriesByCarModelModificationGet(
            TecDocWsClient tecDocWsClient,
            IHttpContextAccessor httpContextAccessor)
        {
            _tecDocWsClient = tecDocWsClient;
            _httpContextAccessor = httpContextAccessor;
        }

        public override async Task<EkKioskProductCategoriesByCarModelModificationGetResponse> ExecuteAsync(EkKioskProductCategoriesByCarModelModificationGetRequest request)
        {
            // cancellation token
            var cancellationToken = _httpContextAccessor.HttpContext?.RequestAborted ?? CancellationToken.None;

            // todo: add cancellationToken support to proxy based clients

            var modelTypeId = request.ModificationId;

            // request for cars first
            var categories = await _tecDocWsClient.GetCategoriesAsync(CarTypeEnum.Car, modelTypeId, null, childNodes: true);
            if (categories == null
                || categories.Length == 0)
            {
                // then request for trucks
                categories = await _tecDocWsClient.GetCategoriesAsync(CarTypeEnum.Truck, modelTypeId, null, childNodes: true);
            }

            if (categories == null
                || categories.Length == 0)
            {
                return GetEmptyResponse();
            }

            const int RootCategoryParentNodeId = 0;
            var categoriesByParentId = categories
                .GroupBy(x => x.ParentNodeId)
                .ToDictionary(
                    x => x.Key ?? RootCategoryParentNodeId,
                    x => x
                        .OrderBy(c => c.AssemblyGroupName)
                        .ToArray());

            var ekProductCategories = GetEkProductCategoryChildren(RootCategoryParentNodeId, categoriesByParentId);

            return new EkKioskProductCategoriesByCarModelModificationGetResponse()
                {
                    Categories = ekProductCategories,
                };
        }

        private EkProductCategory[] GetEkProductCategoryChildren(int parentId, Dictionary<int, Category[]> categoriesByParentId)
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
                        CategoryId = x.AssemblyGroupNodeId.ToString(),
                        Name = new MultiLanguageString()
                            {
                                // sometimes TecDoc names start from lower case
                                [Languages.RussianCode] = FirstLetterToUpperCase(x.AssemblyGroupName),
                            },
                        Children = GetEkProductCategoryChildren(x.AssemblyGroupNodeId, categoriesByParentId),
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