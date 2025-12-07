# DeltaDesk

A Monte Carlo option pricing and portfolio management system built with .NET 9.0, featuring real-time stock prices, exotic option pricing with Greeks, and comprehensive trade tracking.

## Features

- **Multi-Exchange Support** - Track stocks across NYSE, NASDAQ, and other exchanges
- **Real-Time Stock Prices** - Automatic price updates via Alpaca Markets API
- **Exotic Option Pricing** - Price 6 option types using Monte Carlo simulation:
  - European
  - Asian (Arithmetic/Geometric averaging)
  - Digital (Binary payoff)
  - Barrier (Knock-in/Knock-out)
  - Lookback (Floating/Fixed strike)
  - Range
- **Greeks Calculation** - Delta, Gamma, Vega, Theta, Rho for all options
- **Portfolio Management** - Track multiple portfolios with cash and positions
- **Trade Recording** - Buy/sell both stocks and options with automatic position updates
- **Variance Reduction** - Multiple simulation modes including Antithetic Variates, Control Variates, and Quasi-Random sequences
- **Web GUI** - Clean, responsive interface for all operations

## Tech Stack

- **Backend**: .NET 9.0, ASP.NET Core Web API
- **Database**: PostgreSQL 16 with Entity Framework Core
- **Market Data**: Alpaca Markets API
- **Frontend**: Vanilla JavaScript, HTML5, CSS3
- **Pricing Engine**: Custom Monte Carlo simulation library

## Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [PostgreSQL 16](https://www.postgresql.org/download/)
- [Alpaca Markets Account](https://alpaca.markets/) (free tier works)

## Setup

### 1. Clone the Repository

```bash
git clone <repository-url>
cd MonteCarloSolution2
```

### 2. Configure Database

Create a PostgreSQL database:

```sql
CREATE DATABASE montecarlo_dev;
```

Then update the connection string in `MonteCarloAPI/appsettings.Development.json` with your PostgreSQL username (and password if you have one):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=montecarlo_dev;Username=your_username;Password=your_password"
  }
}
```

> **Note:** Alpaca API keys are already configured in `appsettings.json` for paper trading.

### 3. Run Database Migrations

This will create all tables and seed demo data (20 stocks, 1 portfolio, 6 sample options with trades):

```bash
cd MonteCarloAPI
dotnet ef database update
```

### 4. Run the Application

```bash
dotnet run
```

The application will be available at `http://localhost:5262`

## Project Structure

```
MonteCarloSolution2/
├── MonteCarloAPI/           # ASP.NET Core Web API
│   ├── Controllers/         # API endpoints
│   ├── Services/            # Business logic
│   ├── Data/                # Entity Framework entities & DbContext
│   ├── Models/              # DTOs
│   ├── Configuration/       # Rate curves, settings
│   ├── Migrations/          # EF Core migrations
│   └── wwwroot/             # Static files (GUI)
│       ├── index.html
│       ├── css/
│       └── js/
├── MonteCarloOptionPricer/  # Monte Carlo pricing library
│   ├── Options/             # Option type implementations
│   ├── Simulation/          # Path generation & variance reduction
│   └── Greeks/              # Greeks calculation
└── MonteCarlo2.0/           # Console application (legacy)
```

## API Endpoints

### Stocks
- `GET /api/stock` - List all stocks
- `GET /api/stock/{id}` - Get stock by ID
- `POST /api/stock` - Create stock

### Options
- `GET /api/options` - List all options
- `GET /api/options/{id}` - Get option by ID
- `POST /api/options` - Create option
- `DELETE /api/options/{id}` - Delete option

### Pricing
- `POST /api/pricing/{optionId}` - Price an option with custom parameters
- `GET /api/pricing/history/{optionId}` - Get pricing history

### Portfolios
- `GET /api/portfolio` - List portfolios
- `GET /api/portfolio/{id}` - Get portfolio details
- `POST /api/portfolio` - Create portfolio
- `GET /api/portfolio/{id}/valuation` - Get portfolio valuation with P&L

### Trades
- `GET /api/portfolio/{id}/trades` - List trades
- `POST /api/portfolio/{id}/trades` - Record a trade
- `DELETE /api/portfolio/{portfolioId}/trades/{tradeId}` - Delete trade

## Simulation Modes

| Mode | Description |
|------|-------------|
| 0 | Plain Monte Carlo |
| 1 | Antithetic Variates |
| 2 | Control Variates |
| 3 | Antithetic + Control Variates |
| 4 | Quasi-Random (Van Der Corput) |

## Data Model

See `MonteCarloAPI/Data/DATA_MODEL_GUIDE.html` for complete database schema documentation.

## GUI Usage

1. **Create a Portfolio** - Click "+ New Portfolio" on the home dashboard
2. **View Stocks** - Navigate to "View Stocks" to see available underlyings
3. **Create Options** - Go to "Create Options" to define exotic options
4. **Record Trades** - Use "Input Trades" to buy/sell stocks or options
5. **Monitor Positions** - Check "View Positions" to see current holdings
6. **Price Portfolio** - Select a portfolio and click "Update P&L" to run Monte Carlo valuation

## License

MIT License
