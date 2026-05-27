using System;

// Test 9: Boolean, byte, and short arguments (promoted to int in registers)
class Program
{
    static int ppc64leHelloWorldCallee(bool a, byte b, short c, int d)
    {
        int aVal = a ? 1 : 0;
        return aVal + b + c + d;
    }

    static int ppc64leHelloWorld()
    {
        int sum = ppc64leHelloWorldCallee(true, 10, 100, 1000);
        return sum;
    }

    static void Main(string[] args)
    {
        int retVal = ppc64leHelloWorld();
        Console.WriteLine("Test 9: Boolean, byte, and short arguments (4 args)");
        Console.WriteLine("Result: {0}, Expected: 1111", retVal);
        Console.WriteLine("Status: {0}", retVal == 1111 ? "PASS" : "FAIL");
    }
}

// Made with Bob
