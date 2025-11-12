using System;

namespace MonteCarloOptionPricer.Simulation
{
    /// <summary>
    /// Generates standard normal random variables using the Box–Muller transform.
    /// Only returns one of the two generated values to keep implementation simple.
    /// </summary>
    public static class RandomNumberGenerator
    {
        private static Random _rand = new Random();

        /// <summary>
        /// Reseeds the generator for reproducibility.
        /// </summary>
        public static void Seed(int seed)
        {
            _rand = new Random(seed);
        }

        /// <summary>
        /// Returns a single standard normal (mean 0, std 1) random variable.
        /// </summary>
        public static double NextStandardNormal()
        {
            double u, v;

            // Ensure u isn't 0 to avoid log(0)
            do
            {
                u = _rand.NextDouble();
            } while (u == 0.0);

            v = _rand.NextDouble();

            double r = Math.Sqrt(-2.0 * Math.Log(u));
            double theta = 2.0 * Math.PI * v;

            return r * Math.Cos(theta);
        }  

    }
}