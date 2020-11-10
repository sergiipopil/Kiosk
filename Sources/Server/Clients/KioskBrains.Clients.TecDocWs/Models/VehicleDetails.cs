namespace KioskBrains.Clients.TecDocWs.Models
{
    /// <summary>
    /// Model modification details.
    /// </summary>
    public class VehicleDetails
    {
        /// <summary>
        /// <see cref="ModelType"/> Id.
        /// </summary>
        public int CarId { get; set; }

        public int? CcmTech { get; set; }

        /// <summary>
        /// Body type.
        /// </summary>
        public string ConstructionType { get; set; }

        /// <summary>
        /// Number of cylinders.
        /// </summary>
        public int? Cylinder { get; set; }

        public int? CylinderCapacityCcm { get; set; }

        public int? CylinderCapacityLiter { get; set; }

        public string FuelType { get; set; }

        public string FuelTypeProcess { get; set; }

        public string ImpulsionType { get; set; }

        /// <summary>
        /// <see cref="Manufacturer"/> Id.
        /// </summary>
        public int ManuId { get; set; }

        public string ManuName { get; set; }

        /// <summary>
        /// <see cref="Model"/> Id.
        /// </summary>
        public int ModId { get; set; }

        public string ModelName { get; set; }

        public string MotorType { get; set; }

        public int? PowerHpFrom { get; set; }

        public int? PowerHpTo { get; set; }

        public int? PowerKwFrom { get; set; }

        public int? PowerKwTo { get; set; }

        /// <summary>
        /// Specific name of modification. Full name is "<see cref="ModelName"/> <see cref="TypeName"/>".
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// <see cref="ModelType"/> Id.
        /// </summary>
        public int TypeNumber { get; set; }

        public int? Valves { get; set; }

        /// <summary>
        /// Production year-month From.
        /// </summary>
        public string YearOfConstrFrom { get; set; }

        /// <summary>
        /// Production year-month To (null if 'to present time').
        /// </summary>
        public string YearOfConstrTo { get; set; }
    }
}