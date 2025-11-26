using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MonteCarloAPI.Migrations
{
    /// <inheritdoc />
    public partial class ChangeTickerUniqueConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Stocks_Ticker",
                table: "Stocks");

            migrationBuilder.CreateIndex(
                name: "IX_Stocks_Ticker_ExchangeId",
                table: "Stocks",
                columns: new[] { "Ticker", "ExchangeId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Stocks_Ticker_ExchangeId",
                table: "Stocks");

            migrationBuilder.CreateIndex(
                name: "IX_Stocks_Ticker",
                table: "Stocks",
                column: "Ticker",
                unique: true);
        }
    }
}
