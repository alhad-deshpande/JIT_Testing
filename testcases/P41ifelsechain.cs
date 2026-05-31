using System;

class Program
{
    public static int ppc64leHelloWorld()
    {
        int x = 5;
        if (x < 3)
            return 1;
        else if (x < 7)
            return 2;
        else
            return 3;
    }

    static void Main()
    {
        int result = ppc64leHelloWorld();
        Console.WriteLine($"Answer: {result}");
    }
}

