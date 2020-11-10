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

namespace KioskBrains.Server.Domain.Actions.EkKiosk.EkKioskProductSearchByVinCode
{
    [AuthorizeUser(UserRoleEnum.KioskApp)]
    public class EkKioskProductSearchByVinCodeGet : WafActionGet<EkKioskProductSearchByVinCodeGetRequest, EkKioskProductSearchByVinCodeGetResponse>
    {
        private readonly TecDocWsClient _tecDocWsClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EkKioskProductSearchByVinCodeGet(
            TecDocWsClient tecDocWsClient,
            IHttpContextAccessor httpContextAccessor)
        {
            _tecDocWsClient = tecDocWsClient;
            _httpContextAccessor = httpContextAccessor;
        }

        public override async Task<EkKioskProductSearchByVinCodeGetResponse> ExecuteAsync(EkKioskProductSearchByVinCodeGetRequest request)
        {
            if (string.IsNullOrEmpty(request.VinCode)
                || request.VinCode.Length < 8)
            {
                // invalid vin-code
                return GetEmptyResponse();
            }

            // cancellation token
            var cancellationToken = _httpContextAccessor.HttpContext?.RequestAborted ?? CancellationToken.None;

            // todo: add cancellationToken support to proxy based clients

            var records = await _tecDocWsClient.SearchByVinAsync(request.VinCode);
            if (records.Length == 0)
            {
                // no results by passed vin-code
                return GetEmptyResponse();
            }

            var modelTypes = await _tecDocWsClient.GetModelTypesAsync(
                records
                    .Select(x => x.CarId)
                    .ToArray());
            if (modelTypes.Length == 0)
            {
                // no results by passed vin-code
                return GetEmptyResponse();
            }

            var modifications = modelTypes
                .Where(x => x.VehicleDetails != null)
                .Select(x =>
                    {
                        var modificationName = $"{x.VehicleDetails.ManuName} {x.VehicleDetails.ModelName} {x.VehicleDetails.TypeName}";
                        var bodyType = x.VehicleDetails.ConstructionType;
                        var engineType = x.VehicleDetails.MotorType;
                        var engineCode = x.GetMotorCodesString();
                        var capacityCcm = x.VehicleDetails.CylinderCapacityCcm;
                        var engineCapacity = capacityCcm == null
                            ? null
                            : $"{capacityCcm} ccm";
                        var driveType = x.VehicleDetails.ImpulsionType;
                        return new EkCarModelModification()
                            {
                                Id = x.CarId,
                                ModelId = x.VehicleDetails.ModId,
                                Name = new MultiLanguageString()
                                    {
                                        [Languages.RussianCode] = modificationName,
                                    },
                                BodyType = new MultiLanguageString()
                                    {
                                        [Languages.RussianCode] = bodyType,
                                    },
                                EngineType = new MultiLanguageString()
                                    {
                                        [Languages.RussianCode] = engineType,
                                    },
                                EngineCode = engineCode,
                                EngineCapacity = engineCapacity,
                                DriveType = new MultiLanguageString()
                                    {
                                        [Languages.RussianCode] = driveType,
                                    },
                                ProducedFrom = ConversionHelper.ConvertStringToYearMonth(x.VehicleDetails.YearOfConstrFrom),
                                ProducedTo = ConversionHelper.ConvertStringToYearMonth(x.VehicleDetails.YearOfConstrTo),
                            };
                    })
                .ToArray();

            return new EkKioskProductSearchByVinCodeGetResponse()
                {
                    ModelModifications = modifications,
                };
        }

        private EkKioskProductSearchByVinCodeGetResponse GetEmptyResponse()
        {
            return new EkKioskProductSearchByVinCodeGetResponse()
                {
                    ModelModifications = new EkCarModelModification[0],
                };
        }
    }
}