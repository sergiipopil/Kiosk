using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication.Classes
{
    public class OrderPaymentSettings
    {
        public string public_key { get; set; }
        public string version { get; set; }
        public string action { get; set; }
        public string amount { get; set; }
        public string currency { get; set; }
        public string description { get; set; }
        public string order_id { get; set; }
        public string language { get; set; }
        public string paytypes { get; set; }
        public OrderPaymentSettings(string public_keyVal, string versionVal, string actionVal, string amountVal, string currencyVal, string descriptionVal, string order_idVal, string languageVal, string paytypesVal)
        {
            public_key = public_keyVal;
            version = versionVal;
            action = actionVal;
            amount = amountVal;
            currency = currencyVal;
            description = descriptionVal;
            order_id = order_idVal;
            language = languageVal;
            paytypes = paytypesVal;
        }
    }
}
