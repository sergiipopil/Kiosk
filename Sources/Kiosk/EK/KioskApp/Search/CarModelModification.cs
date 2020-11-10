using System.Linq;
using KioskBrains.Common.Constants;
using KioskBrains.Common.Contracts;
using KioskBrains.Common.EK.Api;
using KioskBrains.Kiosk.Helpers.Ui.Binding;

namespace KioskApp.Search
{
    public class CarModelModification : UiBindableObject
    {
        public CarModelModification(EkCarModelModification ekCarModelModification)
        {
            Assure.ArgumentNotNull(ekCarModelModification, nameof(ekCarModelModification));

            Id = ekCarModelModification.Id;
            ModelId = ekCarModelModification.ModelId;
            Name = ekCarModelModification.Name?.GetValue(Languages.RussianCode);
            BodyType = ekCarModelModification.BodyType?.GetValue(Languages.RussianCode);
            EngineType = ekCarModelModification.EngineType?.GetValue(Languages.RussianCode);
            EngineCode = ekCarModelModification.EngineCode;
            EngineCapacity = ekCarModelModification.EngineCapacity;
            DriveType = ekCarModelModification.DriveType?.GetValue(Languages.RussianCode);
            ProducedPeriod = string.Join("-", new[]
                    {
                        ekCarModelModification.ProducedFrom?.ToString("yyyy.MM"),
                        ekCarModelModification.ProducedTo?.ToString("yyyy.MM") ?? "наст. время"
                    }
                .Where(x => x != null));
        }

        public int Id { get; set; }

        public int ModelId { get; set; }

        public string Name { get; set; }

        public string BodyType { get; set; }

        public string EngineType { get; set; }

        public string EngineCode { get; set; }

        public string EngineCapacity { get; set; }

        public string DriveType { get; set; }

        public string ProducedPeriod { get; set; }
    }
}