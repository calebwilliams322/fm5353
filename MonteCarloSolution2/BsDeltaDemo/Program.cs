// See https://aka.ms/new-console-template for more information
using System;

namespace BsDeltaDemo
{
    public enum OptionType { Call, Put }

    public static class BlackScholes
    {
        /// <summary>
        /// Black–Scholes Delta for European options.
        /// S: spot, K: strike, r: risk-free rate (cont. comp.), q: dividend yield (cont. comp.),
        /// sigma: volatility (annualized), T: time to maturity in years.
        /// </summary>
        public static double Delta(double S, double K, double r, double q, double sigma, double T, OptionType type)
        {
            if (S <= 0) throw new ArgumentOutOfRangeException(nameof(S), "Spot must be > 0.");
            if (K <= 0) throw new ArgumentOutOfRangeException(nameof(K), "Strike must be > 0.");
            if (sigma <= 0) throw new ArgumentOutOfRangeException(nameof(sigma), "Vol must be > 0.");
            if (T <= 0) throw new ArgumentOutOfRangeException(nameof(T), "Time to maturity must be > 0.");

            double sqrtT = Math.Sqrt(T);
            double d1 = (Math.Log(S / K) + (r - q + 0.5 * sigma * sigma) * T) / (sigma * sqrtT);

            // e^{-qT} * N(d1) for call; e^{-qT} * (N(d1) - 1) for put
            double Nd1 = StandardNormalCdf(d1);
            double discQ = Math.Exp(-q * T);

            return type == OptionType.Call
                ? discQ * Nd1
                : discQ * (Nd1 - 1.0);
        }

        /// <summary>
        /// Overload with q = 0.
        /// </summary>
        public static double Delta(double S, double K, double r, double sigma, double T, OptionType type)
            => Delta(S, K, r, 0.0, sigma, T, type);

        /// <summary>
        /// Standard normal CDF Φ(x) via an Abramowitz–Stegun style approximation.
        /// Accurate to ~1e-7 or better for practical purposes.
        /// </summary>
        private static double StandardNormalCdf(double x)
        {
            // For numerical stability in tails, use symmetry Φ(-x) = 1 - Φ(x)
            if (x < 0) return 1.0 - StandardNormalCdf(-x);

            // Constants for rational approximation (Hart/AS 26.2.17 style)
            double p = 0.2316419;
            double b1 = 0.319381530;
            double b2 = -0.356563782;
            double b3 = 1.781477937;
            double b4 = -1.821255978;
            double b5 = 1.330274429;

            double t = 1.0 / (1.0 + p * x);
            double pdf = Math.Exp(-0.5 * x * x) / Math.Sqrt(2.0 * Math.PI);
            double poly = (((((b5 * t + b4) * t) + b3) * t + b2) * t + b1) * t;

            return 1.0 - pdf * poly;
        }
    }

    internal class Program
    {
        static void Main()
        {
            // Quick demo:
            // Example: S=100, K=100, r=2%, q=0, sigma=20%, T=1y
            double callDelta = BlackScholes.Delta(100, 100, 0.02, 0.00, 0.20, 1.0, OptionType.Call);
            double putDelta  = BlackScholes.Delta(100, 100, 0.02, 0.00, 0.20, 1.0, OptionType.Put);

            Console.WriteLine($"Call Delta: {callDelta:F6}");
            Console.WriteLine($" Put Delta: {putDelta:F6}");

            // Sanity checks:
            // Deep ITM call → delta ~ e^{-qT} ≈ 1; Deep OTM call → delta ~ 0
            // Deep ITM put  → delta ~ -e^{-qT} ≈ -1; Deep OTM put → delta ~ 0
        }
    }
}
