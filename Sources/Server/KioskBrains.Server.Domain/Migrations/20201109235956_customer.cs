using Microsoft.EntityFrameworkCore.Migrations;

namespace KioskBrains.Server.Domain.Migrations
{
    public partial class customer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
INSERT INTO dbo.Customers
(
  IsSystem
 ,Name
 ,SupportPhone
 ,TimeZoneName
)
VALUES
(
  1 -- IsSystem - bit NOT NULL
 ,N'EK4CAR' -- Name - nvarchar(255) NOT NULL
 ,N'+38 (050) 508 81 08' -- SupportPhone - nvarchar(50)
 ,N'FLE Standard Time' -- TimeZoneName - nvarchar(50)
);");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
