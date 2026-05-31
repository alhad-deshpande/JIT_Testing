using System;

class Program
{
    public static int ppc64leHelloWorld()
    {
        int sum = 0;
        for (int i = 0; i < 10; i++)
        {
            sum += i;
        }
        return sum;
    }

    static void Main()
    {
        int result = ppc64leHelloWorld();
        Console.WriteLine($"Answer: {result}");
    }
}

