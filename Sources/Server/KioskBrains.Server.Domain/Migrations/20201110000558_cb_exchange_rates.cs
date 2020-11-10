using Microsoft.EntityFrameworkCore.Migrations;

namespace KioskBrains.Server.Domain.Migrations
{
    public partial class cb_exchange_rates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
INSERT INTO dbo.CentralBankExchangeRates
(
  LocalCurrencyCode
 ,ForeignCurrencyCode
 ,DefaultOrder
)
VALUES
(
  N'UAH' -- LocalCurrencyCode - nvarchar(3) NOT NULL
 ,N'PLN' -- ForeignCurrencyCode - nvarchar(3) NOT NULL
 ,1 -- DefaultOrder - int NOT NULL
);");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}