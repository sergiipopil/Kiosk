namespace KioskBrains.Clients.TecDocWs.Models
{
    public class Manufacturer
    {
        public int ManuId { get; set; }

        public string ManuName { get; set; }

        public override string ToString()
        {
            return $"{ManuName} ({ManuId})";
        }
    }
}