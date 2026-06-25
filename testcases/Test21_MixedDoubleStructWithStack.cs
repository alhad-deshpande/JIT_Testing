using System;
using System.Runtime.InteropServices;

// Test 21: Mixed double/struct with doubles requiring stack
// Tests combination of scalar doubles and structs with double fields
// Exhausts GPR registers, forcing stack usage for larger structs
[StructLayout(LayoutKind.Sequential)]
struct DoubleStruct
{
    public double field1;
    public double field2;
}

class Program
{
    static double ppc64leHelloWorldCallee(double a, int b, DoubleStruct s1, double c,
                                          int d, DoubleStruct s2, double e, int f,
                                          DoubleStruct s3)
    {
        // PPC64LE ELFv2 ABI: DoubleStruct is HFA (Homogeneous Float Aggregate)
        // HFA structs are passed in float registers
        // For first 8 FPRs, corresponding GPRs are shadowed/consumed
        // a: f1, r3 shadowed
        // b: r4 (first non-shadowed GPR)
        // s1: f2-f3 (HFA with 2 doubles), r5-r6 shadowed
        // c: f4, r7 shadowed
        // d: r8 (next non-shadowed GPR)
        // s2: f5-f6 (HFA with 2 doubles), r9-r10 shadowed (r10 is last GPR)
        // e: f7 (7th FPR, still shadows but GPRs already exhausted)
        // f: stack at SP+96 (GPRs exhausted at r10)
        // s3: f8-f9 (HFA with 2 doubles, 8th FPR stops shadowing)
        return a + b + s1.field1 + s1.field2 + c + d + s2.field1 + s2.field2 + e + f +
               s3.field1 + s3.field2;
    }

    static double ppc64leHelloWorld()
    {
        DoubleStruct s1 = new DoubleStruct { field1 = 3.0, field2 = 4.0 };
        DoubleStruct s2 = new DoubleStruct { field1 = 7.0, field2 = 8.0 };
        DoubleStruct s3 = new DoubleStruct { field1 = 11.0, field2 = 12.0 };
        
        double result = ppc64leHelloWorldCallee(1.0, 2, s1, 5.0, 6, s2, 9.0, 10, s3);
        return result;
    }

    static void Main(string[] args)
    {
        double retVal = ppc64leHelloWorld();
        Console.WriteLine("Test 21: Mixed double/struct with doubles requiring stack");
        Console.WriteLine("Result: {0:F1}, Expected: 78.0", retVal);
        Console.WriteLine("Status: {0}", Math.Abs(retVal - 78.0) < 0.01 ? "PASS" : "FAIL");
    }
}

// Made with Bob