using System;

// Test 6: Mixed arguments with interleaved int/double to test register slot consumption
// Tests that float args consume corresponding int register slots in PPC64LE ELFv2 ABI
class Program
{
    static double ppc64leHelloWorldCallee(int a, double b, int c, double d, int e, double f, 
                                          int g, double h, int i, double j, int k, double l,
                                          int m, double n)
    {
        return a + b + c + d + e + f + g + h + i + j + k + l + m + n;
    }

    static double ppc64leHelloWorld()
    {
        double sum = ppc64leHelloWorldCallee(1, 2.0, 3, 4.0, 5, 6.0, 
                                              7, 8.0, 9, 10.0, 11, 12.0,
                                              13, 14.0);
        return sum;
    }

    static void Main(string[] args)
    {
        double retVal = ppc64leHelloWorld();
        Console.WriteLine("Test 6: Mixed interleaved int/double arguments (14 args)");
        Console.WriteLine("Result: {0:F1}, Expected: 105.0", retVal);
        Console.WriteLine("Status: {0}", Math.Abs(retVal - 105.0) < 0.01 ? "PASS" : "FAIL");
    }
}

// Made with Bob
