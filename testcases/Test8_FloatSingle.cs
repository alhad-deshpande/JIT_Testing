using System;

// Test 8: Single precision float arguments (5 args in f1-f5)
class Program
{
    static float ppc64leHelloWorldCallee(float a, float b, float c, float d, float e)
    {
        return a + b + c + d + e;
    }

    static float ppc64leHelloWorld()
    {
        float sum = ppc64leHelloWorldCallee(1.5f, 2.5f, 3.5f, 4.5f, 5.5f);
        return sum;
    }

    static void Main(string[] args)
    {
        float retVal = ppc64leHelloWorld();
        Console.WriteLine("Test 8: Single precision float arguments (5 args)");
        Console.WriteLine("Result: {0:F1}, Expected: 17.5", retVal);
        Console.WriteLine("Status: {0}", Math.Abs(retVal - 17.5f) < 0.01 ? "PASS" : "FAIL");
    }
}

// Made with Bob
