using System;

// Test 7: Long (64-bit) arguments (10 args: 8 in registers, 2 on stack)
class Program
{
    static long ppc64leHelloWorldCallee(long a, long b, long c, long d, long e, long f, long g, long h,
                                        long i, long j)
    {
        return a + b + c + d + e + f + g + h + i + j;
    }

    static long ppc64leHelloWorld()
    {
        long sum = ppc64leHelloWorldCallee(100L, 200L, 300L, 400L, 500L, 600L, 700L, 800L, 900L, 1000L);
        return sum;
    }

    static void Main(string[] args)
    {
        long retVal = ppc64leHelloWorld();
        Console.WriteLine("Test 7: Long (64-bit) arguments (10 args)");
        Console.WriteLine("Result: {0}, Expected: 5500", retVal);
        Console.WriteLine("Status: {0}", retVal == 5500 ? "PASS" : "FAIL");
    }
}

// Made with Bob
