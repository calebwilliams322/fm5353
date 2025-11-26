using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MonteCarloAPI.Migrations
{
    /// <inheritdoc />
    public partial class SeedCommonStocks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var now = DateTime.UtcNow;

            // Seed 20 common stocks - prices are placeholders and will be updated by AlpacaService
            migrationBuilder.InsertData(
                table: "Stocks",
                columns: new[] { "Ticker", "Name", "CurrentPrice", "Description", "CreatedAt", "UpdatedAt" },
                values: new object[,]
                {
                    // Technology
                    { "AAPL", "Apple Inc.", 150.00m, "Technology - Consumer Electronics", now, now },
                    { "MSFT", "Microsoft Corporation", 330.00m, "Technology - Software", now, now },
                    { "GOOGL", "Alphabet Inc.", 130.00m, "Technology - Internet Services", now, now },
                    { "META", "Meta Platforms Inc.", 300.00m, "Technology - Social Media", now, now },
                    { "NVDA", "NVIDIA Corporation", 450.00m, "Technology - Semiconductors", now, now },
                    { "TSLA", "Tesla Inc.", 240.00m, "Automotive - Electric Vehicles", now, now },
                    { "AMZN", "Amazon.com Inc.", 140.00m, "Technology - E-commerce", now, now },

                    // Finance
                    { "JPM", "JPMorgan Chase & Co.", 150.00m, "Financial Services - Banking", now, now },
                    { "GS", "Goldman Sachs Group Inc.", 380.00m, "Financial Services - Investment Banking", now, now },
                    { "BAC", "Bank of America Corporation", 35.00m, "Financial Services - Banking", now, now },

                    // Consumer
                    { "WMT", "Walmart Inc.", 160.00m, "Retail - Discount Stores", now, now },
                    { "KO", "The Coca-Cola Company", 60.00m, "Consumer Goods - Beverages", now, now },
                    { "PG", "Procter & Gamble Co.", 155.00m, "Consumer Goods - Household Products", now, now },
                    { "NKE", "Nike Inc.", 100.00m, "Consumer Goods - Apparel", now, now },

                    // Healthcare
                    { "JNJ", "Johnson & Johnson", 160.00m, "Healthcare - Pharmaceuticals", now, now },
                    { "UNH", "UnitedHealth Group Inc.", 500.00m, "Healthcare - Insurance", now, now },

                    // Energy
                    { "XOM", "Exxon Mobil Corporation", 110.00m, "Energy - Oil & Gas", now, now },

                    // ETFs
                    { "SPY", "SPDR S&P 500 ETF Trust", 450.00m, "ETF - S&P 500 Index", now, now },
                    { "QQQ", "Invesco QQQ Trust", 380.00m, "ETF - NASDAQ-100 Index", now, now },
                    { "IWM", "iShares Russell 2000 ETF", 200.00m, "ETF - Small Cap Index", now, now }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove the seeded stocks
            migrationBuilder.DeleteData(
                table: "Stocks",
                keyColumn: "Ticker",
                keyValues: new object[]
                {
                    "AAPL", "MSFT", "GOOGL", "META", "NVDA", "TSLA", "AMZN",
                    "JPM", "GS", "BAC",
                    "WMT", "KO", "PG", "NKE",
                    "JNJ", "UNH",
                    "XOM",
                    "SPY", "QQQ", "IWM"
                });
        }
    }
}
