using System;

namespace KioskBrains.Waf.Helpers.Exceptions
{
    public class EntityNotFoundException : Exception
    {
        public EntityNotFoundException(string message)
            : base(message)
        {
        }

        public static EntityNotFoundException Create<TEntity>(int entityId)
        {
            return new EntityNotFoundException($"Entity {typeof(TEntity).Name} with id '{entityId}' was not found.");
        }
    }
}