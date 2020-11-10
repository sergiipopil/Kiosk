namespace KioskBrains.Clients.OmegaAutoBiz.Models
{
    public class DetailsResponseWrapper<TDetails> : ResponseWrapperBase
    {
        public TDetails Details { get; set; }
    }
}