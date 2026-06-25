using System;
using System.Runtime.InteropServices;

// Test 16: Struct with float fields (8 bytes)
// Tests struct with two 4-byte floats passed in a single register
[StructLayout(LayoutKind.Sequential)]
struct FloatStruct
{
    public float field1;
    public float field2;
}

class Program
{
    static float ppc64leHelloWorldCallee(FloatStruct s, int b)
    {
        // PPC64LE ELFv2 ABI: FloatStruct is HFA (Homogeneous Float Aggregate)
        // s: f1-f2 (HFA with 2 floats, each float uses one FPR slot)
        //    r3-r4 shadowed/consumed by f1-f2
        // b: r5 (r3-r4 were shadowed by the HFA)
        return s.field1 + s.field2 + b;
    }

    static float ppc64leHelloWorld()
    {
        FloatStruct s = new FloatStruct { field1 = 10.5f, field2 = 20.5f };
        float result = ppc64leHelloWorldCallee(s, 30);
        return result;
    }

    static void Main(string[] args)
    {
        float retVal = ppc64leHelloWorld();
        Console.WriteLine("Test 16: Struct with float fields (8 bytes)");
        Console.WriteLine("Result: {0:F1}, Expected: 61.0", retVal);
        Console.WriteLine("Status: {0}", Math.Abs(retVal - 61.0f) < 0.01f ? "PASS" : "FAIL");
    }
}

// Made with Bob