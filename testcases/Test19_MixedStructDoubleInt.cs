using System;
using System.Runtime.InteropServices;

// Test 19: Struct with mixed double and int fields (24 bytes)
// Tests struct with double and int fields passed in three registers
[StructLayout(LayoutKind.Sequential)]
struct MixedDoubleStruct
{
    public double field1;
    public int field2;
    public int field3;
    public double field4;
}

class Program
{
    static double ppc64leHelloWorldCallee(MixedDoubleStruct s, int b)
    {
        return s.field1 + s.field2 + s.field3 + s.field4 + b;
    }

    static double ppc64leHelloWorld()
    {
        MixedDoubleStruct s = new MixedDoubleStruct { field1 = 100.5, field2 = 20, field3 = 30, field4 = 200.5 };
        double result = ppc64leHelloWorldCallee(s, 50);
        return result;
    }

    static void Main(string[] args)
    {
        double retVal = ppc64leHelloWorld();
        Console.WriteLine("Test 19: Struct with mixed double and int fields (24 bytes)");
        Console.WriteLine("Result: {0:F1}, Expected: 401.0", retVal);
        Console.WriteLine("Status: {0}", Math.Abs(retVal - 401.0) < 0.01 ? "PASS" : "FAIL");
    }
}

// Made with Bob