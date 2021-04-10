using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication.Classes;

namespace WebApplication.Models
{
    public class PaymentViewModel
    {
        public PaymentLinkData Privat24Data { get; set; }
        public PaymentLinkData QRData { get; set; }
        public PaymentLinkData APayData { get; set; }
        public PaymentLinkData GPayData { get; set; }
        public PaymentLinkData cardData { get; set; }
        public PaymentLinkData liqpayData { get; set; }
        public PaymentLinkData masterpassData { get; set; }
        public PaymentLinkData moment_partData { get; set; }
        public PaymentLinkData cashData { get; set; }
        public PaymentLinkData invoiceData { get; set; }
    }
}
