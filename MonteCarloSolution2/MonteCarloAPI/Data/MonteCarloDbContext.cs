using Microsoft.EntityFrameworkCore;

namespace MonteCarloAPI.Data
{
    /// <summary>
    /// Database context for Monte Carlo option pricing system.
    /// Manages connection to PostgreSQL and provides access to Options table.
    /// </summary>
    public class MonteCarloDbContext : DbContext
    {
        public MonteCarloDbContext(DbContextOptions<MonteCarloDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Options table - stores all option configurations
        /// </summary>
        public DbSet<OptionEntity> Options { get; set; } = null!;

        /// <summary>
        /// PricingHistory table - stores historical pricing results for options
        /// </summary>
        public DbSet<PricingHistoryEntity> PricingHistory { get; set; } = null!;

        /// <summary>
        /// Portfolios table - stores portfolio information
        /// </summary>
        public DbSet<PortfolioEntity> Portfolios { get; set; } = null!;

        /// <summary>
        /// Trades table - stores all trade transactions
        /// </summary>
        public DbSet<TradeEntity> Trades { get; set; } = null!;

        /// <summary>
        /// Positions table - stores current positions (holdings)
        /// </summary>
        public DbSet<PositionEntity> Positions { get; set; } = null!;

        /// <summary>
        /// Stocks table - stores underlying stocks/assets
        /// </summary>
        public DbSet<StockEntity> Stocks { get; set; } = null!;

        /// <summary>
        /// Exchanges table - stores stock exchanges
        /// </summary>
        public DbSet<ExchangeEntity> Exchanges { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the Options table
            modelBuilder.Entity<OptionEntity>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.UpdatedAt)
                    .IsRequired(false);

                // Create index on CreatedAt for efficient querying
                entity.HasIndex(e => e.CreatedAt);

                // Store enums as integers in the database
                entity.Property(e => e.OptionType)
                    .HasConversion<int>()
                    .IsRequired();

                // Nullable enum conversions for type-specific fields
                entity.Property(e => e.AveragingType)
                    .HasConversion<int?>()
                    .IsRequired(false);

                entity.Property(e => e.DigitalCondition)
                    .HasConversion<int?>()
                    .IsRequired(false);

                entity.Property(e => e.BarrierOptionType)
                    .HasConversion<int?>()
                    .IsRequired(false);

                entity.Property(e => e.BarrierDir)
                    .HasConversion<int?>()
                    .IsRequired(false);

                entity.Property(e => e.LookbackOptionType)
                    .HasConversion<int?>()
                    .IsRequired(false);

                // Foreign key relationship to Stock
                entity.HasOne(e => e.Stock)
                    .WithMany(s => s.Options)
                    .HasForeignKey(e => e.StockId)
                    .OnDelete(DeleteBehavior.Restrict); // Don't delete stock if options exist

                // Create index on StockId for efficient querying
                entity.HasIndex(e => e.StockId);
            });

            // Configure the PricingHistory table
            modelBuilder.Entity<PricingHistoryEntity>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.PricedAt)
                    .IsRequired()
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Foreign key relationship to Options table
                entity.HasOne(e => e.Option)
                    .WithMany()
                    .HasForeignKey(e => e.OptionId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Create indexes for efficient querying
                entity.HasIndex(e => e.OptionId);
                entity.HasIndex(e => e.PricedAt);
                entity.HasIndex(e => new { e.OptionId, e.PricedAt });
            });

            // Configure the Portfolios table
            modelBuilder.Entity<PortfolioEntity>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Description)
                    .HasMaxLength(1000);

                entity.Property(e => e.Cash)
                    .IsRequired()
                    .HasDefaultValue(0.0);

                entity.Property(e => e.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.UpdatedAt)
                    .IsRequired(false);

                // Create index on Name for searching
                entity.HasIndex(e => e.Name);
            });

            // Configure the Trades table
            modelBuilder.Entity<TradeEntity>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.AssetType)
                    .HasConversion<int>()
                    .IsRequired();

                entity.Property(e => e.TradeType)
                    .HasConversion<int>()
                    .IsRequired();

                entity.Property(e => e.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Foreign key relationship to Portfolio
                entity.HasOne(e => e.Portfolio)
                    .WithMany(p => p.Trades)
                    .HasForeignKey(e => e.PortfolioId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Foreign key relationship to Stock (nullable)
                entity.HasOne(e => e.Stock)
                    .WithMany()
                    .HasForeignKey(e => e.StockId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);

                // Foreign key relationship to Option (nullable)
                entity.HasOne(e => e.Option)
                    .WithMany()
                    .HasForeignKey(e => e.OptionId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);

                // Create indexes for efficient querying
                entity.HasIndex(e => e.PortfolioId);
                entity.HasIndex(e => e.StockId);
                entity.HasIndex(e => e.OptionId);
                entity.HasIndex(e => e.TradeDate);
                entity.HasIndex(e => new { e.PortfolioId, e.TradeDate });
            });

            // Configure the Positions table
            modelBuilder.Entity<PositionEntity>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.AssetType)
                    .HasConversion<int>()
                    .IsRequired();

                entity.Property(e => e.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.LastUpdated)
                    .IsRequired();

                // Foreign key relationship to Portfolio
                entity.HasOne(e => e.Portfolio)
                    .WithMany(p => p.Positions)
                    .HasForeignKey(e => e.PortfolioId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Foreign key relationship to Stock (nullable)
                entity.HasOne(e => e.Stock)
                    .WithMany()
                    .HasForeignKey(e => e.StockId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);

                // Foreign key relationship to Option (nullable)
                entity.HasOne(e => e.Option)
                    .WithMany()
                    .HasForeignKey(e => e.OptionId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);

                // Unique constraint: one position per (Portfolio, AssetType, StockId/OptionId) combination
                // We use a filtered index approach - separate unique indexes for stock and option positions
                entity.HasIndex(e => new { e.PortfolioId, e.StockId })
                    .IsUnique()
                    .HasFilter("\"StockId\" IS NOT NULL");

                entity.HasIndex(e => new { e.PortfolioId, e.OptionId })
                    .IsUnique()
                    .HasFilter("\"OptionId\" IS NOT NULL");

                // Additional indexes for efficient querying
                entity.HasIndex(e => e.PortfolioId);
                entity.HasIndex(e => e.StockId);
                entity.HasIndex(e => e.OptionId);
            });

            // Configure the Stocks table
            modelBuilder.Entity<StockEntity>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Ticker)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.CurrentPrice)
                    .IsRequired();

                entity.Property(e => e.Description)
                    .HasMaxLength(500);

                entity.Property(e => e.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.UpdatedAt)
                    .IsRequired(false);

                // Foreign key relationship to Exchange
                entity.HasOne(e => e.Exchange)
                    .WithMany(ex => ex.Stocks)
                    .HasForeignKey(e => e.ExchangeId)
                    .OnDelete(DeleteBehavior.Restrict); // Don't delete exchange if stocks exist

                // Create unique index on (Ticker, ExchangeId) - same ticker can exist on different exchanges
                entity.HasIndex(e => new { e.Ticker, e.ExchangeId })
                    .IsUnique();

                // Create index on Name for searching
                entity.HasIndex(e => e.Name);

                // Create index on ExchangeId for efficient querying
                entity.HasIndex(e => e.ExchangeId);
            });

            // Configure the Exchanges table
            modelBuilder.Entity<ExchangeEntity>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Description)
                    .HasMaxLength(500);

                entity.Property(e => e.Country)
                    .HasMaxLength(100);

                entity.Property(e => e.Currency)
                    .HasMaxLength(10);

                entity.Property(e => e.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Create unique index on Name to prevent duplicate exchanges
                entity.HasIndex(e => e.Name)
                    .IsUnique();
            });
        }
    }
}
