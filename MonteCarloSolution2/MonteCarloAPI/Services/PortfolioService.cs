using Microsoft.EntityFrameworkCore;
using MonteCarloAPI.Data;
using MonteCarloAPI.Models;

namespace MonteCarloAPI.Services
{
    /// <summary>
    /// Service for managing portfolios, trades, and positions.
    /// Handles business logic for portfolio operations and keeps Position table synced with Trades.
    /// </summary>
    public class PortfolioService
    {
        private readonly MonteCarloDbContext _context;
        private readonly OptionService _optionService;
        private readonly PricingService _pricingService;

        public PortfolioService(MonteCarloDbContext context, OptionService optionService, PricingService pricingService)
        {
            _context = context;
            _optionService = optionService;
            _pricingService = pricingService;
        }

        #region Portfolio CRUD Operations

        /// <summary>
        /// Create a new portfolio
        /// </summary>
        public async Task<PortfolioDTO> CreatePortfolioAsync(CreatePortfolioDTO createDto)
        {
            var entity = new PortfolioEntity
            {
                Name = createDto.Name,
                Description = createDto.Description,
                Cash = createDto.InitialCash,
                CreatedAt = DateTime.UtcNow
            };

            _context.Portfolios.Add(entity);
            await _context.SaveChangesAsync();

            return PortfolioDTO.FromEntity(entity);
        }

        /// <summary>
        /// Get portfolio by ID
        /// </summary>
        public async Task<PortfolioDTO?> GetPortfolioByIdAsync(int id)
        {
            var entity = await _context.Portfolios.FindAsync(id);
            return entity == null ? null : PortfolioDTO.FromEntity(entity);
        }

        /// <summary>
        /// Get all portfolios
        /// </summary>
        public async Task<List<PortfolioDTO>> GetAllPortfoliosAsync()
        {
            var entities = await _context.Portfolios
                .OrderBy(p => p.Id)
                .ToListAsync();

            return entities.Select(PortfolioDTO.FromEntity).ToList();
        }

        /// <summary>
        /// Get portfolio summary with counts
        /// </summary>
        public async Task<PortfolioSummaryDTO?> GetPortfolioSummaryAsync(int id)
        {
            var portfolio = await _context.Portfolios.FindAsync(id);
            if (portfolio == null) return null;

            var positionCount = await _context.Positions
                .Where(p => p.PortfolioId == id)
                .CountAsync();

            var tradeCount = await _context.Trades
                .Where(t => t.PortfolioId == id)
                .CountAsync();

            var totalInvested = await _context.Positions
                .Where(p => p.PortfolioId == id)
                .SumAsync(p => p.TotalCost);

            return new PortfolioSummaryDTO
            {
                Id = portfolio.Id,
                Name = portfolio.Name,
                Description = portfolio.Description,
                Cash = portfolio.Cash,
                CreatedAt = portfolio.CreatedAt,
                UpdatedAt = portfolio.UpdatedAt,
                PositionCount = positionCount,
                TradeCount = tradeCount,
                TotalInvested = totalInvested
            };
        }

        /// <summary>
        /// Update portfolio information
        /// </summary>
        public async Task<PortfolioDTO?> UpdatePortfolioAsync(int id, UpdatePortfolioDTO updateDto)
        {
            var entity = await _context.Portfolios.FindAsync(id);
            if (entity == null) return null;

            if (updateDto.Name != null)
                entity.Name = updateDto.Name;

            if (updateDto.Description != null)
                entity.Description = updateDto.Description;

            if (updateDto.Cash.HasValue)
                entity.Cash = updateDto.Cash.Value;

            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return PortfolioDTO.FromEntity(entity);
        }

        /// <summary>
        /// Delete portfolio (and all its trades/positions via CASCADE)
        /// </summary>
        public async Task<bool> DeletePortfolioAsync(int id)
        {
            var entity = await _context.Portfolios.FindAsync(id);
            if (entity == null) return false;

            _context.Portfolios.Remove(entity);
            await _context.SaveChangesAsync();

            return true;
        }

        #endregion

        #region Position Operations

        /// <summary>
        /// Delete a position by ID
        /// </summary>
        public async Task<bool> DeletePositionAsync(int positionId)
        {
            var position = await _context.Positions.FindAsync(positionId);
            if (position == null) return false;

            _context.Positions.Remove(position);
            await _context.SaveChangesAsync();

            return true;
        }

        #endregion

        #region Trade Operations

        /// <summary>
        /// Record a new trade and update the corresponding position
        /// </summary>
        public async Task<TradeDTO> RecordTradeAsync(int portfolioId, CreateTradeDTO createDto)
        {
            // Validate portfolio exists
            var portfolio = await _context.Portfolios.FindAsync(portfolioId);
            if (portfolio == null)
                throw new ArgumentException($"Portfolio {portfolioId} not found");

            // Validate based on asset type
            if (createDto.AssetType == AssetType.Stock)
            {
                if (!createDto.StockId.HasValue)
                    throw new ArgumentException("StockId is required for stock trades");
                var stock = await _context.Stocks.FindAsync(createDto.StockId.Value);
                if (stock == null)
                    throw new ArgumentException($"Stock {createDto.StockId} not found");
            }
            else // Option
            {
                if (!createDto.OptionId.HasValue)
                    throw new ArgumentException("OptionId is required for option trades");
                var option = await _context.Options.FindAsync(createDto.OptionId.Value);
                if (option == null)
                    throw new ArgumentException($"Option {createDto.OptionId} not found");
            }

            // Calculate total cost (with sign based on trade type)
            double totalCost = CalculateTradeCost(createDto.TradeType, createDto.Quantity, createDto.Price);

            // Create trade entity
            var trade = new TradeEntity
            {
                PortfolioId = portfolioId,
                AssetType = createDto.AssetType,
                StockId = createDto.StockId,
                OptionId = createDto.OptionId,
                TradeType = createDto.TradeType,
                Quantity = createDto.Quantity,
                Price = createDto.Price,
                TotalCost = totalCost,
                Notes = createDto.Notes,
                TradeDate = createDto.TradeDate ?? DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _context.Trades.Add(trade);

            // Update portfolio cash
            portfolio.Cash -= totalCost;
            portfolio.UpdatedAt = DateTime.UtcNow;

            // Update or create position
            await UpdatePositionForTradeAsync(portfolioId, createDto.AssetType, createDto.StockId, createDto.OptionId, trade);

            await _context.SaveChangesAsync();

            return TradeDTO.FromEntity(trade);
        }

        /// <summary>
        /// Get all trades for a portfolio
        /// </summary>
        public async Task<List<TradeDTO>> GetPortfolioTradesAsync(int portfolioId)
        {
            var trades = await _context.Trades
                .Where(t => t.PortfolioId == portfolioId)
                .OrderByDescending(t => t.TradeDate)
                .ToListAsync();

            return trades.Select(TradeDTO.FromEntity).ToList();
        }

        /// <summary>
        /// Get a specific trade by ID
        /// </summary>
        public async Task<TradeDTO?> GetTradeByIdAsync(int tradeId)
        {
            var trade = await _context.Trades.FindAsync(tradeId);
            return trade == null ? null : TradeDTO.FromEntity(trade);
        }

        /// <summary>
        /// Delete a trade by ID
        /// </summary>
        public async Task<bool> DeleteTradeAsync(int tradeId)
        {
            var trade = await _context.Trades.FindAsync(tradeId);
            if (trade == null) return false;

            _context.Trades.Remove(trade);
            await _context.SaveChangesAsync();

            return true;
        }

        #endregion

        #region Position Operations

        /// <summary>
        /// Get all positions for a portfolio (both stock and option positions)
        /// </summary>
        public async Task<List<PositionDTO>> GetPortfolioPositionsAsync(int portfolioId)
        {
            var positions = await _context.Positions
                .Include(p => p.Stock)
                .ThenInclude(s => s!.Exchange)
                .Include(p => p.Option)
                .ThenInclude(o => o!.Stock)
                .Where(p => p.PortfolioId == portfolioId)
                .Where(p => p.NetQuantity != 0) // Only return open positions
                .OrderBy(p => p.AssetType)
                .ThenBy(p => p.StockId)
                .ThenBy(p => p.OptionId)
                .ToListAsync();

            return positions.Select(p => {
                var dto = PositionDTO.FromEntity(p);
                if (p.AssetType == AssetType.Stock && p.Stock != null)
                {
                    dto.Stock = StockDTO.FromEntity(p.Stock);
                }
                else if (p.AssetType == AssetType.Option && p.Option != null)
                {
                    dto.Option = OptionService.MapToDTO(p.Option);
                }
                return dto;
            }).ToList();
        }

        /// <summary>
        /// Get a specific position
        /// </summary>
        public async Task<PositionDTO?> GetPositionAsync(int portfolioId, int optionId)
        {
            var position = await _context.Positions
                .FirstOrDefaultAsync(p => p.PortfolioId == portfolioId && p.OptionId == optionId);

            return position == null ? null : PositionDTO.FromEntity(position);
        }

        #endregion

        #region Portfolio Valuation

        /// <summary>
        /// Value entire portfolio using current market parameters.
        /// Stock positions use market price, option positions use Monte Carlo simulation.
        /// </summary>
        public async Task<PortfolioValuationDTO> ValuePortfolioAsync(int portfolioId, PortfolioValuationRequestDTO request)
        {
            var portfolio = await _context.Portfolios.FindAsync(portfolioId);
            if (portfolio == null)
                throw new ArgumentException($"Portfolio {portfolioId} not found");

            var positions = await _context.Positions
                .Include(p => p.Stock)
                .ThenInclude(s => s!.Exchange)
                .Include(p => p.Option)
                .ThenInclude(o => o!.Stock)
                .Where(p => p.PortfolioId == portfolioId)
                .Where(p => p.NetQuantity != 0)
                .ToListAsync();

            // Separate stock and option positions
            var stockPositions = positions.Where(p => p.AssetType == AssetType.Stock).ToList();
            var optionPositions = positions.Where(p => p.AssetType == AssetType.Option).ToList();

            var valuedPositions = new List<PositionWithValuationDTO>();
            double stockPositionValue = 0;
            double optionPositionValue = 0;

            // Value stock positions (simple market price calculation)
            foreach (var position in stockPositions)
            {
                double currentPrice = position.Stock!.CurrentPrice;
                double currentValue = position.NetQuantity * currentPrice;
                double unrealizedPnL = currentValue - position.TotalCost;
                double pnlPercentage = position.TotalCost != 0 ? (unrealizedPnL / Math.Abs(position.TotalCost)) * 100 : 0;

                valuedPositions.Add(new PositionWithValuationDTO
                {
                    Id = position.Id,
                    PortfolioId = position.PortfolioId,
                    AssetType = AssetType.Stock,
                    StockId = position.StockId,
                    OptionId = null,
                    NetQuantity = position.NetQuantity,
                    AverageCost = position.AverageCost,
                    TotalCost = position.TotalCost,
                    LastUpdated = position.LastUpdated,
                    Stock = StockDTO.FromEntity(position.Stock),
                    Option = null,
                    CurrentPrice = currentPrice,
                    CurrentValue = currentValue,
                    UnrealizedPnL = unrealizedPnL,
                    PnLPercentage = pnlPercentage,
                    // Stocks don't have Greeks (could set Delta = NetQuantity, but leaving null for clarity)
                    Delta = null,
                    Gamma = null,
                    Vega = null,
                    Theta = null,
                    Rho = null
                });

                stockPositionValue += currentValue;
            }

            // Price option positions in parallel using Monte Carlo
            var pricingTasks = optionPositions.Select(async position =>
            {
                // Create simulation parameters for this specific option using its underlying stock's current price
                // NOTE: RiskFreeRate and TimeToExpiry will be calculated by PricingService based on option's ExpiryDate
                // NOTE: We disable multithreading per-option since we're already parallelizing at the portfolio level
                var simParams = new SimulationParametersDTO
                {
                    InitialPrice = position.Option!.Stock!.CurrentPrice,
                    Volatility = request.Volatility,
                    TimeSteps = request.TimeSteps,
                    NumberOfPaths = request.NumberOfPaths,
                    UseMultithreading = false,  // Disabled to prevent thread explosion
                    SimMode = (SimulationMode)request.SimMode
                };

                // Price the option (this calculates RiskFreeRate and TimeToExpiry based on ExpiryDate)
                var optionDto = OptionService.MapToDTO(position.Option);
                var pricingResult = await _pricingService.PriceOptionAsync(optionDto, simParams);

                // Calculate position value and P&L
                double currentPrice = pricingResult.Price;
                double currentValue = position.NetQuantity * currentPrice;
                double unrealizedPnL = currentValue - position.TotalCost;
                double pnlPercentage = position.TotalCost != 0 ? (unrealizedPnL / Math.Abs(position.TotalCost)) * 100 : 0;

                // Scale Greeks by quantity and return valued position with simParams
                return (
                    Position: new PositionWithValuationDTO
                    {
                        Id = position.Id,
                        PortfolioId = position.PortfolioId,
                        AssetType = AssetType.Option,
                        StockId = null,
                        OptionId = position.OptionId,
                        NetQuantity = position.NetQuantity,
                        AverageCost = position.AverageCost,
                        TotalCost = position.TotalCost,
                        LastUpdated = position.LastUpdated,
                        Stock = null,
                        Option = optionDto,
                        CurrentPrice = currentPrice,
                        CurrentValue = currentValue,
                        UnrealizedPnL = unrealizedPnL,
                        PnLPercentage = pnlPercentage,
                        Delta = pricingResult.Delta * position.NetQuantity,
                        Gamma = pricingResult.Gamma * position.NetQuantity,
                        Vega = pricingResult.Vega * position.NetQuantity,
                        Theta = pricingResult.Theta * position.NetQuantity,
                        Rho = pricingResult.Rho * position.NetQuantity
                    },
                    SimParams: simParams  // Contains calculated RiskFreeRate and TimeToExpiry
                );
            }).ToList();

            // Wait for all pricing tasks to complete
            var pricingResults = (await Task.WhenAll(pricingTasks)).ToList();
            valuedPositions.AddRange(pricingResults.Select(r => r.Position));

            // Save option pricing to history (stocks don't have pricing history)
            foreach (var result in pricingResults)
            {
                var valuedPosition = result.Position;
                var simParams = result.SimParams;
                var historyEntry = new PricingHistoryEntity
                {
                    OptionId = valuedPosition.OptionId!.Value,
                    InitialPrice = simParams.InitialPrice,
                    Volatility = simParams.Volatility,
                    RiskFreeRate = simParams.RiskFreeRate,
                    TimeToExpiry = simParams.TimeToExpiry,
                    TimeSteps = simParams.TimeSteps,
                    NumberOfPaths = simParams.NumberOfPaths,
                    UseMultithreading = false,
                    SimMode = (int)simParams.SimMode,
                    Price = valuedPosition.CurrentPrice,
                    StandardError = 0.0,
                    ExecutionTimeMs = 0.0,
                    Delta = valuedPosition.Delta / valuedPosition.NetQuantity,
                    Gamma = valuedPosition.Gamma / valuedPosition.NetQuantity,
                    Vega = valuedPosition.Vega / valuedPosition.NetQuantity,
                    Theta = valuedPosition.Theta / valuedPosition.NetQuantity,
                    Rho = valuedPosition.Rho / valuedPosition.NetQuantity,
                    RequestSource = $"Portfolio-{portfolioId}",
                    PricedAt = DateTime.UtcNow
                };
                _context.PricingHistory.Add(historyEntry);
                optionPositionValue += valuedPosition.CurrentValue;
            }
            await _context.SaveChangesAsync();

            // Accumulate portfolio totals
            double totalPositionValue = stockPositionValue + optionPositionValue;
            double totalCost = valuedPositions.Sum(p => p.TotalCost);

            // Sum Greeks (only from option positions)
            double? portfolioDelta = 0, portfolioGamma = 0, portfolioVega = 0, portfolioTheta = 0, portfolioRho = 0;
            foreach (var valuedPosition in valuedPositions.Where(p => p.AssetType == AssetType.Option))
            {
                portfolioDelta += valuedPosition.Delta;
                portfolioGamma += valuedPosition.Gamma;
                portfolioVega += valuedPosition.Vega;
                portfolioTheta += valuedPosition.Theta;
                portfolioRho += valuedPosition.Rho;
            }

            double totalValue = totalPositionValue + portfolio.Cash;
            double totalUnrealizedPnL = totalPositionValue - totalCost;
            double totalPnLPercentage = totalCost != 0 ? (totalUnrealizedPnL / Math.Abs(totalCost)) * 100 : 0;

            // Create a summary market parameters object
            var summaryMarketParams = new SimulationParametersDTO
            {
                InitialPrice = 0,      // Per-option (each uses its own stock price)
                Volatility = request.Volatility,
                RiskFreeRate = 0,      // Per-option (calculated from rate curve based on expiry)
                TimeToExpiry = 0,      // Per-option (calculated from option's ExpiryDate)
                TimeSteps = request.TimeSteps,
                NumberOfPaths = request.NumberOfPaths,
                UseMultithreading = request.UseMultithreading,
                SimMode = (SimulationMode)request.SimMode
            };

            return new PortfolioValuationDTO
            {
                PortfolioId = portfolio.Id,
                PortfolioName = portfolio.Name,
                Cash = portfolio.Cash,
                Positions = valuedPositions,
                StockPositionValue = stockPositionValue,
                OptionPositionValue = optionPositionValue,
                TotalPositionValue = totalPositionValue,
                TotalValue = totalValue,
                TotalCost = totalCost,
                TotalUnrealizedPnL = totalUnrealizedPnL,
                TotalPnLPercentage = totalPnLPercentage,
                PortfolioDelta = portfolioDelta,
                PortfolioGamma = portfolioGamma,
                PortfolioVega = portfolioVega,
                PortfolioTheta = portfolioTheta,
                PortfolioRho = portfolioRho,
                MarketParameters = summaryMarketParams
            };
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Calculate trade cost with proper sign
        /// Buy = negative (cash out), Sell = positive (cash in)
        /// </summary>
        private double CalculateTradeCost(TradeType tradeType, int quantity, double price)
        {
            double baseCost = quantity * price;

            return tradeType switch
            {
                TradeType.Buy => baseCost,  // Pay for contracts
                TradeType.Sell => -baseCost, // Receive cash
                TradeType.Close => quantity > 0 ? -(Math.Abs(quantity) * price) : (Math.Abs(quantity) * price),
                _ => throw new ArgumentException($"Unknown trade type: {tradeType}")
            };
        }

        /// <summary>
        /// Update or create position based on trade
        /// </summary>
        private async Task UpdatePositionForTradeAsync(int portfolioId, AssetType assetType, int? stockId, int? optionId, TradeEntity trade)
        {
            // Find existing position based on asset type
            PositionEntity? position;
            if (assetType == AssetType.Stock)
            {
                position = await _context.Positions
                    .FirstOrDefaultAsync(p => p.PortfolioId == portfolioId && p.StockId == stockId);
            }
            else
            {
                position = await _context.Positions
                    .FirstOrDefaultAsync(p => p.PortfolioId == portfolioId && p.OptionId == optionId);
            }

            if (position == null)
            {
                // Create new position
                position = new PositionEntity
                {
                    PortfolioId = portfolioId,
                    AssetType = assetType,
                    StockId = stockId,
                    OptionId = optionId,
                    NetQuantity = trade.Quantity,
                    AverageCost = trade.Price,
                    TotalCost = trade.TotalCost,
                    CreatedAt = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow
                };
                _context.Positions.Add(position);
            }
            else
            {
                // Update existing position
                int newQuantity = position.NetQuantity + trade.Quantity;

                if (newQuantity == 0)
                {
                    // Position closed - could delete or set to zero
                    position.NetQuantity = 0;
                    position.AverageCost = 0;
                    position.TotalCost = 0;
                }
                else
                {
                    // Recalculate average cost and total cost
                    double newTotalCost = position.TotalCost + trade.TotalCost;
                    position.NetQuantity = newQuantity;
                    position.TotalCost = newTotalCost;
                    position.AverageCost = newTotalCost / newQuantity;
                }

                position.LastUpdated = DateTime.UtcNow;
            }
        }

        #endregion
    }
}
