namespace KioskBrains.Common.KioskConfiguration
{
    public class ComponentStatePropertyLink
    {
        public string ComponentRole { get; set; }

        public string ContractTypeName { get; set; }

        public string PropertyName { get; set; }

        public override string ToString()
        {
            return $"[{ComponentRole}, {ContractTypeName}, {PropertyName}]";
        }
    }
}