using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MonteCarloAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddPricingHistoryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PricingHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OptionId = table.Column<int>(type: "integer", nullable: false),
                    InitialPrice = table.Column<double>(type: "double precision", nullable: false),
                    Volatility = table.Column<double>(type: "double precision", nullable: false),
                    RiskFreeRate = table.Column<double>(type: "double precision", nullable: false),
                    TimeToExpiry = table.Column<double>(type: "double precision", nullable: false),
                    TimeSteps = table.Column<int>(type: "integer", nullable: false),
                    NumberOfPaths = table.Column<int>(type: "integer", nullable: false),
                    UseMultithreading = table.Column<bool>(type: "boolean", nullable: false),
                    SimMode = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<double>(type: "double precision", nullable: false),
                    StandardError = table.Column<double>(type: "double precision", nullable: false),
                    ExecutionTimeMs = table.Column<double>(type: "double precision", nullable: false),
                    Delta = table.Column<double>(type: "double precision", nullable: true),
                    Gamma = table.Column<double>(type: "double precision", nullable: true),
                    Vega = table.Column<double>(type: "double precision", nullable: true),
                    Theta = table.Column<double>(type: "double precision", nullable: true),
                    Rho = table.Column<double>(type: "double precision", nullable: true),
                    PricedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    RequestSource = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PricingHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PricingHistory_Options_OptionId",
                        column: x => x.OptionId,
                        principalTable: "Options",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PricingHistory_OptionId",
                table: "PricingHistory",
                column: "OptionId");

            migrationBuilder.CreateIndex(
                name: "IX_PricingHistory_OptionId_PricedAt",
                table: "PricingHistory",
                columns: new[] { "OptionId", "PricedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PricingHistory_PricedAt",
                table: "PricingHistory",
                column: "PricedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PricingHistory");
        }
    }
}
