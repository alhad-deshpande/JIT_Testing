using System;

// Test 5: Mixed integer and float arguments (6 args)
// Note: Float args consume both float register AND corresponding int register slot
class Program
{
    static double ppc64leHelloWorldCallee(int a, double b, int c, double d, int e, double f)
    {
        return a + b + c + d + e + f;
    }

    static double ppc64leHelloWorld()
    {
        double sum = ppc64leHelloWorldCallee(10, 20.5, 30, 40.5, 50, 60.5);
        return sum;
    }

    static void Main(string[] args)
    {
        double retVal = ppc64leHelloWorld();
        Console.WriteLine("Test 5: Mixed integer and float arguments (6 args)");
        Console.WriteLine("Result: {0:F1}, Expected: 211.5", retVal);
        Console.WriteLine("Status: {0}", Math.Abs(retVal - 211.5) < 0.01 ? "PASS" : "FAIL");
    }
}

// Made with Bob
