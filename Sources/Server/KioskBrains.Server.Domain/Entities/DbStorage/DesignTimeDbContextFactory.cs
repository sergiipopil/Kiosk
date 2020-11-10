using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace KioskBrains.Server.Domain.Entities.DbStorage
{
    internal class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<KioskBrainsContext>
    {
        public KioskBrainsContext CreateDbContext(string[] args)
        {
            // get connection string
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{environmentName}.json", true)
                .Build();
            var connectionString = configuration.GetConnectionString("KioskBrainsContext");

            var optionsBuilder = new DbContextOptionsBuilder<KioskBrainsContext>();

            optionsBuilder.UseSqlServer(
                connectionString,
                options =>
                    {
                        // set timeout 10 minutes
                        var timeoutSeconds = (int)TimeSpan.FromMinutes(10).TotalSeconds;
                        options.CommandTimeout(timeoutSeconds);
                    });

            return new KioskBrainsContext(optionsBuilder.Options);
        }
    }
}