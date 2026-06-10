using System;
using System.Runtime.InteropServices;

// Test 20: Mixed float/double/struct arguments requiring stack
// Tests combination of scalar floats/doubles and structs with float fields
// Exhausts both GPR and FPR registers, forcing stack usage
[StructLayout(LayoutKind.Sequential)]
struct FloatStruct
{
    public float field1;
    public float field2;
}

class Program
{
    static double ppc64leHelloWorldCallee(float a, FloatStruct s1, double b, int c,
                                          float d, FloatStruct s2, double e, int f,
                                          float g, double h, FloatStruct s3, int i)
    {
        // a: f1, r3 consumed
        // s1: r4 (8 bytes struct)
        // b: f2, r5 consumed
        // c: r6
        // d: f3, r7 consumed
        // s2: r8 (8 bytes struct)
        // e: f4, r9 consumed
        // f: r10
        // g: f5, r11 consumed (last GPR)
        // h: f6, r12 consumed (GPRs exhausted, but r12 available)
        // s3: needs r13 but exhausted -> goes to stack at SP+96
        // i: goes to stack at SP+104
        return a + s1.field1 + s1.field2 + b + c + d + s2.field1 + s2.field2 + 
               e + f + g + h + s3.field1 + s3.field2 + i;
    }

    static double ppc64leHelloWorld()
    {
        FloatStruct s1 = new FloatStruct { field1 = 2.0f, field2 = 3.0f };
        FloatStruct s2 = new FloatStruct { field1 = 7.0f, field2 = 8.0f };
        FloatStruct s3 = new FloatStruct { field1 = 13.0f, field2 = 14.0f };
        
        double result = ppc64leHelloWorldCallee(1.0f, s1, 4.0, 5, 6.0f, s2, 9.0, 10, 11.0f, 12.0, s3, 15);
        return result;
    }

    static void Main(string[] args)
    {
        double retVal = ppc64leHelloWorld();
        Console.WriteLine("Test 20: Mixed float/double/struct arguments with stack");
        Console.WriteLine("Result: {0:F1}, Expected: 120.0", retVal);
        Console.WriteLine("Status: {0}", Math.Abs(retVal - 120.0) < 0.01 ? "PASS" : "FAIL");
    }
}

// Made with Bob