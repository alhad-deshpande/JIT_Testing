using System;
using System.Runtime.InteropServices;

// Test 11: Struct parameter passing - 8 bytes (1 slot)
// Tests struct passed by value in a single register
[StructLayout(LayoutKind.Sequential)]
struct SmallStruct
{
    public int field1;
    public int field2;
}

class Program
{
    static int ppc64leHelloWorldCallee(SmallStruct s, int b)
    {
        return s.field1 + s.field2 + b;
    }

    static int ppc64leHelloWorld()
    {
        SmallStruct s = new SmallStruct { field1 = 10, field2 = 20 };
        int result = ppc64leHelloWorldCallee(s, 30);
        return result;
    }

    static void Main(string[] args)
    {
        int retVal = ppc64leHelloWorld();
        Console.WriteLine("Test 11: Struct parameter (8 bytes)");
        Console.WriteLine("Result: {0}, Expected: 60", retVal);
        Console.WriteLine("Status: {0}", retVal == 60 ? "PASS" : "FAIL");
    }
}

// Made with Bob