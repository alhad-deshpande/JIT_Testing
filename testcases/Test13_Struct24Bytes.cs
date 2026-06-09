using System;
using System.Runtime.InteropServices;

// Test 13: Struct parameter passing - 24 bytes (3 slots, >16 bytes)
// Tests struct passed by value in three registers
[StructLayout(LayoutKind.Sequential)]
struct LargeStruct
{
    public long field1;
    public long field2;
    public long field3;
}

class Program
{
    static long ppc64leHelloWorldCallee(LargeStruct s, int b)
    {
        return s.field1 + s.field2 + s.field3 + b;
    }

    static long ppc64leHelloWorld()
    {
        LargeStruct s = new LargeStruct { field1 = 100, field2 = 200, field3 = 300 };
        long result = ppc64leHelloWorldCallee(s, 50);
        return result;
    }

    static void Main(string[] args)
    {
        long retVal = ppc64leHelloWorld();
        Console.WriteLine("Test 13: Struct parameter (24 bytes, >16)");
        Console.WriteLine("Result: {0}, Expected: 650", retVal);
        Console.WriteLine("Status: {0}", retVal == 650 ? "PASS" : "FAIL");
    }
}

// Made with Bob