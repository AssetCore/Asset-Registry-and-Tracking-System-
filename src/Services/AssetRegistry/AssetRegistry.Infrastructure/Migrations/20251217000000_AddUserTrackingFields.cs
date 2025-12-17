using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssetRegistry.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserTrackingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "assets",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "assets",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2025, 12, 17, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "assets",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "assets",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "assets");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "assets");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "assets");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "assets");
        }
    }
}
