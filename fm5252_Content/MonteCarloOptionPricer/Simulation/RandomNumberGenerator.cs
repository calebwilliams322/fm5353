using System;


namespace MonteCarloOptionPricer.Simulation
{

    // Use code from previous week

    public static class NormalGenerator
    {

    static Random rand = new Random();


    public static void Seed(int seed)
    {
        rand = new Random(seed);
    }

    
    public static double NextStandardNormal()
    {

        // Box Muller Method! 

        double u = rand.NextDouble();
        double v = rand.NextDouble();
        double r = Math.Sqrt(-2.0 * Math.Log(u));
        double theta = 2.0 * Math.PI * v;

        double z1 = r * Math.Cos(theta);
        double z2 = r * Math.Sin(theta);


        // I am choosing to return only ONE of the two random numbers generated
        // Utilizing both z1 and z2 is an opportunity for improved eff in the future

        return z1;
    }

    public static (double, double) NextStandardNormalBothValues()
    {

        // Box Muller Method! 

        double u = rand.NextDouble();
        double v = rand.NextDouble();
        double r = Math.Sqrt(-2.0 * Math.Log(u));
        double theta = 2.0 * Math.PI * v;

        double z1 = r * Math.Cos(theta);
        double z2 = r * Math.Sin(theta);


        // I am choosing to return only ONE of the two random numbers generated
        // Utilizing both z1 and z2 is an opportunity for improved eff in the future

        return (z1, z2);
    }






    }
}

