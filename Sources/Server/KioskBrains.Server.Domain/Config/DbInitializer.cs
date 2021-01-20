using KioskBrains.Server.Domain.Entities.DbStorage;
using System;
using System.Collections.Generic;
using System.Text;

namespace KioskBrains.Server.Domain.Config
{
    public class DbInitializer : IDbInitializer<KioskBrainsContext>
    {
        #region Public Interface

        public void Initialize(KioskBrainsContext context)
        {
            //context.Configuration.AutoDetectChangesEnabled = false;
            // Empty
        }

        public void SeedEverything(KioskBrainsContext dbContext)
        {
            // TODO: implement
        }

        #endregion
    }
}
