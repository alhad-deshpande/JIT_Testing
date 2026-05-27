using System;

// Test 3: Float/Double arguments in registers (8 args in f1-f8)
class Program
{
    static double ppc64leHelloWorldCallee(double a, double b, double c, double d, 
                                          double e, double f, double g, double h)
    {
        return a + b + c + d + e + f + g + h;
    }

    static double ppc64leHelloWorld()
    {
        double sum = ppc64leHelloWorldCallee(1.1, 2.2, 3.3, 4.4, 5.5, 6.6, 7.7, 8.8);
        return sum;
    }

    static void Main(string[] args)
    {
        double retVal = ppc64leHelloWorld();
        Console.WriteLine("Test 3: Float arguments in registers (8 args)");
        Console.WriteLine("Result: {0:F1}, Expected: 39.6", retVal);
        Console.WriteLine("Status: {0}", Math.Abs(retVal - 39.6) < 0.01 ? "PASS" : "FAIL");
    }
}

// Made with Bob
