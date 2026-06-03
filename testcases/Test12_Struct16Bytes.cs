using System;
using System.Runtime.InteropServices;

// Test 12: Struct parameter passing - 16 bytes (2 slots)
// Tests struct passed by value in two registers
[StructLayout(LayoutKind.Sequential)]
struct MediumStruct
{
    public long field1;
    public long field2;
}

class Program
{
    static long ppc64leHelloWorldCallee(MediumStruct s, int b)
    {
        return s.field1 + s.field2 + b;
    }

    static long ppc64leHelloWorld()
    {
        MediumStruct s = new MediumStruct { field1 = 100, field2 = 200 };
        long result = ppc64leHelloWorldCallee(s, 50);
        return result;
    }

    static void Main(string[] args)
    {
        long retVal = ppc64leHelloWorld();
        Console.WriteLine("Test 12: Struct parameter (16 bytes)");
        Console.WriteLine("Result: {0}, Expected: 350", retVal);
        Console.WriteLine("Status: {0}", retVal == 350 ? "PASS" : "FAIL");
    }
}

// Made with Bob