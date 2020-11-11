using System.Collections.Generic;

namespace KioskBrains.Common.EK.Api
{
    public class EkKioskAllegroProductDescriptionGetResponse
    {
        public MultiLanguageString Description { get; set; }
        public IList<OfferParameter> Parameters { get; set; }
    }
}