using System;

namespace MonteCarloOptionPricer.TestTools
{
    /// <summary>
    /// Provides analytical Black–Scholes pricing for European options,
    /// for testing Monte Carlo convergence and correctness.
    /// </summary>
    public static class BlackScholesHelper
    {
        /// <summary>
        /// Computes the Black–Scholes price of a European call or put.
        /// </summary>
        public static double Price(double S, double K, double r, double sigma, double T, bool isCall)
        {
            double d1 = (Math.Log(S / K) + (r + 0.5 * sigma * sigma) * T) / (sigma * Math.Sqrt(T));
            double d2 = d1 - sigma * Math.Sqrt(T);

            double Nd1 = Cdf(d1);
            double Nd2 = Cdf(d2);
            double Nnd1 = Cdf(-d1);
            double Nnd2 = Cdf(-d2);

            return isCall
                ? S * Nd1 - K * Math.Exp(-r * T) * Nd2
                : K * Math.Exp(-r * T) * Nnd2 - S * Nnd1;
        }

        /// <summary>
        /// Standard normal cumulative distribution function.
        /// </summary>
        public static double Cdf(double x)
        {
            return 0.5 * (1.0 + Erf(x / Math.Sqrt(2.0)));
        }

        /// <summary>
        /// Error function approximation (Abramowitz–Stegun 7.1.26).
        /// </summary>
        private static double Erf(double x)
        {
            double sign = Math.Sign(x);
            x = Math.Abs(x);

            double a1 = 0.254829592;
            double a2 = -0.284496736;
            double a3 = 1.421413741;
            double a4 = -1.453152027;
            double a5 = 1.061405429;
            double p = 0.3275911;

            double t = 1.0 / (1.0 + p * x);
            double y = 1.0 - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * Math.Exp(-x * x);

            return sign * y;
        }
    }
}