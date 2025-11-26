using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MonteCarloAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Options",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OptionType = table.Column<int>(type: "integer", nullable: false),
                    Strike = table.Column<double>(type: "double precision", nullable: false),
                    IsCall = table.Column<bool>(type: "boolean", nullable: false),
                    AveragingType = table.Column<int>(type: "integer", nullable: false),
                    ObservationFrequency = table.Column<int>(type: "integer", nullable: false),
                    DigitalCondition = table.Column<int>(type: "integer", nullable: false),
                    BarrierOptionType = table.Column<int>(type: "integer", nullable: false),
                    BarrierDir = table.Column<int>(type: "integer", nullable: false),
                    BarrierLevel = table.Column<double>(type: "double precision", nullable: false),
                    LookbackOptionType = table.Column<int>(type: "integer", nullable: false),
                    RangeObservationFrequency = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Options", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Options_CreatedAt",
                table: "Options",
                column: "CreatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Options");
        }
    }
}
