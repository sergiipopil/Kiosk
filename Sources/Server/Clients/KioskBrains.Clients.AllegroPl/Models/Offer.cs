﻿using KioskBrains.Common.Constants;
using KioskBrains.Common.EK.Api;
using System.Collections.Generic;

namespace KioskBrains.Clients.AllegroPl.Models
{
    public class Offer
    {
        public string Id { get; set; }

        public string CategoryId { get; set; }

        public MultiLanguageString Name { get; set; }

        public OfferStateEnum State { get; set; }

        public decimal Price { get; set; }

        public string PriceCurrencyCode { get; set; }

        public int AvailableQuantity { get; set; }
        public decimal? SellerRating { get; set; }

        public MultiLanguageString Description { get; set; }

        public OfferImage[] Images { get; set; }

        public DeliveryOption[] DeliveryOptions { get; set; }
        public List<OfferParameter> Parameters { get; set; }

        public Offer()
        {

        }

        public Offer(OfferExtraData data)
        {
            Parameters = data.Parameters;
            Description = data.Description;
            State = data.State;
            Name = new MultiLanguageString()
            {
                [Languages.PolishCode] = "",
                [Languages.RussianCode] = ""
            };
        }
    }
}