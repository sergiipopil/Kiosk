namespace KioskBrains.Server.Domain.Entities
{
    public enum UserRoleEnum
    {
        /// <summary>
        /// The role is set to <see cref="CustomerAdmin"/> of system company (KioskBrains).
        /// </summary>
        GlobalAdmin = 1,
        /// <summary>
        /// The role is set to <see cref="CustomerSupport"/> of system company (KioskBrains).
        /// </summary>
        GlobalSupport = 2,

        CustomerAdmin = 10,
        CustomerSupport = 11,

        KioskApp = 30,

        IntegrationClient = 40,
    }
}