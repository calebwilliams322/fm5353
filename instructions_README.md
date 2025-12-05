# DeltaDesk - Monte Carlo Option Pricing Application

A full-stack application for pricing exotic options using Monte Carlo simulation, with real-time stock prices from Alpaca Markets.

## Prerequisites

Before running the application, ensure you have:

1. **.NET 9 SDK**
   - Download from: https://dotnet.microsoft.com/download/dotnet/9.0
   - Verify installation: `dotnet --version` (should show 9.x)

2. **PostgreSQL** (running locally)
   - Download from: https://www.postgresql.org/download/
   - Default configuration expected:
     - Host: `localhost`
     - Port: `5432`
     - Username: `postgres`
     - Password: `postgres`

3. **Entity Framework Core Tools** (for migrations)
   ```bash
   dotnet tool install --global dotnet-ef
   ```

## Quick Start

```bash
# 1. Navigate to the API project
cd MonteCarloSolution2/MonteCarloAPI

# 2. Create the database and apply migrations
dotnet ef database update

# 3. Run the application
dotnet run
```

## Access the Application

Once running, open your browser to:

| Resource | URL |
|----------|-----|
| **Frontend (DeltaDesk)** | http://localhost:5262 |
| **Swagger API Docs** | http://localhost:5262/swagger |

## Pre-Populated Demo Data

The application comes with seed data so you can explore immediately:

- **1 Exchange:** NYSE
- **20 Stocks:** Major tickers (AAPL, MSFT, GOOGL, TSLA, NVDA, AMZN, etc.)
- **1 Demo Portfolio:** "Demo Portfolio" with ~$99,287 cash
- **6 Exotic Options:** One of each type with varying moneyness:

| Option Type | Underlying | Call/Put | Strike | Moneyness |
|-------------|------------|----------|--------|-----------|
| European | AAPL | Call | $220 | ITM |
| Asian | MSFT | Put | $430 | ATM |
| Digital | NVDA | Call | $150 | OTM |
| Barrier | TSLA | Call | $340 | ITM |
| Lookback | AMZN | Put | $210 | ATM |
| Range | GOOGL | Call | $185 | OTM |

- **6 Trades:** One buy order for each option
- **6 Open Positions:** Ready to be priced with Monte Carlo simulation

## Features to Explore

1. **Home Dashboard** - View portfolio P&L, cash, positions, and Greeks
2. **View Stocks** - See all 20 stocks with live prices (updated every 5 minutes via Alpaca)
3. **Create Options** - Design new exotic options (European, Asian, Digital, Barrier, Lookback, Range)
4. **Input Trades** - Price options and record buy/sell trades
5. **View Positions** - See current holdings and price all positions
6. **View Portfolio** - Portfolio management and valuation

## Configuration (Optional)

If your PostgreSQL setup differs from the defaults, update the connection string in `MonteCarloSolution2/MonteCarloAPI/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=montecarlo;Username=YOUR_USERNAME;Password=YOUR_PASSWORD"
  }
}
```

## Troubleshooting

**Database connection error:**
- Ensure PostgreSQL is running: `pg_isready -h localhost -p 5432`
- Verify credentials match your PostgreSQL setup

**Port already in use:**
- The app runs on port 5262 by default
- Check if another process is using it: `lsof -i :5262`

**EF Core tools not found:**
- Install globally: `dotnet tool install --global dotnet-ef`
- Or restore local tools: `dotnet tool restore`

## Project Structure

```
fm5353/
├── MonteCarloSolution2/
│   ├── MonteCarloAPI/          # ASP.NET Core Web API + Frontend
│   │   ├── Controllers/        # API endpoints
│   │   ├── Data/               # Entity Framework entities & context
│   │   ├── Migrations/         # Database migrations
│   │   ├── Models/             # DTOs and view models
│   │   ├── Services/           # Business logic (pricing, Alpaca, etc.)
│   │   └── wwwroot/            # Frontend (HTML, CSS, JS)
│   ├── MonteCarloOptionPricer/ # Core Monte Carlo simulation library
│   └── MonteCarlo2.0/          # Console application for testing
└── Archive/                    # Previous coursework
```
