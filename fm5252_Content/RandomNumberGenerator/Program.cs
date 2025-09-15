// See https://aka.ms/new-console-template for more information
using System;

class NormalGenerator
{
    // Static Random instance to use across methods
    static Random rand = new Random();

    static void Main()
    {
        Console.WriteLine(
        "\nHello! Welcome to my Random Number Generator! Every 'sample' provided by this app\n" +
        "is a random Gaussian value and a resulting correlated value. \n"
        );

        Console.Write("How many samples do you want to generate for each method? ");
        int n;
        while (!int.TryParse(Console.ReadLine(), out n))
        {
            Console.Write("Invalid input. Please enter an integer: ");
        }
        
        // Prompt for the correlation coefficient (rho) and ensure it is between -1 and 1.
        Console.Write("Enter correlation coefficient (rho) between -1 and 1: ");
        double rho;
        while (!double.TryParse(Console.ReadLine(), out rho) || rho < -1 || rho > 1)
        {
            Console.Write("Invalid input. Please enter a number between -1 and 1: ");
        }
        
        Console.WriteLine("\n=== Sum Twelve Method ===");
        for (int i = 0; i < n; i++)
        {
            double z1 = SumTwelve();
            double z2 = SumTwelve();
            double correlatedZ2 = rho * z1 + Math.Sqrt(1 - rho * rho) * z2;
            Console.WriteLine($"Sample {i + 1}: z1 = {z1:F4}, correlated z2 = {correlatedZ2:F4}");
        }

        Console.WriteLine("\n=== Box-Muller Method ===");
        for (int i = 0; i < n; i++)
        {
            (double z1, double z2) = BoxMuller();
            double correlatedZ2 = rho * z1 + Math.Sqrt(1 - rho * rho) * z2;
            Console.WriteLine($"Sample {i + 1}: z1 = {z1:F4}, correlated z2 = {correlatedZ2:F4}");
        }

        Console.WriteLine("\n=== Polar Rejection Method ===");
        for (int i = 0; i < n; i++)
        {
            (double z1, double z2) = PolarRejection();
            double correlatedZ2 = rho * z1 + Math.Sqrt(1 - rho * rho) * z2;
            Console.WriteLine($"Sample {i + 1}: z1 = {z1:F4}, correlated z2 = {correlatedZ2:F4}");
        }
    }

    static double SumTwelve()
    {
        double sum = 0.0;
        for (int i = 0; i < 12; i++)
        {
            sum += rand.NextDouble();
        }
        return sum - 6.0; 
    }

    static (double, double) BoxMuller()
    {
        double u = rand.NextDouble();
        double v = rand.NextDouble();
        double r = Math.Sqrt(-2.0 * Math.Log(u));
        double theta = 2.0 * Math.PI * v;

        double z1 = r * Math.Cos(theta);
        double z2 = r * Math.Sin(theta);

        return (z1, z2);
    }

    static (double, double) PolarRejection()
    {
        double x1, x2, w;
        do
        {
            x1 = 2.0 * rand.NextDouble() - 1.0;
            x2 = 2.0 * rand.NextDouble() - 1.0;
            w = x1 * x1 + x2 * x2;
        }
        while (w >= 1.0 || w == 0.0);

        double c = Math.Sqrt(-2.0 * Math.Log(w) / w);
        return (c * x1, c * x2);
    }
}


