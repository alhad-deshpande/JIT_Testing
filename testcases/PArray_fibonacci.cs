using System;

class Program
{
    public static int ppc64leHelloWorld()
    {
        Console.WriteLine("Test: Fibonacci Pattern");
        int[] fib = new int[10];
        fib[0] = 1;
        fib[1] = 1;
        for (int i = 2; i < fib.Length; i++)
        {
            fib[i] = fib[i-1] + fib[i-2];
        }
        Console.WriteLine($"Fib[9]: {fib[9]}, Expected: 55");
        return (fib[9] == 55) ? 10 : 0;
    }

    static void Main()
    {
        int result = ppc64leHelloWorld();
        Console.WriteLine($"Answer: {result}");
    }
}

