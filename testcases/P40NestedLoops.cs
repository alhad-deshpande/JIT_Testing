using System;

class Program
{
    public static int ppc64leHelloWorld()
    {
        int sum = 0;
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                sum++;
            }
        }
        return sum;
    }

    static void Main()
    {
        int result = ppc64leHelloWorld();
        Console.WriteLine($"Answer: {result}");
    }
}

