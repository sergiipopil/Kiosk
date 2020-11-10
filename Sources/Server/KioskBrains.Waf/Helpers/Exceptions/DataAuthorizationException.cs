using System;

namespace KioskBrains.Waf.Helpers.Exceptions
{
    public class DataAuthorizationException : Exception
    {
        public DataAuthorizationException(string message)
            : base(message)
        {
        }

        public static DataAuthorizationException CreateInvalidEntityKey<TEntity>(int entityId)
        {
            return new DataAuthorizationException($"Entity key '{entityId}' for {typeof(TEntity).Name} was not found.");
        }
    }
}