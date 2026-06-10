using System;
using System.Runtime.InteropServices;

// Test 17: Struct with double fields (16 bytes)
// Tests struct with two 8-byte doubles passed in two registers
[StructLayout(LayoutKind.Sequential)]
struct DoubleStruct
{
    public double field1;
    public double field2;
}

class Program
{
    static double ppc64leHelloWorldCallee(DoubleStruct s, int b)
    {
        return s.field1 + s.field2 + b;
    }

    static double ppc64leHelloWorld()
    {
        DoubleStruct s = new DoubleStruct { field1 = 100.5, field2 = 200.5 };
        double result = ppc64leHelloWorldCallee(s, 50);
        return result;
    }

    static void Main(string[] args)
    {
        double retVal = ppc64leHelloWorld();
        Console.WriteLine("Test 17: Struct with double fields (16 bytes)");
        Console.WriteLine("Result: {0:F1}, Expected: 351.0", retVal);
        Console.WriteLine("Status: {0}", Math.Abs(retVal - 351.0) < 0.01 ? "PASS" : "FAIL");
    }
}

// Made with Bob