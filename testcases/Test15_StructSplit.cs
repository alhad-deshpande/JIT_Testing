using System;
using System.Runtime.InteropServices;

// Test 15: Struct parameter split between register and stack
// Tests struct that starts in last register and continues on stack
[StructLayout(LayoutKind.Sequential)]
struct MediumStruct
{
    public long field1;
    public long field2;
}

class Program
{
    static long ppc64leHelloWorldCallee(int a, int b, int c, int d, int e, int f, int g,
                                         MediumStruct s, int j)
    {
        return a + b + c + d + e + f + g + s.field1 + s.field2 + j;
    }

    static long ppc64leHelloWorld()
    {
        MediumStruct s = new MediumStruct { field1 = 80, field2 = 90 };
        long result = ppc64leHelloWorldCallee(1, 2, 3, 4, 5, 6, 7, s, 10);
        return result;
    }

    static void Main(string[] args)
    {
        long retVal = ppc64leHelloWorld();
        Console.WriteLine("Test 15: Struct split between register and stack");
        Console.WriteLine("Result: {0}, Expected: 208", retVal);
        Console.WriteLine("Status: {0}", retVal == 208 ? "PASS" : "FAIL");
    }
}

// Made with Bob