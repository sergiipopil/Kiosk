using System.Linq;

namespace KioskBrains.Clients.TecDocWs.Models
{
    /// <summary>
    /// Model modification.
    /// </summary>
    public class ModelType
    {
        /// <summary>
        /// <see cref="ModelType"/> Id.
        /// </summary>
        public int CarId { get; set; }

        /// <summary>
        /// Modification name.
        /// </summary>
        public string CarName { get; set; }

        public MotorCodeInfo[] MotorCodes { get; set; }

        public string[] GetMotorCodes()
        {
            return MotorCodes?
                       .Select(x => x.MotorCode)
                       .ToArray()
                   ?? new string[0];
        }

        public string GetMotorCodesString()
        {
            return string.Join(", ", GetMotorCodes());
        }

        public VehicleDetails VehicleDetails { get; set; }

        public override string ToString()
        {
            return $"{VehicleDetails?.ManuName} {VehicleDetails?.ModelName} {VehicleDetails?.TypeName} ({CarId}) {VehicleDetails?.MotorType} {GetMotorCodesString()} {VehicleDetails?.CylinderCapacityCcm}ccm {VehicleDetails?.PowerHpFrom}HP {VehicleDetails?.ImpulsionType} {VehicleDetails?.YearOfConstrFrom}-{VehicleDetails?.YearOfConstrTo}";
        }
    }
}