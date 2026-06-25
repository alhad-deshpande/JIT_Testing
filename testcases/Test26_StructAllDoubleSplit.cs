using System;
using System.Runtime.InteropServices;

// Test 26: Struct with all doubles split between FPR and stack
// Homogeneous double struct (HFA) passed in FPRs, splits when FPRs exhausted
[StructLayout(LayoutKind.Sequential)]
struct AllDoubleStruct
{
    public double field1;
    public double field2;
    public double field3;
}

class Program
{
    static double ppc64leHelloWorldCallee(double a, double b, double c, double d, 
                                          double e, double f, double g, double h,
                                          double i, double j, double k, double l,
                                          AllDoubleStruct s, double m)
    {
        // For first 8 FPRs, corresponding GPRs are shadowed/consumed
        // a-h: f1-f8 (8 FPRs, r3-r10 shadowed - all GPRs exhausted)
        // i-l: f9-f12 (no more GPR shadowing)
        // s: all-double struct (HFA) needs 3 FPR slots (24 bytes = 3 doubles)
        //    - field1 in f13 (last FPR, only 1 slot available)
        //    - field2 on stack at SP+96 (SPLIT!)
        //    - field3 on stack at SP+104
        // m: stack at SP+112 (FPRs exhausted)
        return a + b + c + d + e + f + g + h + i + j + k + l + 
               s.field1 + s.field2 + s.field3 + m;
    }

    static double ppc64leHelloWorld()
    {
        AllDoubleStruct s = new AllDoubleStruct { field1 = 13.0, field2 = 14.0, field3 = 15.0 };
        double result = ppc64leHelloWorldCallee(1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0,
                                                 9.0, 10.0, 11.0, 12.0, s, 16.0);
        return result;
    }

    static void Main(string[] args)
    {
        double retVal = ppc64leHelloWorld();
        Console.WriteLine("Test 26: Struct with all doubles (HFA) split between FPR and stack");
        Console.WriteLine("Result: {0:F1}, Expected: 136.0", retVal);
        Console.WriteLine("Status: {0}", Math.Abs(retVal - 136.0) < 0.01 ? "PASS" : "FAIL");
    }
}

// Made with Bob