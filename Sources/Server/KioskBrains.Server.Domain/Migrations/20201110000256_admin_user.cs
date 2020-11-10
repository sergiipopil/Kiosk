using KioskBrains.Server.Domain.Security;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KioskBrains.Server.Domain.Migrations
{
    public partial class admin_user : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
INSERT INTO dbo.PortalUsers
(
  CustomerId
 ,Username
 ,PasswordHash
 ,Role
 ,FullName
)
VALUES
(
  (select top 1 Id from Customers) -- CustomerId - int NOT NULL
 ,N'admin@ekgid.com' -- Username - nvarchar(255) NOT NULL
 ,N'{PasswordHelper.GetPasswordHash("change")}' -- PasswordHash - nvarchar(255) NOT NULL
 ,10 -- Role - int NOT NULL
 ,N'Admin' -- FullName - nvarchar(255) NOT NULL
);");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}