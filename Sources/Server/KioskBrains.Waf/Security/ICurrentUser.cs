namespace KioskBrains.Waf.Security
{
    public interface ICurrentUser
    {
        int Id { get; }

        bool IsInRole(string role);

        TValueType GetClaimValue<TValueType>(string claimType, bool mandatory = true);
    }
}