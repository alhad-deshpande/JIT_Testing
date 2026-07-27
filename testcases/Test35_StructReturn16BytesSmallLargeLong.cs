using System;
using System.Runtime.InteropServices;

// Test 35: Struct parameter passing with 16-byte struct return
// Tests one small struct and one large struct, both using long fields
[StructLayout(LayoutKind.Sequential)]
struct SmallStruct
{
    public long field1;
}

[StructLayout(LayoutKind.Sequential)]
struct LargeStruct
{
    public long field1;
    public long field2;
}

[StructLayout(LayoutKind.Sequential)]
struct ReturnStruct
{
    public long field1;
    public long field2;
}

class Program
{
    static ReturnStruct ppc64leHelloWorldCallee(SmallStruct small, LargeStruct large)
    {
        return new ReturnStruct
        {
            field1 = small.field1 + large.field1,
            field2 = large.field2
        };
    }

    static ReturnStruct ppc64leHelloWorld()
    {
        SmallStruct small = new SmallStruct { field1 = 100 };
        LargeStruct large = new LargeStruct { field1 = 200, field2 = 300 };
        ReturnStruct result = ppc64leHelloWorldCallee(small, large);
        return result;
    }

    static void Main(string[] args)
    {
        ReturnStruct retVal = ppc64leHelloWorld();
        Console.WriteLine("Test 35: Struct return (16 bytes) with small + large long structs");
        Console.WriteLine("field1 = {0}", retVal.field1);
        Console.WriteLine("field2 = {0}", retVal.field2);
        Console.WriteLine("Status: {0}", retVal.field1 == 300 && retVal.field2 == 300 ? "PASS" : "FAIL");
    }
}

// Made with Bob
