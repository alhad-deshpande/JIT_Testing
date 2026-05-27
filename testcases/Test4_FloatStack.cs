using System;

// Test 4: Float arguments with stack overflow (15 args: 13 in f1-f13, 2 on stack)
class Program
{
    static double ppc64leHelloWorldCallee(double a, double b, double c, double d, double e,
                                          double f, double g, double h, double i, double j,
                                          double k, double l, double m, double n, double o)
    {
        return a + b + c + d + e + f + g + h + i + j + k + l + m + n + o;
    }

    static double ppc64leHelloWorld()
    {
        double sum = ppc64leHelloWorldCallee(1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0,
                                              9.0, 10.0, 11.0, 12.0, 13.0, 14.0, 15.0);
        return sum;
    }

    static void Main(string[] args)
    {
        double retVal = ppc64leHelloWorld();
        Console.WriteLine("Test 4: Float arguments with stack (15 args)");
        Console.WriteLine("Result: {0:F1}, Expected: 120.0", retVal);
        Console.WriteLine("Status: {0}", Math.Abs(retVal - 120.0) < 0.01 ? "PASS" : "FAIL");
    }
}

// Made with Bob
