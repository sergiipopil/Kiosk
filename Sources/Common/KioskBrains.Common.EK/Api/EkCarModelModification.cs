using System;

namespace KioskBrains.Common.EK.Api
{
    public class EkCarModelModification
    {
        public int Id { get; set; }

        public int ModelId { get; set; }

        public MultiLanguageString Name { get; set; }

        public MultiLanguageString BodyType { get; set; }

        public MultiLanguageString EngineType { get; set; }

        public string EngineCode { get; set; }

        public string EngineCapacity { get; set; }

        public MultiLanguageString DriveType { get; set; }

        public DateTime? ProducedFrom { get; set; }

        public DateTime? ProducedTo { get; set; }
    }
}