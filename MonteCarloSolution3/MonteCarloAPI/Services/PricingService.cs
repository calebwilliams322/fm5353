using MonteCarloAPI.Models;
using MonteCarloOptionPricer.OptionModels;
using MonteCarloOptionPricer.Models;

namespace MonteCarloAPI.Services
{
    public class PricingService
    {
        public Task<PricingResultDTO> PriceOptionAsync(OptionConfigDTO optionConfig, SimulationParametersDTO simParams)
        {
            // Validate simulation parameters
            var validation = ValidateSimulationParameters(simParams, optionConfig.OptionParameters);
            if (!validation.IsValid)
                throw new ArgumentException(validation.ErrorMessage);

            // Create the appropriate option instance based on option type
            var option = CreateOption(optionConfig, simParams.InitialPrice);

            // Convert SimulationParametersDTO enums to library enums
            var simMode = ConvertSimulationMode(simParams.SimMode);

            // Price the option
            var result = option.GetPrice(
                volatility: simParams.Volatility,
                riskFreeRate: simParams.RiskFreeRate,
                timeSteps: simParams.TimeSteps,
                numberOfPaths: simParams.NumberOfPaths,
                calculateGreeks: true,
                useMultithreading: simParams.UseMultithreading,
                simMode: simMode
            );

            // Map to DTO
            var dto = new PricingResultDTO
            {
                Price = result.Price,
                StandardError = result.StandardError,
                Delta = result.Delta,
                Gamma = result.Gamma,
                Vega = result.Vega,
                Theta = result.Theta,
                Rho = result.Rho,
                Timestamp = DateTime.UtcNow,
                OptionConfigId = optionConfig.Id,
                SimulationMode = simParams.SimMode.ToString()
            };

            return Task.FromResult(dto);
        }

        private ValidationResult ValidateSimulationParameters(SimulationParametersDTO simParams, OptionParametersDTO optionParams)
        {
            // Validate initial price
            if (simParams.InitialPrice <= 0)
                return ValidationResult.Fail("Initial price must be greater than 0.");

            // Validate volatility
            if (simParams.Volatility <= 0)
                return ValidationResult.Fail("Volatility must be greater than 0.");
            if (simParams.Volatility > 5.0)
                return ValidationResult.Fail("Volatility cannot exceed 500% (5.0). Please check your input.");

            // Validate risk-free rate (allow negative rates for some markets)
            if (simParams.RiskFreeRate < -0.1 || simParams.RiskFreeRate > 1.0)
                return ValidationResult.Fail("Risk-free rate must be between -10% and 100%.");

            // Validate time to expiry
            if (simParams.TimeToExpiry <= 0)
                return ValidationResult.Fail("Time to expiry must be greater than 0.");
            if (simParams.TimeToExpiry > 50)
                return ValidationResult.Fail("Time to expiry cannot exceed 50 years.");

            // Validate time steps
            if (simParams.TimeSteps <= 0)
                return ValidationResult.Fail("Time steps must be greater than 0.");
            if (simParams.TimeSteps > 10000)
                return ValidationResult.Fail("Time steps cannot exceed 10,000 for performance reasons.");

            // Validate number of paths
            if (simParams.NumberOfPaths <= 0)
                return ValidationResult.Fail("Number of paths must be greater than 0.");
            if (simParams.NumberOfPaths > 1000000)
                return ValidationResult.Fail("Number of paths cannot exceed 1,000,000 for performance reasons.");

            // Validate strike
            if (optionParams.Strike <= 0)
                return ValidationResult.Fail("Strike price must be greater than 0.");

            // Validate barrier options
            if (optionParams.OptionType == Models.OptionType.Barrier)
            {
                if (optionParams.BarrierLevel <= 0)
                    return ValidationResult.Fail("Barrier level must be greater than 0.");

                // Check barrier direction makes sense
                if (optionParams.BarrierDir == Models.BarrierDirection.Up && optionParams.BarrierLevel <= simParams.InitialPrice)
                    return ValidationResult.Fail("Up barrier must be above initial price.");

                if (optionParams.BarrierDir == Models.BarrierDirection.Down && optionParams.BarrierLevel >= simParams.InitialPrice)
                    return ValidationResult.Fail("Down barrier must be below initial price.");
            }

            return ValidationResult.Success();
        }

        private IOption CreateOption(OptionConfigDTO config, double initialPrice)
        {
            var optParams = config.OptionParameters;
            var expiry = DateTime.Today.AddYears(1); // Default 1 year expiry

            return (OptionType)optParams.OptionType switch
            {
                OptionType.European => new EuropeanOption(
                    initialPrice,
                    optParams.Strike,
                    expiry,
                    optParams.IsCall
                ),

                OptionType.Asian => new AsianOption(
                    initialPrice,
                    optParams.Strike,
                    expiry,
                    optParams.IsCall,
                    ConvertAveragingType(optParams.AveragingType)
                ),

                OptionType.Digital => new DigitalOption(
                    initialPrice,
                    optParams.Strike,
                    expiry,
                    optParams.IsCall,
                    isCashOrNothing: true,
                    payout: 1.0
                ),

                OptionType.Barrier => new BarrierOption(
                    initialPrice,
                    optParams.Strike,
                    expiry,
                    optParams.IsCall,
                    ConvertBarrierType(optParams.BarrierOptionType),
                    ConvertBarrierDirection(optParams.BarrierDir),
                    optParams.BarrierLevel
                ),

                OptionType.Lookback => new LookbackOption(
                    initialPrice,
                    optParams.Strike,
                    expiry,
                    optParams.IsCall,
                    ConvertLookbackType(optParams.LookbackOptionType)
                ),

                OptionType.Range => new RangeOption(
                    initialPrice,
                    optParams.Strike,
                    expiry
                ),

                _ => throw new ArgumentException($"Unsupported option type: {optParams.OptionType}")
            };
        }

        // Conversion methods between DTO enums and library enums
        private MonteCarloOptionPricer.Models.AveragingType ConvertAveragingType(Models.AveragingType dto)
            => dto == Models.AveragingType.Arithmetic
                ? MonteCarloOptionPricer.Models.AveragingType.Arithmetic
                : MonteCarloOptionPricer.Models.AveragingType.Geometric;

        private MonteCarloOptionPricer.Models.BarrierType ConvertBarrierType(Models.BarrierType dto)
            => dto == Models.BarrierType.KnockIn
                ? MonteCarloOptionPricer.Models.BarrierType.KnockIn
                : MonteCarloOptionPricer.Models.BarrierType.KnockOut;

        private MonteCarloOptionPricer.Models.BarrierDirection ConvertBarrierDirection(Models.BarrierDirection dto)
            => dto == Models.BarrierDirection.Up
                ? MonteCarloOptionPricer.Models.BarrierDirection.Up
                : MonteCarloOptionPricer.Models.BarrierDirection.Down;

        private MonteCarloOptionPricer.Models.LookbackType ConvertLookbackType(Models.LookbackType dto)
            => dto == Models.LookbackType.Max
                ? MonteCarloOptionPricer.Models.LookbackType.Max
                : MonteCarloOptionPricer.Models.LookbackType.Min;

        private MonteCarloOptionPricer.Models.SimulationMode ConvertSimulationMode(Models.SimulationMode dto)
            => dto switch
            {
                Models.SimulationMode.Plain => MonteCarloOptionPricer.Models.SimulationMode.Plain,
                Models.SimulationMode.Antithetic => MonteCarloOptionPricer.Models.SimulationMode.Antithetic,
                Models.SimulationMode.ControlVariate => MonteCarloOptionPricer.Models.SimulationMode.ControlVariate,
                Models.SimulationMode.AntitheticAndControlVariate => MonteCarloOptionPricer.Models.SimulationMode.AntitheticAndControlVariate,
                Models.SimulationMode.VanDerCorput => MonteCarloOptionPricer.Models.SimulationMode.VanDerCorput,
                _ => MonteCarloOptionPricer.Models.SimulationMode.Plain
            };
    }
}
