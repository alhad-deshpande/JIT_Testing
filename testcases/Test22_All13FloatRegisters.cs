using System;

// Test 22: All 13 floating-point registers with mixed float/double
// Tests that all f1-f13 registers are used with a mix of float and double types
// Also tests that 14th float argument goes to stack
class Program
{
    static double ppc64leHelloWorldCallee(float a, double b, float c, double d,
                                          float e, double f, float g, double h,
                                          float i, double j, float k, double l,
                                          float m, double n)
    {
        // For first 8 FPRs, corresponding GPRs are shadowed/consumed
        // a: f1, r3 shadowed
        // b: f2, r4 shadowed
        // c: f3, r5 shadowed
        // d: f4, r6 shadowed
        // e: f5, r7 shadowed
        // f: f6, r8 shadowed
        // g: f7, r9 shadowed
        // h: f8, r10 shadowed (8th FPR, last GPR exhausted)
        // i: f9 (no more GPR shadowing after f8)
        // j: f10
        // k: f11
        // l: f12
        // m: f13 (last FPR)
        // n: stack at SP+96 (FPRs exhausted)
        return a + b + c + d + e + f + g + h + i + j + k + l + m + n;
    }

    static double ppc64leHelloWorld()
    {
        double result = ppc64leHelloWorldCallee(1.0f, 2.0, 3.0f, 4.0, 5.0f, 6.0, 7.0f, 8.0,
                                                 9.0f, 10.0, 11.0f, 12.0, 13.0f, 14.0);
        return result;
    }

    static void Main(string[] args)
    {
        double retVal = ppc64leHelloWorld();
        Console.WriteLine("Test 22: All 13 floating-point registers with mixed float/double");
        Console.WriteLine("Result: {0:F1}, Expected: 105.0", retVal);
        Console.WriteLine("Status: {0}", Math.Abs(retVal - 105.0) < 0.01 ? "PASS" : "FAIL");
    }
}

// Made with Bob