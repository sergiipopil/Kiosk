using System.Collections.Generic;

namespace KioskBrains.Clients.ElitUa
{
    public class ElitPriceList
    {
        public bool IsSuccess { get; set; }

        public string PriceListId { get; set; }

        public List<ElitPriceListRecord> Records { get; set; }

        public string StatusMessage { get; set; }

        internal static ElitPriceList GetForError(string errorMessage)
        {
            return new ElitPriceList()
                {
                    IsSuccess = false,
                    StatusMessage = errorMessage,
                };
        }
    }
}