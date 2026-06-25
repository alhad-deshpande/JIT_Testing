using System;
using System.Runtime.InteropServices;

// Test 25: Struct with mixed float/int fields split between GPR and stack
// Mixed struct passed in GPRs, splits when GPRs exhausted
[StructLayout(LayoutKind.Sequential)]
struct MixedFloatStruct
{
    public float field1;
    public int field2;
    public float field3;
    public int field4;
}

class Program
{
    static float ppc64leHelloWorldCallee(float a, float b, float c, float d, 
                                         float e, float f, float g,
                                         MixedFloatStruct s, float h)
    {
        // For first 8 FPRs, corresponding GPRs are shadowed/consumed
        // a-g: f1-f7, r3-r9 shadowed (7 GPRs consumed)
        // s: mixed struct (NOT HFA) passed in GPRs, needs 2 slots (16 bytes)
        //    - first 8 bytes (field1, field2) in r10 (last available GPR)
        //    - second 8 bytes (field3, field4) on stack at SP+96 (SPLIT!)
        // h: f8 (8th FPR, no more GPR shadowing, but r10 already used)
        //    Stack at SP+104 for any additional GPR needs
        return a + b + c + d + e + f + g + s.field1 + s.field2 + s.field3 + s.field4 + h;
    }

    static float ppc64leHelloWorld()
    {
        MixedFloatStruct s = new MixedFloatStruct { field1 = 8.0f, field2 = 9, field3 = 10.0f, field4 = 11 };
        float result = ppc64leHelloWorldCallee(1.0f, 2.0f, 3.0f, 4.0f, 5.0f, 6.0f, 7.0f, s, 12.0f);
        return result;
    }

    static void Main(string[] args)
    {
        float retVal = ppc64leHelloWorld();
        Console.WriteLine("Test 25: Struct with mixed float/int fields split between GPR and stack");
        Console.WriteLine("Result: {0:F1}, Expected: 78.0", retVal);
        Console.WriteLine("Status: {0}", Math.Abs(retVal - 78.0f) < 0.01f ? "PASS" : "FAIL");
    }
}

// Made with Bob