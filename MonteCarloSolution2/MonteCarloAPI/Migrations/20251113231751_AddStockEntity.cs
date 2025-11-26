using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MonteCarloAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddStockEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StockId",
                table: "Options",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Stocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Ticker = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CurrentPrice = table.Column<double>(type: "double precision", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stocks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Options_StockId",
                table: "Options",
                column: "StockId");

            migrationBuilder.CreateIndex(
                name: "IX_Stocks_Name",
                table: "Stocks",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Stocks_Ticker",
                table: "Stocks",
                column: "Ticker",
                unique: true);

            // Insert a default stock for existing options
            migrationBuilder.Sql(@"
                INSERT INTO ""Stocks"" (""Ticker"", ""Name"", ""CurrentPrice"", ""Description"", ""CreatedAt"")
                VALUES ('DEFAULT', 'Default Stock', 100.0, 'Default stock for existing options', CURRENT_TIMESTAMP)
            ");

            // Update existing options to point to the default stock (Id=1)
            migrationBuilder.Sql(@"
                UPDATE ""Options""
                SET ""StockId"" = (SELECT ""Id"" FROM ""Stocks"" WHERE ""Ticker"" = 'DEFAULT')
                WHERE ""StockId"" = 0
            ");

            migrationBuilder.AddForeignKey(
                name: "FK_Options_Stocks_StockId",
                table: "Options",
                column: "StockId",
                principalTable: "Stocks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Options_Stocks_StockId",
                table: "Options");

            migrationBuilder.DropTable(
                name: "Stocks");

            migrationBuilder.DropIndex(
                name: "IX_Options_StockId",
                table: "Options");

            migrationBuilder.DropColumn(
                name: "StockId",
                table: "Options");
        }
    }
}
