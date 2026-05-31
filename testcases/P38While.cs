using System;

class Program
{
    public static int ppc64leHelloWorld()
    {
        int i = 0;
        int sum = 0;
        while (i < 10)
        {
            sum += i;
            i++;
        }
        return sum;
    }

    static void Main()
    {
        int result = ppc64leHelloWorld();
        Console.WriteLine($"Answer: {result}");
    }
}

