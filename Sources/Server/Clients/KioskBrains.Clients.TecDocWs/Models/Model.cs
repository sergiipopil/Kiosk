namespace KioskBrains.Clients.TecDocWs.Models
{
    public class Model
    {
        public int ModelId { get; set; }

        public string Modelname { get; set; }

        /// <summary>
        /// Production year-month From.
        /// </summary>
        public string YearOfConstrFrom { get; set; }

        /// <summary>
        /// Production year-month To (null if 'to present time').
        /// </summary>
        public string YearOfConstrTo { get; set; }

        public override string ToString()
        {
            return $"{Modelname} ({ModelId}, {YearOfConstrFrom}-{YearOfConstrTo})";
        }
    }
}