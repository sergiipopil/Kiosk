using Microsoft.EntityFrameworkCore.Migrations;

namespace KioskBrains.Server.Domain.Migrations
{
    public partial class translate_fields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsInitial",
                table: "Translations",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsUsedForDescription",
                table: "Translations",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsUsedForName",
                table: "Translations",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsUsedForParameter",
                table: "Translations",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Length",
                table: "Translations",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TermPart",
                table: "Translations",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TranslatePart",
                table: "Translations",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsInitial",
                table: "Translations");

            migrationBuilder.DropColumn(
                name: "IsUsedForDescription",
                table: "Translations");

            migrationBuilder.DropColumn(
                name: "IsUsedForName",
                table: "Translations");

            migrationBuilder.DropColumn(
                name: "IsUsedForParameter",
                table: "Translations");

            migrationBuilder.DropColumn(
                name: "Length",
                table: "Translations");

            migrationBuilder.DropColumn(
                name: "TermPart",
                table: "Translations");

            migrationBuilder.DropColumn(
                name: "TranslatePart",
                table: "Translations");
        }
    }
}
