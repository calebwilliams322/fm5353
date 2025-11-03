using System;

namespace MonteCarloOptionPricer.Simulation
{
    public class RandomNumberGenerator
    {
        static Random rand = new Random();

        public static void Seed(int seed)
        {
            rand = new Random(seed);
        }

        public static double NextStandardNormal()
        {
            // Box Muller Method   
            double u = rand.NextDouble();
            double v = rand.NextDouble();
            double r = Math.Sqrt(-2.0 * Math.Log(u));
            double theta = 2.0 * Math.PI * v;

            double z1 = r * Math.Cos(theta);
            double z2 = r * Math.Sin(theta);

            return z1;

        }

        public static (double, double) NextTwoStandardNormals()
        {

            // Returns both numbers from the Box Muller Method 
            double u = rand.NextDouble();
            double v = rand.NextDouble();
            double r = Math.Sqrt(-2.0 * Math.Log(u));
            double theta = 2.0 * Math.PI * v;

            double z1 = r * Math.Cos(theta);
            double z2 = r * Math.Sin(theta);

            return (z1, z2);
        }


    }


}