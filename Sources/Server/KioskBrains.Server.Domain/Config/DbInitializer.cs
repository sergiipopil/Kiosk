using System;
using System.Collections.Generic;
using System.Text;

namespace KioskBrains.Server.Domain.Config
{
    public class DbInitializer : IDbInitializer<SSDbContext>
    {
        #region Public Interface

        public void Initialize(SSDbContext context)
        {
            //context.Configuration.AutoDetectChangesEnabled = false;
            // Empty
        }

        public void SeedEverything(SSDbContext dbContext)
        {
            // TODO: implement
        }

        #endregion
    }
}
