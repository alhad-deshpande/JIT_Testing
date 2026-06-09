using System;
using System.Runtime.InteropServices;

// Test 14: Struct parameter passing on stack
// Tests struct passed on stack when registers are exhausted
[StructLayout(LayoutKind.Sequential)]
struct SmallStruct
{
    public int field1;
    public int field2;
}

class Program
{
    static int ppc64leHelloWorldCallee(int a, int b, int c, int d, int e, int f, int g, int h,
                                        SmallStruct s, int j)
    {
        return a + b + c + d + e + f + g + h + s.field1 + s.field2 + j;
    }

    static int ppc64leHelloWorld()
    {
        SmallStruct s = new SmallStruct { field1 = 90, field2 = 100 };
        int result = ppc64leHelloWorldCallee(1, 2, 3, 4, 5, 6, 7, 8, s, 10);
        return result;
    }

    static void Main(string[] args)
    {
        int retVal = ppc64leHelloWorld();
        Console.WriteLine("Test 14: Struct parameter on stack");
        Console.WriteLine("Result: {0}, Expected: 236", retVal);
        Console.WriteLine("Status: {0}", retVal == 236 ? "PASS" : "FAIL");
    }
}

// Made with Bob