using System;
using System.Runtime.InteropServices;

// Test 24: Struct with all floats split between FPR and stack
// Homogeneous float struct (HFA) passed in FPRs, splits when FPRs exhausted
[StructLayout(LayoutKind.Sequential)]
struct AllFloatStruct
{
    public float field1;
    public float field2;
    public float field3;
    public float field4;
}

class Program
{
    static float ppc64leHelloWorldCallee(float a, float b, float c, float d, 
                                         float e, float f, float g, float h,
                                         float i, float j, float k, float l,
                                         AllFloatStruct s, float m)
    {
        // For first 8 FPRs, corresponding GPRs are shadowed/consumed
        // a-h: f1-f8 (8 FPRs, r3-r10 shadowed - all GPRs exhausted)
        // i-l: f9-f12 (no more GPR shadowing)
        // s: all-float struct (HFA) needs 4 FPR slots (16 bytes = 4 floats)
        //    - field1 in f13 (last FPR, only 1 slot available)
        //    - field2-field4 on stack at SP+96 (SPLIT!)
        // m: stack at SP+108 (FPRs exhausted)
        return a + b + c + d + e + f + g + h + i + j + k + l + 
               s.field1 + s.field2 + s.field3 + s.field4 + m;
    }

    static float ppc64leHelloWorld()
    {
        AllFloatStruct s = new AllFloatStruct { field1 = 13.0f, field2 = 14.0f, field3 = 15.0f, field4 = 16.0f };
        float result = ppc64leHelloWorldCallee(1.0f, 2.0f, 3.0f, 4.0f, 5.0f, 6.0f, 7.0f, 8.0f,
                                                9.0f, 10.0f, 11.0f, 12.0f, s, 17.0f);
        return result;
    }

    static void Main(string[] args)
    {
        float retVal = ppc64leHelloWorld();
        Console.WriteLine("Test 24: Struct with all floats (HFA) split between FPR and stack");
        Console.WriteLine("Result: {0:F1}, Expected: 153.0", retVal);
        Console.WriteLine("Status: {0}", Math.Abs(retVal - 153.0f) < 0.01f ? "PASS" : "FAIL");
    }
}

// Made with Bob