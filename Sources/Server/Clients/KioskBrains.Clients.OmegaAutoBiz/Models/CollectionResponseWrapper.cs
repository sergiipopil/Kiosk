namespace KioskBrains.Clients.OmegaAutoBiz.Models
{
    public class CollectionResponseWrapper<TRecord> : ResponseWrapperBase
    {
        public TRecord[] Result { get; set; }

        public int Total { get; set; }
    }
}