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

            migrationBuilder.CreateTable(
                name: "Portfolios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Cash = table.Column<double>(type: "double precision", nullable: false, defaultValue: 0.0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Portfolios", x => x.Id);
                });

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
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExchangeId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Stocks_Exchanges_ExchangeId",
                        column: x => x.ExchangeId,
                        principalTable: "Exchanges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Options",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StockId = table.Column<int>(type: "integer", nullable: false),
                    OptionType = table.Column<int>(type: "integer", nullable: false),
                    Strike = table.Column<double>(type: "double precision", nullable: false),
                    IsCall = table.Column<bool>(type: "boolean", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AveragingType = table.Column<int>(type: "integer", nullable: true),
                    ObservationFrequency = table.Column<int>(type: "integer", nullable: true),
                    DigitalCondition = table.Column<int>(type: "integer", nullable: true),
                    BarrierOptionType = table.Column<int>(type: "integer", nullable: true),
                    BarrierDir = table.Column<int>(type: "integer", nullable: true),
                    BarrierLevel = table.Column<double>(type: "double precision", nullable: true),
                    LookbackOptionType = table.Column<int>(type: "integer", nullable: true),
                    RangeObservationFrequency = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Options", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Options_Stocks_StockId",
                        column: x => x.StockId,
                        principalTable: "Stocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Positions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PortfolioId = table.Column<int>(type: "integer", nullable: false),
                    AssetType = table.Column<int>(type: "integer", nullable: false),
                    StockId = table.Column<int>(type: "integer", nullable: true),
                    OptionId = table.Column<int>(type: "integer", nullable: true),
                    NetQuantity = table.Column<int>(type: "integer", nullable: false),
                    AverageCost = table.Column<double>(type: "double precision", nullable: false),
                    TotalCost = table.Column<double>(type: "double precision", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Positions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Positions_Options_OptionId",
                        column: x => x.OptionId,
                        principalTable: "Options",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Positions_Portfolios_PortfolioId",
                        column: x => x.PortfolioId,
                        principalTable: "Portfolios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Positions_Stocks_StockId",
                        column: x => x.StockId,
                        principalTable: "Stocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

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

            migrationBuilder.CreateTable(
                name: "Trades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PortfolioId = table.Column<int>(type: "integer", nullable: false),
                    AssetType = table.Column<int>(type: "integer", nullable: false),
                    StockId = table.Column<int>(type: "integer", nullable: true),
                    OptionId = table.Column<int>(type: "integer", nullable: true),
                    TradeType = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<double>(type: "double precision", nullable: false),
                    TotalCost = table.Column<double>(type: "double precision", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    TradeDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Trades_Options_OptionId",
                        column: x => x.OptionId,
                        principalTable: "Options",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Trades_Portfolios_PortfolioId",
                        column: x => x.PortfolioId,
                        principalTable: "Portfolios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Trades_Stocks_StockId",
                        column: x => x.StockId,
                        principalTable: "Stocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Exchanges_Name",
                table: "Exchanges",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Options_CreatedAt",
                table: "Options",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Options_StockId",
                table: "Options",
                column: "StockId");

            migrationBuilder.CreateIndex(
                name: "IX_Portfolios_Name",
                table: "Portfolios",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Positions_OptionId",
                table: "Positions",
                column: "OptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Positions_PortfolioId",
                table: "Positions",
                column: "PortfolioId");

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

            migrationBuilder.CreateIndex(
                name: "IX_Stocks_ExchangeId",
                table: "Stocks",
                column: "ExchangeId");

            migrationBuilder.CreateIndex(
                name: "IX_Stocks_Name",
                table: "Stocks",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Stocks_Ticker_ExchangeId",
                table: "Stocks",
                columns: new[] { "Ticker", "ExchangeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trades_OptionId",
                table: "Trades",
                column: "OptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Trades_PortfolioId",
                table: "Trades",
                column: "PortfolioId");

            migrationBuilder.CreateIndex(
                name: "IX_Trades_PortfolioId_TradeDate",
                table: "Trades",
                columns: new[] { "PortfolioId", "TradeDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Trades_StockId",
                table: "Trades",
                column: "StockId");

            migrationBuilder.CreateIndex(
                name: "IX_Trades_TradeDate",
                table: "Trades",
                column: "TradeDate");

            // ============================================================
            // SEED DATA
            // ============================================================

            // 1. Seed Exchange (NYSE)
            migrationBuilder.Sql(@"
                INSERT INTO ""Exchanges"" (""Name"", ""Description"", ""Country"", ""Currency"", ""CreatedAt"")
                VALUES ('NYSE', 'New York Stock Exchange', 'USA', 'USD', CURRENT_TIMESTAMP);
            ");

            // 2. Seed 20 common stocks (all referencing NYSE with ExchangeId = 1)
            migrationBuilder.Sql(@"
                INSERT INTO ""Stocks"" (""Ticker"", ""Name"", ""CurrentPrice"", ""Description"", ""ExchangeId"", ""CreatedAt"", ""UpdatedAt"")
                VALUES
                    ('AAPL', 'Apple Inc.', 230.00, 'Technology - Consumer Electronics', 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    ('MSFT', 'Microsoft Corporation', 430.00, 'Technology - Software', 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    ('GOOGL', 'Alphabet Inc.', 175.00, 'Technology - Internet Services', 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    ('META', 'Meta Platforms Inc.', 580.00, 'Technology - Social Media', 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    ('NVDA', 'NVIDIA Corporation', 140.00, 'Technology - Semiconductors', 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    ('TSLA', 'Tesla Inc.', 350.00, 'Automotive - Electric Vehicles', 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    ('AMZN', 'Amazon.com Inc.', 210.00, 'Technology - E-commerce', 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    ('JPM', 'JPMorgan Chase & Co.', 150.00, 'Financial Services - Banking', 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    ('GS', 'Goldman Sachs Group Inc.', 380.00, 'Financial Services - Investment Banking', 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    ('BAC', 'Bank of America Corporation', 35.00, 'Financial Services - Banking', 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    ('WMT', 'Walmart Inc.', 160.00, 'Retail - Discount Stores', 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    ('KO', 'The Coca-Cola Company', 60.00, 'Consumer Goods - Beverages', 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    ('PG', 'Procter & Gamble Co.', 155.00, 'Consumer Goods - Household Products', 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    ('NKE', 'Nike Inc.', 100.00, 'Consumer Goods - Apparel', 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    ('JNJ', 'Johnson & Johnson', 160.00, 'Healthcare - Pharmaceuticals', 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    ('UNH', 'UnitedHealth Group Inc.', 500.00, 'Healthcare - Insurance', 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    ('XOM', 'Exxon Mobil Corporation', 110.00, 'Energy - Oil & Gas', 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    ('SPY', 'SPDR S&P 500 ETF Trust', 450.00, 'ETF - S&P 500 Index', 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    ('QQQ', 'Invesco QQQ Trust', 380.00, 'ETF - NASDAQ-100 Index', 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    ('IWM', 'iShares Russell 2000 ETF', 200.00, 'ETF - Small Cap Index', 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);
            ");

            // 3. Seed Demo Portfolio with $100,000 initial cash (will be adjusted after trades)
            migrationBuilder.Sql(@"
                INSERT INTO ""Portfolios"" (""Name"", ""Description"", ""Cash"", ""CreatedAt"")
                VALUES ('Demo Portfolio', 'Sample portfolio for demonstrating Monte Carlo option pricing with exotic options', 99286.50, CURRENT_TIMESTAMP);
            ");

            // 4. Seed 6 Options (one of each type, varying moneyness)
            // Stock IDs: AAPL=1, MSFT=2, GOOGL=3, META=4, NVDA=5, TSLA=6, AMZN=7
            // OptionType: European=0, Asian=1, Digital=2, Barrier=3, Lookback=4, Range=5
            migrationBuilder.Sql(@"
                INSERT INTO ""Options"" (""StockId"", ""OptionType"", ""Strike"", ""IsCall"", ""ExpiryDate"",
                    ""AveragingType"", ""ObservationFrequency"", ""DigitalCondition"",
                    ""BarrierOptionType"", ""BarrierDir"", ""BarrierLevel"",
                    ""LookbackOptionType"", ""RangeObservationFrequency"", ""CreatedAt"")
                VALUES
                    -- Option 1: AAPL European Call, ITM (Strike $220 < Spot ~$230)
                    (1, 0, 220.00, true, CURRENT_TIMESTAMP + INTERVAL '6 months',
                     NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP),

                    -- Option 2: MSFT Asian Put, ATM (Strike $430 = Spot ~$430), Arithmetic averaging
                    (2, 1, 430.00, false, CURRENT_TIMESTAMP + INTERVAL '6 months',
                     0, 1, NULL, NULL, NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP),

                    -- Option 3: NVDA Digital Call, OTM (Strike $150 > Spot ~$140), AboveStrike condition
                    (5, 2, 150.00, true, CURRENT_TIMESTAMP + INTERVAL '6 months',
                     NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP),

                    -- Option 4: TSLA Barrier Call, ITM (Strike $340 < Spot ~$350), Up-and-Out at $400
                    (6, 3, 340.00, true, CURRENT_TIMESTAMP + INTERVAL '6 months',
                     NULL, NULL, NULL, 1, 0, 400.00, NULL, NULL, CURRENT_TIMESTAMP),

                    -- Option 5: AMZN Lookback Put, ATM (Strike $210 = Spot ~$210), Min lookback
                    (7, 4, 210.00, false, CURRENT_TIMESTAMP + INTERVAL '6 months',
                     NULL, NULL, NULL, NULL, NULL, NULL, 1, NULL, CURRENT_TIMESTAMP),

                    -- Option 6: GOOGL Range Call, OTM (Strike $185 > Spot ~$175)
                    (3, 5, 185.00, true, CURRENT_TIMESTAMP + INTERVAL '6 months',
                     NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, CURRENT_TIMESTAMP);
            ");

            // 5. Seed 6 Trades (one buy for each option)
            // Portfolio ID = 1, Option IDs = 1-6
            // AssetType: Option=1, TradeType: Buy=0
            migrationBuilder.Sql(@"
                INSERT INTO ""Trades"" (""PortfolioId"", ""AssetType"", ""StockId"", ""OptionId"", ""TradeType"",
                    ""Quantity"", ""Price"", ""TotalCost"", ""Notes"", ""TradeDate"", ""CreatedAt"")
                VALUES
                    -- Trade 1: Buy 10 AAPL European Calls @ $18.00
                    (1, 1, NULL, 1, 0, 10, 18.00, 180.00, 'Opening ITM European call position', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),

                    -- Trade 2: Buy 5 MSFT Asian Puts @ $12.50
                    (1, 1, NULL, 2, 0, 5, 12.50, 62.50, 'Opening ATM Asian put position', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),

                    -- Trade 3: Buy 15 NVDA Digital Calls @ $4.00
                    (1, 1, NULL, 3, 0, 15, 4.00, 60.00, 'Opening OTM Digital call position', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),

                    -- Trade 4: Buy 8 TSLA Barrier Calls @ $22.00
                    (1, 1, NULL, 4, 0, 8, 22.00, 176.00, 'Opening ITM Barrier call position', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),

                    -- Trade 5: Buy 6 AMZN Lookback Puts @ $25.00
                    (1, 1, NULL, 5, 0, 6, 25.00, 150.00, 'Opening ATM Lookback put position', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),

                    -- Trade 6: Buy 10 GOOGL Range Calls @ $8.50
                    (1, 1, NULL, 6, 0, 10, 8.50, 85.00, 'Opening OTM Range call position', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);
            ");

            // 6. Seed 6 Positions (matching the trades)
            // AssetType: Option=1
            migrationBuilder.Sql(@"
                INSERT INTO ""Positions"" (""PortfolioId"", ""AssetType"", ""StockId"", ""OptionId"",
                    ""NetQuantity"", ""AverageCost"", ""TotalCost"", ""LastUpdated"", ""CreatedAt"")
                VALUES
                    -- Position 1: 10 AAPL European Calls
                    (1, 1, NULL, 1, 10, 18.00, 180.00, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),

                    -- Position 2: 5 MSFT Asian Puts
                    (1, 1, NULL, 2, 5, 12.50, 62.50, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),

                    -- Position 3: 15 NVDA Digital Calls
                    (1, 1, NULL, 3, 15, 4.00, 60.00, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),

                    -- Position 4: 8 TSLA Barrier Calls
                    (1, 1, NULL, 4, 8, 22.00, 176.00, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),

                    -- Position 5: 6 AMZN Lookback Puts
                    (1, 1, NULL, 5, 6, 25.00, 150.00, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),

                    -- Position 6: 10 GOOGL Range Calls
                    (1, 1, NULL, 6, 10, 8.50, 85.00, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Positions");

            migrationBuilder.DropTable(
                name: "PricingHistory");

            migrationBuilder.DropTable(
                name: "Trades");

            migrationBuilder.DropTable(
                name: "Options");

            migrationBuilder.DropTable(
                name: "Portfolios");

            migrationBuilder.DropTable(
                name: "Stocks");

            migrationBuilder.DropTable(
                name: "Exchanges");
        }
    }
}
