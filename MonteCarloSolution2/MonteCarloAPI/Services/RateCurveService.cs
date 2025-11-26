using Microsoft.Extensions.Options;
using MonteCarloAPI.Configuration;

namespace MonteCarloAPI.Services
{
    /// <summary>
    /// Service for retrieving risk-free rates from the configured rate curve.
    /// Uses linear interpolation between curve points.
    /// </summary>
    public class RateCurveService
    {
        private readonly List<RateCurvePoint> _curvePoints;

        public RateCurveService(IOptions<RateCurveConfiguration> config)
        {
            _curvePoints = config.Value.Points
                .OrderBy(p => p.TenorYears)
                .ToList();

            if (_curvePoints.Count == 0)
            {
                throw new InvalidOperationException("Rate curve configuration is empty. Please configure RateCurve:Points in appsettings.json");
            }
        }

        /// <summary>
        /// Get the interpolated risk-free rate for a given time to expiry.
        /// Uses linear interpolation between curve points.
        /// </summary>
        /// <param name="timeToExpiryYears">Time to expiry in years</param>
        /// <returns>Interpolated risk-free rate</returns>
        public double GetRate(double timeToExpiryYears)
        {
            if (timeToExpiryYears <= 0)
            {
                throw new ArgumentException("Time to expiry must be positive", nameof(timeToExpiryYears));
            }

            // If before the first point, use the first rate (flat extrapolation)
            if (timeToExpiryYears <= _curvePoints[0].TenorYears)
            {
                return _curvePoints[0].Rate;
            }

            // If after the last point, use the last rate (flat extrapolation)
            if (timeToExpiryYears >= _curvePoints[^1].TenorYears)
            {
                return _curvePoints[^1].Rate;
            }

            // Find the two points to interpolate between
            for (int i = 0; i < _curvePoints.Count - 1; i++)
            {
                var lower = _curvePoints[i];
                var upper = _curvePoints[i + 1];

                if (timeToExpiryYears >= lower.TenorYears && timeToExpiryYears <= upper.TenorYears)
                {
                    // Linear interpolation: r = r1 + (r2 - r1) * (t - t1) / (t2 - t1)
                    double t1 = lower.TenorYears;
                    double t2 = upper.TenorYears;
                    double r1 = lower.Rate;
                    double r2 = upper.Rate;

                    double interpolatedRate = r1 + (r2 - r1) * (timeToExpiryYears - t1) / (t2 - t1);
                    return interpolatedRate;
                }
            }

            // Fallback (should never reach here)
            return _curvePoints[^1].Rate;
        }

        /// <summary>
        /// Calculate time to expiry in years from an expiry date.
        /// </summary>
        /// <param name="expiryDate">The option's expiry date</param>
        /// <returns>Time to expiry in years</returns>
        public static double CalculateTimeToExpiry(DateTime expiryDate)
        {
            var today = DateTime.UtcNow.Date;
            var expiry = expiryDate.Date;

            if (expiry <= today)
            {
                throw new ArgumentException($"Option has expired or expires today. Expiry date: {expiryDate:yyyy-MM-dd}");
            }

            // Calculate time to expiry in years (using 365.25 days per year)
            double daysToExpiry = (expiry - today).TotalDays;
            return daysToExpiry / 365.25;
        }

        /// <summary>
        /// Get all configured curve points (for debugging/display)
        /// </summary>
        public IReadOnlyList<RateCurvePoint> GetCurvePoints() => _curvePoints.AsReadOnly();
    }
}
