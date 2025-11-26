using Microsoft.EntityFrameworkCore;
using MonteCarloAPI.Data;
using MonteCarloAPI.Models;

namespace MonteCarloAPI.Services
{
    /// <summary>
    /// Service for managing option configurations using PostgreSQL database.
    /// Replaces in-memory storage with persistent database storage.
    /// </summary>
    public class OptionService
    {
        private readonly MonteCarloDbContext _context;

        public OptionService(MonteCarloDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Add a new option configuration to the database
        /// </summary>
        public async Task<OptionConfigDTO> AddOptionAsync(OptionConfigDTO option)
        {
            // Validate that the stock exists
            var stockExists = await _context.Stocks.AnyAsync(s => s.Id == option.StockId);
            if (!stockExists)
            {
                throw new ArgumentException($"Stock with ID {option.StockId} does not exist.");
            }

            var entity = MapToEntity(option);
            entity.CreatedAt = DateTime.UtcNow;

            _context.Options.Add(entity);
            await _context.SaveChangesAsync();

            // Reload the entity with Stock navigation property
            await _context.Entry(entity).Reference(o => o.Stock).LoadAsync();

            return MapToDTO(entity);
        }

        /// <summary>
        /// Retrieve an option by ID from the database
        /// </summary>
        public async Task<OptionConfigDTO?> GetOptionByIdAsync(int id)
        {
            var entity = await _context.Options
                .Include(o => o.Stock)  // Include Stock navigation property
                .FirstOrDefaultAsync(o => o.Id == id);

            return entity == null ? null : MapToDTO(entity);
        }

        /// <summary>
        /// Retrieve all options from the database
        /// </summary>
        public async Task<List<OptionConfigDTO>> GetAllOptionsAsync()
        {
            var entities = await _context.Options
                .Include(o => o.Stock)  // Include Stock navigation property
                .OrderBy(o => o.Id)
                .ToListAsync();

            return entities.Select(MapToDTO).ToList();
        }

        /// <summary>
        /// Update an existing option in the database
        /// </summary>
        public async Task<OptionConfigDTO?> UpdateOptionAsync(int id, OptionConfigDTO updated)
        {
            var entity = await _context.Options
                .Include(o => o.Stock)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (entity == null)
                return null;

            // Validate that the new stock exists if StockId is being changed
            if (entity.StockId != updated.StockId)
            {
                var stockExists = await _context.Stocks.AnyAsync(s => s.Id == updated.StockId);
                if (!stockExists)
                {
                    throw new ArgumentException($"Stock with ID {updated.StockId} does not exist.");
                }
                entity.StockId = updated.StockId;
            }

            // Update core parameters
            entity.OptionType = updated.OptionParameters.OptionType;
            entity.Strike = updated.OptionParameters.Strike;
            entity.IsCall = updated.OptionParameters.IsCall;
            entity.ExpiryDate = updated.OptionParameters.ExpiryDate;
            entity.UpdatedAt = DateTime.UtcNow;

            // Clear all type-specific fields first
            entity.AveragingType = null;
            entity.ObservationFrequency = null;
            entity.DigitalCondition = null;
            entity.BarrierOptionType = null;
            entity.BarrierDir = null;
            entity.BarrierLevel = null;
            entity.LookbackOptionType = null;
            entity.RangeObservationFrequency = null;

            // Set only relevant fields based on option type
            switch (updated.OptionParameters.OptionType)
            {
                case OptionType.Asian:
                    entity.AveragingType = updated.OptionParameters.AveragingType;
                    entity.ObservationFrequency = updated.OptionParameters.ObservationFrequency;
                    break;

                case OptionType.Digital:
                    entity.DigitalCondition = updated.OptionParameters.DigitalCondition;
                    break;

                case OptionType.Barrier:
                    entity.BarrierOptionType = updated.OptionParameters.BarrierOptionType;
                    entity.BarrierDir = updated.OptionParameters.BarrierDir;
                    entity.BarrierLevel = updated.OptionParameters.BarrierLevel;
                    break;

                case OptionType.Lookback:
                    entity.LookbackOptionType = updated.OptionParameters.LookbackOptionType;
                    break;

                case OptionType.Range:
                    entity.RangeObservationFrequency = updated.OptionParameters.RangeObservationFrequency;
                    break;
            }

            await _context.SaveChangesAsync();

            return MapToDTO(entity);
        }

        /// <summary>
        /// Delete an option from the database
        /// </summary>
        public async Task<bool> DeleteOptionAsync(int id)
        {
            var entity = await _context.Options.FindAsync(id);
            if (entity == null)
                return false;

            _context.Options.Remove(entity);
            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Map database entity to DTO
        /// </summary>
        public static OptionConfigDTO MapToDTO(OptionEntity entity)
        {
            return new OptionConfigDTO
            {
                Id = entity.Id,
                StockId = entity.StockId,
                Stock = entity.Stock != null ? StockDTO.FromEntity(entity.Stock) : null,
                CreatedAt = entity.CreatedAt,
                OptionParameters = new OptionParametersDTO
                {
                    OptionType = entity.OptionType,
                    Strike = entity.Strike,
                    IsCall = entity.IsCall,
                    ExpiryDate = entity.ExpiryDate,
                    // Nullable fields - use defaults if NULL
                    AveragingType = entity.AveragingType ?? AveragingType.Arithmetic,
                    ObservationFrequency = entity.ObservationFrequency ?? 1,
                    DigitalCondition = entity.DigitalCondition ?? ConditionType.AboveStrike,
                    BarrierOptionType = entity.BarrierOptionType ?? BarrierType.KnockOut,
                    BarrierDir = entity.BarrierDir ?? BarrierDirection.Up,
                    BarrierLevel = entity.BarrierLevel ?? 0.0,
                    LookbackOptionType = entity.LookbackOptionType ?? LookbackType.Max,
                    RangeObservationFrequency = entity.RangeObservationFrequency ?? 1
                }
            };
        }

        /// <summary>
        /// Map DTO to database entity, setting NULL for irrelevant fields based on option type
        /// </summary>
        private static OptionEntity MapToEntity(OptionConfigDTO dto)
        {
            var entity = new OptionEntity
            {
                StockId = dto.StockId,
                OptionType = dto.OptionParameters.OptionType,
                Strike = dto.OptionParameters.Strike,
                IsCall = dto.OptionParameters.IsCall,
                ExpiryDate = dto.OptionParameters.ExpiryDate
            };

            // Set type-specific fields based on option type
            switch (dto.OptionParameters.OptionType)
            {
                case OptionType.European:
                    // European options have no type-specific parameters - all NULL
                    break;

                case OptionType.Asian:
                    entity.AveragingType = dto.OptionParameters.AveragingType;
                    entity.ObservationFrequency = dto.OptionParameters.ObservationFrequency;
                    break;

                case OptionType.Digital:
                    entity.DigitalCondition = dto.OptionParameters.DigitalCondition;
                    break;

                case OptionType.Barrier:
                    entity.BarrierOptionType = dto.OptionParameters.BarrierOptionType;
                    entity.BarrierDir = dto.OptionParameters.BarrierDir;
                    entity.BarrierLevel = dto.OptionParameters.BarrierLevel;
                    break;

                case OptionType.Lookback:
                    entity.LookbackOptionType = dto.OptionParameters.LookbackOptionType;
                    break;

                case OptionType.Range:
                    entity.RangeObservationFrequency = dto.OptionParameters.RangeObservationFrequency;
                    break;
            }

            return entity;
        }
    }
}
