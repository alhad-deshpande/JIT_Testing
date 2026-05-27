using System;

// Test 1: Integer arguments in registers (8 args in r3-r10)
class Program
{
    static int ppc64leHelloWorldCallee(int a, int b, int c, int d, int e, int f, int g, int h)
    {
        return a + b + c + d + e + f + g + h;
    }

    static int ppc64leHelloWorld()
    {
        int sum = ppc64leHelloWorldCallee(1, 2, 3, 4, 5, 6, 7, 8);
        return sum;
    }

    static void Main(string[] args)
    {
        int retVal = ppc64leHelloWorld();
        Console.WriteLine("Answer: {0}, Expected: 36", retVal);
        Console.WriteLine("Status: {0}", retVal == 36 ? "PASS" : "FAIL");
    }
}

// Made with Bob
