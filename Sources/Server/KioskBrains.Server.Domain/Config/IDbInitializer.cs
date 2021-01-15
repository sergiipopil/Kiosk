﻿using Microsoft.EntityFrameworkCore;

namespace KioskBrains.Server.Domain.Config
{
    public interface IDbInitializer<T>
        where T : DbContext
    {
        void SeedEverything(T dbContext);

        void Initialize(T dbContext);
    }
}
