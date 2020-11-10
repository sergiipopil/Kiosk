using System;

namespace KioskApp.Ek.Catalog.AutoParts
{
    public class SearchTypeSelectedEventArgs : EventArgs
    {
        public SearchTypeEnum SearchType { get; }

        public SearchTypeSelectedEventArgs(SearchTypeEnum searchType)
        {
            SearchType = searchType;
        }
    }
}