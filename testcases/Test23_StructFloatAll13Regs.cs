using System;
using System.Runtime.InteropServices;

// Test 23: Struct with floats using all 13 FPRs and stack
// Tests structs containing float fields that exhaust all floating-point registers
[StructLayout(LayoutKind.Sequential)]
struct FloatStruct
{
    public float field1;
    public float field2;
}

class Program
{
    static float ppc64leHelloWorldCallee(FloatStruct s1, float a, FloatStruct s2, double b,
                                         FloatStruct s3, float c, FloatStruct s4, double d,
                                         FloatStruct s5, float e, FloatStruct s6, double f,
                                         float g, FloatStruct s7)
    {
        // PPC64LE ELFv2 ABI: FloatStruct is HFA (Homogeneous Float Aggregate)
        // HFA structs are passed in float registers
        // For first 8 FPRs, corresponding GPRs are shadowed/consumed
        // s1: f1-f2 (HFA with 2 floats), r3-r4 shadowed
        // a: f3, r5 shadowed
        // s2: f4-f5 (HFA with 2 floats), r6-r7 shadowed
        // b: f6, r8 shadowed
        // s3: f7-f8 (HFA with 2 floats), r9-r10 shadowed (8th FPR, last GPR)
        // c: f9 (no more GPR shadowing)
        // s4: f10-f11 (HFA with 2 floats)
        // d: f12
        // s5: f13 + stack at SP+96 (HFA split: field1 in f13, field2 on stack)
        // e: stack at SP+100 (FPRs exhausted)
        // s6: stack at SP+104 (HFA on stack, 8 bytes)
        // f: stack at SP+112
        // g: stack at SP+120
        // s7: stack at SP+124 (HFA on stack, 8 bytes)
        return s1.field1 + s1.field2 + a + s2.field1 + s2.field2 + (float)b +
               s3.field1 + s3.field2 + c + s4.field1 + s4.field2 + (float)d +
               s5.field1 + s5.field2 + e + s6.field1 + s6.field2 + (float)f + g +
               s7.field1 + s7.field2;
    }

    static float ppc64leHelloWorld()
    {
        FloatStruct s1 = new FloatStruct { field1 = 1.0f, field2 = 2.0f };
        FloatStruct s2 = new FloatStruct { field1 = 4.0f, field2 = 5.0f };
        FloatStruct s3 = new FloatStruct { field1 = 7.0f, field2 = 8.0f };
        FloatStruct s4 = new FloatStruct { field1 = 10.0f, field2 = 11.0f };
        FloatStruct s5 = new FloatStruct { field1 = 13.0f, field2 = 14.0f };
        FloatStruct s6 = new FloatStruct { field1 = 16.0f, field2 = 17.0f };
        FloatStruct s7 = new FloatStruct { field1 = 20.0f, field2 = 21.0f };
        
        float result = ppc64leHelloWorldCallee(s1, 3.0f, s2, 6.0, s3, 9.0f, s4, 12.0,
                                                s5, 15.0f, s6, 18.0, 19.0f, s7);
        return result;
    }

    static void Main(string[] args)
    {
        float retVal = ppc64leHelloWorld();
        Console.WriteLine("Test 23: Struct with floats using all 13 FPRs and stack");
        Console.WriteLine("Result: {0:F1}, Expected: 231.0", retVal);
        Console.WriteLine("Status: {0}", Math.Abs(retVal - 231.0f) < 0.01f ? "PASS" : "FAIL");
    }
}

// Made with Bob