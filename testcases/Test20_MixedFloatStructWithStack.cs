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
        // PPC64LE ELFv2 ABI: FloatStruct is HFA (Homogeneous Float Aggregate)
        // HFA structs are passed in float registers
        // For first 8 FPRs, corresponding GPRs are shadowed/consumed
        // a: f1, r3 shadowed
        // s1: f2-f3 (HFA with 2 floats), r4-r5 shadowed
        // b: f4, r6 shadowed
        // c: r7 (first non-shadowed GPR)
        // d: f5, r8 shadowed
        // s2: f6-f7 (HFA with 2 floats), r9-r10 shadowed (r10 is last GPR)
        // e: f8 (8th FPR, no more GPR shadowing after this)
        // f: stack at SP+96 (GPRs exhausted at r10)
        // g: f9
        // h: f10
        // s3: f11-f12 (HFA with 2 floats)
        // i: stack at SP+100
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