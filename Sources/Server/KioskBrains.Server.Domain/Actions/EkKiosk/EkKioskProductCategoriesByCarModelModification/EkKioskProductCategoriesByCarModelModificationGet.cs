using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KioskBrains.Clients.AllegroPl;
using KioskBrains.Clients.AllegroPl.Models;
using KioskBrains.Clients.AllegroPl.ServiceInterfaces;

using KioskBrains.Common.Constants;
using KioskBrains.Common.EK.Api;
using KioskBrains.Server.Domain.Entities;
using KioskBrains.Server.Domain.Entities.DbStorage;
using KioskBrains.Server.Domain.Security;
using KioskBrains.Waf.Actions.Common;
using Microsoft.AspNetCore.Http;

namespace KioskBrains.Server.Domain.Actions.EkKiosk.EkKioskProductCategoriesByCarModelModification
{
    [AuthorizeUser(UserRoleEnum.KioskApp)]
    public class EkKioskProductCategoriesByCarModelModificationGet : WafActionGet<EkKioskProductCategoriesByCarModelModificationGetRequest, EkKioskProductCategoriesByCarModelModificationGetResponse>
    {        
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AllegroPlClient _allegroPlClient;
        private readonly ITranslateService _translateService;

        public EkKioskProductCategoriesByCarModelModificationGet(            
            
            AllegroPlClient allegroPlClient,
            IHttpContextAccessor httpContextAccessor,
            ITranslateService translateService)
        {
            _allegroPlClient = allegroPlClient;
            _httpContextAccessor = httpContextAccessor;
            _translateService = translateService;
        }

        public override async Task<EkKioskProductCategoriesByCarModelModificationGetResponse> ExecuteAsync(EkKioskProductCategoriesByCarModelModificationGetRequest request)
        {
            // cancellation token
            var cancellationToken = _httpContextAccessor.HttpContext?.RequestAborted ?? CancellationToken.None;

            // todo: add cancellationToken support to proxy based clients


            var categories = await _allegroPlClient.GetCategoriesByFullModelName(
                request.FullModelName,
                cancellationToken);

            

            return new EkKioskProductCategoriesByCarModelModificationGetResponse()
                {
                    CategoriesIds = categories.ToArray(),
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