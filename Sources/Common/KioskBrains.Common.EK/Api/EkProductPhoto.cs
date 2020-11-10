namespace KioskBrains.Common.EK.Api
{
    public class EkProductPhoto
    {
        /// <summary>
        /// Can be null if there is no thumbnail - <see cref="Url"/> should be used in such cases.
        /// </summary>
        public string ThumbnailUrl { get; set; }

        public string Url { get; set; }
    }
}