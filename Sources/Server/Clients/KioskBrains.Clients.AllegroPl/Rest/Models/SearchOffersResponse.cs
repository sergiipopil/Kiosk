namespace KioskBrains.Clients.AllegroPl.Rest.Models
{
    internal class SearchOffersResponse
    {
        public SearchOffersResponseItems Items { get; set; }

        public SearchMeta SearchMeta { get; set; }
        public int TotalCount { get; set; }
    }
}