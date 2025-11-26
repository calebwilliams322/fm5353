using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MonteCarloAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddStockTradingSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Positions_PortfolioId_OptionId",
                table: "Positions");

            migrationBuilder.AlterColumn<int>(
                name: "OptionId",
                table: "Trades",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "AssetType",
                table: "Trades",
                type: "integer",
                nullable: false,
                defaultValue: 1); // Default to Option (1) for existing trades

            migrationBuilder.AddColumn<int>(
                name: "StockId",
                table: "Trades",
                type: "integer",
                nullable: true);

            // Update existing trades to have AssetType = Option (1)
            migrationBuilder.Sql("UPDATE \"Trades\" SET \"AssetType\" = 1 WHERE \"OptionId\" IS NOT NULL");

            migrationBuilder.AlterColumn<int>(
                name: "OptionId",
                table: "Positions",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "AssetType",
                table: "Positions",
                type: "integer",
                nullable: false,
                defaultValue: 1); // Default to Option (1) for existing positions

            migrationBuilder.AddColumn<int>(
                name: "StockId",
                table: "Positions",
                type: "integer",
                nullable: true);

            // Update existing positions to have AssetType = Option (1)
            migrationBuilder.Sql("UPDATE \"Positions\" SET \"AssetType\" = 1 WHERE \"OptionId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Trades_StockId",
                table: "Trades",
                column: "StockId");

            migrationBuilder.CreateIndex(
                name: "IX_Positions_PortfolioId_OptionId",
                table: "Positions",
                columns: new[] { "PortfolioId", "OptionId" },
                unique: true,
                filter: "\"OptionId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Positions_PortfolioId_StockId",
                table: "Positions",
                columns: new[] { "PortfolioId", "StockId" },
                unique: true,
                filter: "\"StockId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Positions_StockId",
                table: "Positions",
                column: "StockId");

            migrationBuilder.AddForeignKey(
                name: "FK_Positions_Stocks_StockId",
                table: "Positions",
                column: "StockId",
                principalTable: "Stocks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Trades_Stocks_StockId",
                table: "Trades",
                column: "StockId",
                principalTable: "Stocks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Positions_Stocks_StockId",
                table: "Positions");

            migrationBuilder.DropForeignKey(
                name: "FK_Trades_Stocks_StockId",
                table: "Trades");

            migrationBuilder.DropIndex(
                name: "IX_Trades_StockId",
                table: "Trades");

            migrationBuilder.DropIndex(
                name: "IX_Positions_PortfolioId_OptionId",
                table: "Positions");

            migrationBuilder.DropIndex(
                name: "IX_Positions_PortfolioId_StockId",
                table: "Positions");

            migrationBuilder.DropIndex(
                name: "IX_Positions_StockId",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "AssetType",
                table: "Trades");

            migrationBuilder.DropColumn(
                name: "StockId",
                table: "Trades");

            migrationBuilder.DropColumn(
                name: "AssetType",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "StockId",
                table: "Positions");

            migrationBuilder.AlterColumn<int>(
                name: "OptionId",
                table: "Trades",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "OptionId",
                table: "Positions",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Positions_PortfolioId_OptionId",
                table: "Positions",
                columns: new[] { "PortfolioId", "OptionId" },
                unique: true);
        }
    }
}
