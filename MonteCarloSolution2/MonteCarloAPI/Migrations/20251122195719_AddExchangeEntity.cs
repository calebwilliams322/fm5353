using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MonteCarloAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddExchangeEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Create the Exchanges table first
            migrationBuilder.CreateTable(
                name: "Exchanges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exchanges", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Exchanges_Name",
                table: "Exchanges",
                column: "Name",
                unique: true);

            // Step 2: Seed the default NYSE exchange
            migrationBuilder.Sql(@"
                INSERT INTO ""Exchanges"" (""Name"", ""Description"", ""Country"", ""Currency"", ""CreatedAt"")
                VALUES ('NYSE', 'New York Stock Exchange', 'USA', 'USD', CURRENT_TIMESTAMP);
            ");

            // Step 3: Add ExchangeId column as nullable first
            migrationBuilder.AddColumn<int>(
                name: "ExchangeId",
                table: "Stocks",
                type: "integer",
                nullable: true);

            // Step 4: Update all existing stocks to reference NYSE (first exchange, ID 1)
            migrationBuilder.Sql(@"
                UPDATE ""Stocks"" SET ""ExchangeId"" = (SELECT ""Id"" FROM ""Exchanges"" WHERE ""Name"" = 'NYSE' LIMIT 1);
            ");

            // Step 5: Make ExchangeId non-nullable
            migrationBuilder.AlterColumn<int>(
                name: "ExchangeId",
                table: "Stocks",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            // Step 6: Create index and foreign key
            migrationBuilder.CreateIndex(
                name: "IX_Stocks_ExchangeId",
                table: "Stocks",
                column: "ExchangeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Stocks_Exchanges_ExchangeId",
                table: "Stocks",
                column: "ExchangeId",
                principalTable: "Exchanges",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Stocks_Exchanges_ExchangeId",
                table: "Stocks");

            migrationBuilder.DropTable(
                name: "Exchanges");

            migrationBuilder.DropIndex(
                name: "IX_Stocks_ExchangeId",
                table: "Stocks");

            migrationBuilder.DropColumn(
                name: "ExchangeId",
                table: "Stocks");
        }
    }
}
