using System;

// Test 2: Integer arguments with stack overflow (12 args: 8 in registers, 4 on stack)
class Program
{
    static int ppc64leHelloWorldCallee(int a, int b, int c, int d, int e, int f, int g, int h, 
                                        int i, int j, int k, int l)
    {
        return a + b + c + d + e + f + g + h + i + j + k + l;
    }

    static int ppc64leHelloWorld()
    {
        int sum = ppc64leHelloWorldCallee(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12);
        return sum;
    }

    static void Main(string[] args)
    {
        int retVal = ppc64leHelloWorld();
        Console.WriteLine("Test 2: Integer arguments with stack (12 args)");
        Console.WriteLine("Result: {0}, Expected: 78", retVal);
        Console.WriteLine("Status: {0}", retVal == 78 ? "PASS" : "FAIL");
    }
}

// Made with Bob
