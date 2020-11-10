namespace KioskBrains.Clients.TecDocWs.Models
{
    public class SearchByVinRecord
    {
        public int ManuId { get; set; }

        public int ModelId { get; set; }

        public int CarId { get; set; }

        public string VehicleTypeDescription { get; set; }

        public string CarName { get; set; }
    }
}