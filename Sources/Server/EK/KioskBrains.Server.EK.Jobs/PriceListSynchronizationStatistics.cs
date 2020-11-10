namespace KioskBrains.Server.EK.Jobs
{
    public class PriceListSynchronizationStatistics
    {
        public int Received { get; set; }

        public int Removed { get; set; }

        public override string ToString()
        {
            return $"received: {Received}, removed: {Removed}";
        }
    }
}