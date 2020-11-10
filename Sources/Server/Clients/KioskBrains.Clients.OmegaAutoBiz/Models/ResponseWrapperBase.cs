namespace KioskBrains.Clients.OmegaAutoBiz.Models
{
    public abstract class ResponseWrapperBase
    {
        public bool IsSuccess => Errors == null
                                 || Errors.Length == 0;

        public ErrorMessage[] Errors { get; set; }
    }
}