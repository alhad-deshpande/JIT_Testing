using System;

class Program
{
    public static int ppc64leHelloWorld()
    {
        int i = 0;
        int sum = 0;
        do
        {
            sum += i;
            i++;
        } while (i < 10);
        return sum;
    }

    static void Main()
    {
        int result = ppc64leHelloWorld();
        Console.WriteLine($"Answer: {result}");
    }
}

