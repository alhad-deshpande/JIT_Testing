using System;
using System.Runtime.InteropServices;

// Test 18: Struct with mixed float and int fields (12 bytes)
// Tests struct with float and int fields passed in two registers
[StructLayout(LayoutKind.Sequential)]
struct MixedStruct
{
    public float field1;
    public int field2;
    public float field3;
}

class Program
{
    static float ppc64leHelloWorldCallee(MixedStruct s, int b)
    {
        return s.field1 + s.field2 + s.field3 + b;
    }

    static float ppc64leHelloWorld()
    {
        MixedStruct s = new MixedStruct { field1 = 10.5f, field2 = 20, field3 = 30.5f };
        float result = ppc64leHelloWorldCallee(s, 40);
        return result;
    }

    static void Main(string[] args)
    {
        float retVal = ppc64leHelloWorld();
        Console.WriteLine("Test 18: Struct with mixed float and int fields (12 bytes)");
        Console.WriteLine("Result: {0:F1}, Expected: 101.0", retVal);
        Console.WriteLine("Status: {0}", Math.Abs(retVal - 101.0f) < 0.01f ? "PASS" : "FAIL");
    }
}

// Made with Bob