using KioskBrains.Common.EK.Api;
using System;
using System.Collections.Generic;
using System.Text;

namespace KioskBrains.Clients.AllegroPl.Models
{
    public class OfferExtraData
    {
        public MultiLanguageString Description { get; set; }
        public List<OfferParameter> Parameters { get; set; }

        public OfferStateEnum State { get; set; }
    }
}
